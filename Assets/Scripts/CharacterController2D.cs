using System;
using UnityEngine;

/// <summary>
/// MonoBehaviour: Celeste-стиль платформер на основе AABB vs Grid2D&lt;CellData&gt;.
/// Кастомная физика без Rigidbody2D. Вызови Init(board) из GameManager.
/// </summary>
[RequireComponent(typeof(CharacterView))]
public class CharacterController2D : MonoBehaviour
{
    // ─── Настройки (Inspector) ────────────────────────────────────────────────

    [Header("Движение")]
    [SerializeField] private float _maxSpeed    = 6f;
    [SerializeField] private float _acceleration = 40f;
    [SerializeField] private float _deceleration = 50f;

    [Header("Прыжок")]
    [SerializeField] private float _jumpForce        = 12f;
    [SerializeField] private float _gravity          = -30f;
    [SerializeField] private float _coyoteTime       = 0.12f;
    [SerializeField] private float _jumpBufferTime   = 0.12f;
    [SerializeField] private float _variableJumpMult = 0.45f;

    [Header("Wall-Slide / Wall-Jump")]
    [SerializeField] private float   _wallSlideGravMult = 0.3f;
    [SerializeField] private Vector2 _wallJumpForce     = new Vector2(8f, 12f);
    [SerializeField] private float   _wallJumpLockTime  = 0.15f;

    [Header("Удар")]
    [SerializeField] private KeyCode _keyAttackLeft  = KeyCode.J;
    [SerializeField] private KeyCode _keyAttackRight = KeyCode.L;
    [SerializeField] private float   _attackCooldown = 0.3f;

    [Header("Ввод")]
    [SerializeField] private KeyCode _keyLeft  = KeyCode.A;
    [SerializeField] private KeyCode _keyRight = KeyCode.D;
    [SerializeField] private KeyCode _keyJump  = KeyCode.W;

    [Header("Размер коллайдера (в ячейках)")]
    [SerializeField] private float _colliderWidthCells  = 0.8f;
    [SerializeField] private float _colliderHeightCells = 1.8f;

    // ─── Публичный доступ ─────────────────────────────────────────────────────

    public Vector2 Velocity      => _velocity;
    public bool    IsGrounded    => _grounded;
    public bool    IsWallSliding => _wallSliding;

    /// <summary>Удар по ячейке: x, y, направление (-1/+1).</summary>
    public event Action<int, int, int> OnAttack;

    // ─── Private ──────────────────────────────────────────────────────────────

    private TetrisBoard   _board;
    private CharacterView _view;

    private Vector2 _velocity;
    private bool    _grounded;
    private bool    _wallSliding;
    private int     _wallDir;

    private float _coyoteTimer;
    private float _jumpBufferTimer;
    private float _wallJumpLockTimer;
    private float _attackCooldownTimer;

    // ─── Инициализация ────────────────────────────────────────────────────────

    public void Init(TetrisBoard board)
    {
        _board = board;
        _view  = GetComponent<CharacterView>();
    }

    // ─── Unity Update ─────────────────────────────────────────────────────────

    private void Update()
    {
        if (_board == null) return;

        float dt = Time.deltaTime;
        _attackCooldownTimer -= dt;
        _wallJumpLockTimer   -= dt;

        HandleAttack();
        HandleHorizontal(dt);
        HandleJumpInput();
        ApplyGravity(dt);
        MoveAndCollide(dt);
        _view.UpdateAnimations(_velocity.x, _grounded, _wallSliding);
    }

    // ─── Удар ─────────────────────────────────────────────────────────────────

    private void HandleAttack()
    {
        if (_attackCooldownTimer > 0f) return;

        int dir = 0;
        if (Input.GetKeyDown(_keyAttackLeft))  dir = -1;
        if (Input.GetKeyDown(_keyAttackRight)) dir = +1;
        if (dir == 0) return;

        _attackCooldownTimer = _attackCooldown;
        _view.PlayAttack();

        _board.Grid.WorldToCell(transform.position, out int cx, out int cy);
        OnAttack?.Invoke(cx + dir, cy, dir);
    }

    // ─── Горизонтальное движение ──────────────────────────────────────────────

    private void HandleHorizontal(float dt)
    {
        if (_wallJumpLockTimer > 0f) return;

        float input = 0f;
        if (Input.GetKey(_keyLeft))  input -= 1f;
        if (Input.GetKey(_keyRight)) input += 1f;

        if (input != 0f)
            _velocity.x = Mathf.MoveTowards(_velocity.x, input * _maxSpeed, _acceleration * dt);
        else
            _velocity.x = Mathf.MoveTowards(_velocity.x, 0f, _deceleration * dt);
    }

    // ─── Прыжок (Coyote Time + Jump Buffer + Variable Height) ─────────────────

    private void HandleJumpInput()
    {
        // Coyote time
        if (_grounded)
            _coyoteTimer = _coyoteTime;
        else
            _coyoteTimer -= Time.deltaTime;

        // Jump buffer
        if (Input.GetKeyDown(_keyJump))
            _jumpBufferTimer = _jumpBufferTime;
        else
            _jumpBufferTimer -= Time.deltaTime;

        // Variable jump: обрезать скорость при отпускании кнопки
        if (Input.GetKeyUp(_keyJump) && _velocity.y > 0f)
            _velocity.y *= _variableJumpMult;

        // Попытка прыжка
        if (_jumpBufferTimer > 0f)
        {
            if (_wallSliding)
            {
                // Wall-jump: оттолкнуться от стены
                _velocity.x        = -_wallDir * _wallJumpForce.x;
                _velocity.y        =  _wallJumpForce.y;
                _wallJumpLockTimer = _wallJumpLockTime;
                _coyoteTimer       = 0f;
                _jumpBufferTimer   = 0f;
                _grounded          = false;
                _wallSliding       = false;
                _view.PlayJump();
            }
            else if (_coyoteTimer > 0f)
            {
                _velocity.y      = _jumpForce;
                _coyoteTimer     = 0f;
                _jumpBufferTimer = 0f;
                _grounded        = false;
                _view.PlayJump();
            }
        }
    }

