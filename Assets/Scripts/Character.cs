using UnityEngine;
using Photon.Pun;
using DG.Tweening;

[RequireComponent(typeof(Rigidbody2D))]
public class Character : MonoBehaviourPun
{
    [Header("Movement")]
    [SerializeField] private float _moveSpeed = 5f;
    [SerializeField] private float _jumpForce = 12f;

    [Header("Wall Jump")]
    [SerializeField] private float _wallJumpForceX = 8f;
    [SerializeField] private float _wallJumpForceY = 12f;

    [Header("Collision Detection")]
    [SerializeField] private LayerMask _solidLayer;
    [SerializeField] private Vector2 _groundCheckSize = new Vector2(0.8f, 0.1f);
    [SerializeField] private Vector2 _wallCheckSize   = new Vector2(0.1f, 0.8f);

    [Header("Push")]
    [SerializeField] private Board _board;
    [SerializeField] private LineRenderer _facingLine;

    [Header("Destroy Block")]
    [SerializeField] private KeyCode _destroySideKey = KeyCode.X;
    [SerializeField] private KeyCode _destroyDownKey = KeyCode.Z;
    [SerializeField] private float _destroyCooldown = 5f;
    private float _destroyCooldownTimer = 0f;

    public float DestroyCooldownNormalized => _destroyCooldown > 0f ? Mathf.Clamp01(1f - _destroyCooldownTimer / _destroyCooldown) : 1f;
    public bool IsDestroyReady => _destroyCooldownTimer <= 0f;

    [Header("Juice")]
    [SerializeField] private Transform _visual;
    [SerializeField] private Animator _animator;

    private Rigidbody2D _rb;
    private Camera _camera;
    private bool _isGrounded;
    private bool _wasGrounded;
    private bool _isWallSliding;
    private bool _isTouchingLeftWall;
    private bool _isTouchingRightWall;
    private int _lastWallJumpDir; // -1 = от левой, 1 = от правой, 0 = не было
    private int _facingDir = 1;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _camera = Camera.main;
        if (_board == null) _board = FindObjectOfType<Board>();
    }

    private void Update()
    {
        if (!photonView.IsMine && !GameManager.LocalDebug) return;
        CheckCollisions();
        if (!_wasGrounded && _isGrounded)
        {
            _visual?.DOPunchScale(new Vector3(-0.2f, 0.25f, 0), 0.2f, 5, 0.5f);
            _camera?.transform.DOShakePosition(0.1f, 0.15f, 10, 90, false, true);
        }
        _isWallSliding = (_isTouchingLeftWall || _isTouchingRightWall)
                         && !_isGrounded && _rb.velocity.y < 0;

        // Замедляем падение вдоль стены
        if (_isWallSliding)
            _rb.velocity = new Vector2(_rb.velocity.x, Mathf.Max(_rb.velocity.y, -2f));

        _wasGrounded = _isGrounded;
        _animator?.SetBool("IsGrounded",    _isGrounded);
        _animator?.SetBool("IsWallSliding", _isWallSliding);
        HandleInput();
    }

    private void CheckCollisions()
    {
        Vector2 pos = transform.position;

        _isGrounded = _rb.velocity.y <= 0.1f && Physics2D.OverlapBox(
            pos + Vector2.down * 0.5f, _groundCheckSize, 0f, _solidLayer);

        if (_isGrounded) _lastWallJumpDir = 0;

        _isTouchingLeftWall = Physics2D.OverlapBox(
            pos + Vector2.left * 0.5f, _wallCheckSize, 0f, _solidLayer);

        _isTouchingRightWall = Physics2D.OverlapBox(
            pos + Vector2.right * 0.5f, _wallCheckSize, 0f, _solidLayer);
    }

    private void HandleInput()
    {
        // Horizontal movement
        float h = (Input.GetKey(KeyCode.LeftArrow) ? -1f : 0f) + (Input.GetKey(KeyCode.RightArrow) ? 1f : 0f);
        _rb.velocity = new Vector2(h * _moveSpeed, _rb.velocity.y);
        if (h != 0)
        {
            _facingDir = (int)Mathf.Sign(h);
            if (_visual != null)
                _visual.localScale = new Vector3(_facingDir, 1f, 1f);
        }
        _animator?.SetFloat("Speed", Mathf.Abs(_rb.velocity.x));
        UpdateFacingLine();

        // Push
        if (Input.GetKeyDown(KeyCode.Keypad0) || Input.GetKeyDown(KeyCode.Insert))
            TryPush();

        // Destroy block
        _destroyCooldownTimer -= Time.deltaTime;
        if (Input.GetKeyDown(_destroySideKey))
            TryDestroySide();
        if (Input.GetKeyDown(_destroyDownKey))
            TryDestroyDown();

        // Jump
        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.Space))
        {
            if (_isGrounded)
            {
                Jump(Vector2.up * _jumpForce);
            }
            else if (_isTouchingLeftWall && _lastWallJumpDir != -1 && _rb.velocity.y <= 0)
            {
                _lastWallJumpDir = -1;
                Jump(new Vector2(_wallJumpForceX, _wallJumpForceY));
            }
            else if (_isTouchingRightWall && _lastWallJumpDir != 1 && _rb.velocity.y <= 0)
            {
                _lastWallJumpDir = 1;
                Jump(new Vector2(-_wallJumpForceX, _wallJumpForceY));
            }
        }
    }

    private void UpdateFacingLine()
    {
        if (_facingLine == null) return;
        Vector3 origin = transform.position;
        Vector3 end = origin + new Vector3(_facingDir, 0f, 0f);
        _facingLine.SetPosition(0, origin);
        _facingLine.SetPosition(1, end);
    }

    private void TryPush()
    {
        _visual?.DOPunchScale(new Vector3(0.2f, -0.15f, 0) * _facingDir, 0.15f, 5, 0.5f);
        _animator?.SetBool("IsPushing", true);
        Invoke(nameof(StopPushAnim), 0.4f);

        // Пробуем толкнуть BombNPC — ищем в зоне перед персонажем
        Vector2 checkPos = (Vector2)transform.position + Vector2.right * _facingDir * 0.9f;
        var cols = Physics2D.OverlapCircleAll(checkPos, 0.6f);
        foreach (var col in cols)
        {
            var npc = col.GetComponent<BombNPC>()
                   ?? col.GetComponentInParent<BombNPC>();
            if (npc != null)
            {
                npc.Push(new Vector2(_facingDir, 0.5f));
                return;
            }
        }

        // Иначе толкаем блок тетриса
        if (_board == null) return;
        Vector2Int myPos = Vector2Int.RoundToInt(
            (Vector2)transform.position - (Vector2)_board.transform.position);
        Vector2Int pushPos = myPos + new Vector2Int(_facingDir, 0);
        Vector2Int pushDir = new Vector2Int(_facingDir, 0);
        if (GameManager.LocalDebug)
            RpcPushBlock(pushPos.x, pushPos.y, pushDir.x, pushDir.y);
        else
            photonView.RPC(nameof(RpcPushBlock), RpcTarget.All,
                pushPos.x, pushPos.y, pushDir.x, pushDir.y);
    }

    private void TryDestroySide()
    {
        if (_visual != null) { _visual.DOKill(); _visual.localScale = Vector3.one; }
        _animator?.SetTrigger("Destroy");
        if (_destroyCooldownTimer > 0f || _board == null) return;

        Vector2Int myPos = Vector2Int.RoundToInt(
            (Vector2)transform.position - (Vector2)_board.transform.position);

        foreach (var dir in new[] { new Vector2Int(_facingDir, 0), new Vector2Int(-_facingDir, 0) })
        {
            Vector2Int target = myPos + dir;
            if (_board.IsInBounds(target) && _board.IsOccupied(target))
            {
                if (GameManager.LocalDebug)
                    RpcDestroyBlock(target.x, target.y);
                else
                    photonView.RPC(nameof(RpcDestroyBlock), RpcTarget.All, target.x, target.y);
                _destroyCooldownTimer = _destroyCooldown;
                return;
            }
        }
    }

    private void TryDestroyDown()
    {
        if (_visual != null) { _visual.DOKill(); _visual.localScale = Vector3.one; }
        _animator?.SetTrigger("DestroyDown");
        if (_destroyCooldownTimer > 0f || _board == null) return;

        Vector2Int myPos = Vector2Int.RoundToInt(
            (Vector2)transform.position - (Vector2)_board.transform.position);

        Vector2Int target = myPos + Vector2Int.down;
        if (_board.IsInBounds(target) && _board.IsOccupied(target))
        {
            if (GameManager.LocalDebug)
                RpcDestroyBlock(target.x, target.y);
            else
                photonView.RPC(nameof(RpcDestroyBlock), RpcTarget.All, target.x, target.y);
            _destroyCooldownTimer = _destroyCooldown;
        }
    }

    [PunRPC]
    private void RpcDestroyBlock(int x, int y)
    {
        _board?.DestroyAt(new Vector2Int(x, y));
    }

    [PunRPC]
    private void RpcPushBlock(int px, int py, int dx, int dy)
    {
        if (_board == null) return;
        _board.TryPushBlock(new Vector2Int(px, py), new Vector2Int(dx, dy));
    }

    private void StopPushAnim() => _animator?.SetBool("IsPushing", false);

    private void Jump(Vector2 force)
    {
        _rb.velocity = new Vector2(_rb.velocity.x, 0f);
        _rb.AddForce(force, ForceMode2D.Impulse);
        _visual?.DOPunchScale(new Vector3(-0.15f, 0.3f, 0), 0.25f, 5, 0.5f);
    }

    public void Die()
    {
        if (GameManager.LocalDebug)
            RpcDie();
        else
            photonView.RPC(nameof(RpcDie), RpcTarget.All);
    }

    [PunRPC]
    private void RpcDie()
    {
        CancelInvoke(nameof(StopPushAnim));
        _animator?.SetBool("IsDead", true);
        _camera?.transform.DOShakePosition(0.5f, 1f, 20, 90, false, true);
        enabled = false;
        _rb.isKinematic = true;
        GameManager.Instance?.GameOver();
    }
}