    // ─── Гравитация ───────────────────────────────────────────────────────────

    private void ApplyGravity(float dt)
    {
        float grav = _gravity;
        if (_wallSliding && _velocity.y < 0f)
            grav *= _wallSlideGravMult;
        _velocity.y += grav * dt;
    }

    // ─── Движение + коллизия (AABB vs Grid) ──────────────────────────────────

    private void MoveAndCollide(float dt)
    {
        Vector3 origin3 = transform.position;
        Vector2 pos     = origin3;
        float   cs      = _board.Grid.CellSize;
        float   halfW   = _colliderWidthCells  * cs * 0.5f;
        float   charH   = _colliderHeightCells * cs;

        // ── Горизонталь ───────────────────────────────────────────────────────
        pos.x      += _velocity.x * dt;
        _wallSliding = false;
        _wallDir     = 0;

        if (_velocity.x > 0f && SampleWall(pos.x + halfW, pos.y, charH, out int wallCellX))
        {
            pos.x       = _board.Grid.GetCellOrigin(wallCellX, 0).x - halfW - 0.002f;
            _velocity.x = 0f;
            if (!_grounded && _velocity.y < 0f) { _wallSliding = true; _wallDir = 1; }
        }
        else if (_velocity.x < 0f && SampleWall(pos.x - halfW, pos.y, charH, out wallCellX))
        {
            pos.x       = _board.Grid.GetCellOrigin(wallCellX + 1, 0).x + halfW + 0.002f;
            _velocity.x = 0f;
            if (!_grounded && _velocity.y < 0f) { _wallSliding = true; _wallDir = -1; }
        }

        // ── Вертикаль ─────────────────────────────────────────────────────────
        pos.y    += _velocity.y * dt;
        _grounded = false;

        if (_velocity.y <= 0f)
        {
            if (pos.y <= 0f)
            {
                // Нижняя граница доски
                pos.y       = 0f;
                _velocity.y = 0f;
                _grounded   = true;
            }
            else if (SampleFloor(pos.x, pos.y, halfW, out int floorCellY))
            {
                pos.y       = _board.Grid.GetCellOrigin(0, floorCellY + 1).y;
                _velocity.y = 0f;
                _grounded   = true;
            }
        }
        else // движение вверх
        {
            if (SampleCeiling(pos.x, pos.y + charH, halfW, out int ceilCellY))
            {
                pos.y       = _board.Grid.GetCellOrigin(0, ceilCellY).y - charH - 0.002f;
                _velocity.y = 0f;
            }
        }

        // Зажать по горизонтальным границам доски
        float boardLeft  = _board.Grid.GetCellOrigin(0, 0).x + halfW;
        float boardRight = _board.Grid.GetCellOrigin(_board.Width, 0).x - halfW;
        pos.x = Mathf.Clamp(pos.x, boardLeft, boardRight);

        transform.position = new Vector3(pos.x, pos.y, origin3.z);
        _view.SetFacing(_velocity.x);
    }

    // ─── Вспомогательные: sampling ────────────────────────────────────────────

    /// <summary>Проверить вертикальное ребро (x=edgeX) на наличие блока. Возвращает X ячейки.</summary>
    private bool SampleWall(float edgeX, float botY, float charH, out int cellX)
    {
        cellX = 0;
        float[] tSamples = { 0.25f, 0.75f };
        foreach (float t in tSamples)
        {
            float testY = botY + charH * t;
            _board.Grid.WorldToCell(new Vector3(edgeX, testY), out int cx, out int cy);
            if (_board.Grid.IsValid(cx, cy) && _board.IsCellOccupied(cx, cy))
            {
                cellX = cx;
                return true;
            }
        }
        return false;
    }

    /// <summary>Проверить горизонтальное ребро снизу (y=botY). Возвращает Y ячейки.</summary>
    private bool SampleFloor(float centerX, float botY, float halfW, out int cellY)
    {
        cellY = 0;
        float testY = botY - 0.002f;
        float[] xSamples = { centerX - halfW * 0.9f, centerX, centerX + halfW * 0.9f };
        foreach (float testX in xSamples)
        {
            _board.Grid.WorldToCell(new Vector3(testX, testY), out int cx, out int cy);
            if (_board.Grid.IsValid(cx, cy) && _board.IsCellOccupied(cx, cy))
            {
                cellY = cy;
                return true;
            }
        }
        return false;
    }

    /// <summary>Проверить горизонтальное ребро сверху (y=topY). Возвращает Y ячейки.</summary>
    private bool SampleCeiling(float centerX, float topY, float halfW, out int cellY)
    {
        cellY = 0;
        float testY = topY + 0.002f;
        float[] xSamples = { centerX - halfW * 0.9f, centerX, centerX + halfW * 0.9f };
        foreach (float testX in xSamples)
        {
            _board.Grid.WorldToCell(new Vector3(testX, testY), out int cx, out int cy);
            if (_board.Grid.IsValid(cx, cy) && _board.IsCellOccupied(cx, cy))
            {
                cellY = cy;
                return true;
            }
        }
        return false;
    }
}
