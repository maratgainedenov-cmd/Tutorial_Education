using System;
using UnityEngine;
using DG.Tweening;
using Photon.Pun;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class BombNPC : MonoBehaviourPun, IPunObservable
{
    [Header("Movement")]
    [SerializeField] private float     _moveSpeed         = 2f;
    [SerializeField] private float     _jumpForce         = 8f;
    [SerializeField] private LayerMask _solidLayer;

    [Header("Obstacle Detection")]
    [SerializeField] private float _obstacleCheckDist = 0.6f;
    [SerializeField] private float _jumpCooldown      = 0.6f;

    [Header("Explosion")]
    [SerializeField] private float _explodeDistance = 1f;
    [SerializeField] private float _explodeRadius   = 2f;
    [SerializeField] private float _fuseTime        = 2f;
    [SerializeField] private Board  _board;

    [Header("Push")]
    [SerializeField] private float _pushForce = 8f;

    [Header("Visual")]
    [SerializeField] private Transform      _visual;
    [SerializeField] private Animator       _animator;
    [SerializeField] private SpriteRenderer _spriteRenderer;

    public event Action OnDied;

    private Rigidbody2D _rb;
    private Camera      _camera;
    private Transform   _target;
    private bool        _exploded;
    private bool        _isGrounded;
    private float       _jumpTimer;
    private float       _moveDir;

    // Приземление
    private bool _isLanded;
    private int  _landingGridX;

    // Ghost preview
    private GameObject     _ghostObject;
    private SpriteRenderer _ghostRenderer;

    // Fuse
    private bool  _fuseActive;
    private float _fuseTimer;
    private Tween _pulseTween;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _camera = Camera.main;
        if (_board          == null) _board          = FindObjectOfType<Board>();
        if (_visual         == null) _visual         = transform.Find("Visual") ?? transform;
        if (_animator       == null) _animator       = GetComponentInChildren<Animator>();
        if (_spriteRenderer == null) _spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        // Вычисляем колонку по позиции спавна
        if (_board != null)
            _landingGridX = Mathf.RoundToInt(
                _board.transform.InverseTransformPoint(transform.position).x);

        // Ghost только у мастера (он один кидает бомбу)
        if (photonView.IsMine || GameManager.LocalDebug)
            CreateGhost();
    }

    private void CreateGhost()
    {
        _ghostObject   = new GameObject("BombGhost");
        _ghostRenderer = _ghostObject.AddComponent<SpriteRenderer>();

        if (_spriteRenderer != null)
        {
            _ghostRenderer.sprite       = _spriteRenderer.sprite;
            _ghostRenderer.sortingOrder = _spriteRenderer.sortingOrder - 1;
        }
        _ghostRenderer.color = new Color(1f, 0.3f, 0f, 0.45f);
        _ghostObject.transform.localScale = transform.localScale;
        _ghostObject.SetActive(false);
    }

    private void Start()
    {
        if (photonView.IsMine || GameManager.LocalDebug)
            _rb.AddForce(Vector2.up * 4f, ForceMode2D.Impulse);

        if (_visual != null)
            _pulseTween = _visual.DOScale(1.15f, 0.4f)
                                 .SetLoops(-1, LoopType.Yoyo)
                                 .SetEase(Ease.InOutSine)
                                 .SetLink(gameObject);
    }

    private void Update()
    {
        // Только мастер управляет AI и физикой
        if (!photonView.IsMine && !GameManager.LocalDebug) return;
        if (_exploded) return;

        // ── Фаза падения ──────────────────────────────────────────────
        if (!_isLanded)
        {
            UpdateGhost();
            return;
        }

        // ── Фаза NPC ──────────────────────────────────────────────────
        if (_target == null) return;

        _jumpTimer -= Time.deltaTime;
        CheckGround();

        float dist = Vector2.Distance(transform.position, _target.position);

        if (!_fuseActive && dist <= _explodeDistance)
            StartFuse();

        if (_fuseActive)
        {
            _fuseTimer -= Time.deltaTime;
            if (_fuseTimer <= 0f) { TriggerExplode(); return; }
            _rb.velocity = new Vector2(0f, _rb.velocity.y);
            return;
        }

        _moveDir = Mathf.Sign(_target.position.x - transform.position.x);

        if (_isGrounded && _jumpTimer <= 0f && HasObstacleAhead())
            Jump();

        _rb.velocity = new Vector2(_moveDir * _moveSpeed, _rb.velocity.y);

        if (_spriteRenderer != null)
            _spriteRenderer.flipX = _moveDir < 0;
        else if (_visual != null)
            _visual.localScale = new Vector3(_moveDir, 1f, 1f);
    }

    // Определяем приземление по физическому контакту (только мастер)
    private void OnCollisionEnter2D(Collision2D col)
    {
        if (!photonView.IsMine && !GameManager.LocalDebug) return;
        if (_isLanded || _exploded) return;

        foreach (var contact in col.contacts)
        {
            if (contact.normal.y > 0.5f)
            {
                OnLanded();
                return;
            }
        }
    }

    private void UpdateGhost()
    {
        if (_ghostObject == null || _board == null) return;

        if (_spriteRenderer != null)
            _ghostRenderer.sprite = _spriteRenderer.sprite;

        int gridX = Mathf.Clamp(_landingGridX, 0, _board.Width - 1);

        int landingY = 0;
        for (int y = _board.Height - 1; y >= 0; y--)
        {
            if (_board.IsOccupied(new Vector2Int(gridX, y)))
            {
                landingY = y + 1;
                break;
            }
        }

        Vector3 worldPos = _board.transform.TransformPoint(new Vector3(gridX, landingY, 0f));
        float visualOffsetY = _visual != null ? _visual.localPosition.y : 0f;

        _ghostObject.SetActive(true);
        _ghostObject.transform.position = new Vector3(
            worldPos.x, worldPos.y + visualOffsetY, transform.position.z - 0.1f);
    }

    private void OnLanded()
    {
        _isLanded = true;

        if (_ghostObject != null) Destroy(_ghostObject);

        _visual?.DOPunchScale(new Vector3(-0.2f, 0.4f, 0), 0.3f, 5, 0.5f);

        var playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null) _target = playerObj.transform;
    }

    private void CheckGround()
    {
        LayerMask mask = _solidLayer.value == 0 ? Physics2D.AllLayers : _solidLayer;
        _isGrounded = _rb.velocity.y <= 0.1f && Physics2D.OverlapBox(
            (Vector2)transform.position + Vector2.down * 0.6f,
            new Vector2(0.7f, 0.1f), 0f, mask);

        _animator?.SetBool("IsGrounded", _isGrounded);
    }

    private bool HasObstacleAhead()
    {
        LayerMask mask = _solidLayer.value == 0 ? Physics2D.AllLayers : _solidLayer;
        Vector2 origin = (Vector2)transform.position + Vector2.up * 0.1f;
        RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.right * _moveDir,
                                              _obstacleCheckDist, mask);
        return hit.collider != null;
    }

    private void Jump()
    {
        _rb.velocity = new Vector2(_rb.velocity.x, 0f);
        _rb.AddForce(Vector2.up * _jumpForce, ForceMode2D.Impulse);
        _jumpTimer = _jumpCooldown;
        _visual?.DOPunchScale(new Vector3(-0.15f, 0.3f, 0), 0.2f, 5, 0.5f);
    }

    private void StartFuse()
    {
        _fuseActive = true;
        _fuseTimer  = _fuseTime;
        ApplyFuseVisuals();
    }

    private void ApplyFuseVisuals()
    {
        _animator?.SetBool("IsFuse", true);
        _pulseTween?.Kill();
        if (_visual == null) return;
        _visual.localScale = Vector3.one;
        AnimateFuse();
    }

    private void AnimateFuse()
    {
        if (_exploded || !_fuseActive || _visual == null) return;

        float progress = 1f - Mathf.Clamp01(_fuseTimer / _fuseTime);
        float interval = Mathf.Lerp(0.3f, 0.05f, progress);

        _visual.DOScale(1.4f, interval * 0.5f)
               .SetEase(Ease.OutQuad)
               .OnComplete(() =>
               {
                   if (_exploded || !_fuseActive || _visual == null) return;
                   _visual.DOScale(1f, interval * 0.5f)
                          .SetEase(Ease.InQuad)
                          .OnComplete(AnimateFuse);
               });
    }

    public void Push(Vector2 direction)
    {
        if (_exploded) return;
        _rb.velocity = Vector2.zero;
        _rb.AddForce(direction.normalized * _pushForce, ForceMode2D.Impulse);
        _visual?.DOPunchScale(new Vector3(direction.x * 0.3f, -0.2f, 0), 0.2f, 5, 0.5f);
    }

    private void OnDestroy()
    {
        if (_ghostObject != null) Destroy(_ghostObject);
        OnDied?.Invoke();
    }

    // ── Взрыв ────────────────────────────────────────────────────────────

    private void TriggerExplode()
    {
        // Только мастер вычисляет параметры и рассылает RPC
        Vector2Int boardPos = _board != null
            ? Vector2Int.RoundToInt((Vector2)transform.position - (Vector2)_board.transform.position)
            : Vector2Int.zero;

        bool killPlayer = _target != null
            && Vector2.Distance(transform.position, _target.position) <= _explodeRadius;

        if (GameManager.LocalDebug)
            RpcExplode(boardPos.x, boardPos.y, killPlayer);
        else
            photonView.RPC(nameof(RpcExplode), RpcTarget.All, boardPos.x, boardPos.y, killPlayer);
    }

    [PunRPC]
    private void RpcExplode(int bx, int by, bool killPlayer)
    {
        _exploded = true;
        _rb.velocity = Vector2.zero;
        _rb.isKinematic = true;
        if (_ghostObject != null) Destroy(_ghostObject);

        _animator?.SetTrigger("Explode");
        _pulseTween?.Kill();
        DOTween.Kill(_visual);

        Transform t = _visual != null ? _visual : transform;
        t.DOScale(2.5f, 0.1f)
         .SetEase(Ease.OutCubic)
         .OnComplete(() =>
             t.DOScale(0f, 0.2f)
              .SetEase(Ease.InBack)
              .OnComplete(() =>
              {
                  if (GameManager.LocalDebug)
                      Destroy(gameObject);
                  else if (PhotonNetwork.IsMasterClient)
                      PhotonNetwork.Destroy(gameObject);
                  // Клиент: объект уничтожится сам когда мастер вызовет PhotonNetwork.Destroy
              }));

        _camera?.transform.DOShakePosition(0.5f, 1.2f, 20, 90, false, true);

        // Разрушение блоков — на всех клиентах
        if (_board != null)
            _board.Explode(new Vector2Int(bx, by), 3);

        // Убийство персонажа — только мастер (Character.Die использует свой RPC)
        if (killPlayer && (PhotonNetwork.IsMasterClient || GameManager.LocalDebug) && _target != null)
        {
            var character = _target.GetComponentInParent<Character>()
                         ?? _target.GetComponent<Character>();
            character?.Die();
        }
    }

    // ── Сетевая синхронизация ─────────────────────────────────────────────

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(transform.position);
            stream.SendNext(_rb.velocity);
            stream.SendNext(_isLanded);
            stream.SendNext(_fuseActive);
            stream.SendNext(_moveDir);
        }
        else
        {
            transform.position = (Vector3)stream.ReceiveNext();
            _rb.velocity       = (Vector2)stream.ReceiveNext();
            bool newLanded     = (bool)stream.ReceiveNext();
            bool newFuse       = (bool)stream.ReceiveNext();
            _moveDir           = (float)stream.ReceiveNext();

            // Триггерим события приземления и фитиля локально на клиенте
            if (!_isLanded && newLanded) OnLanded();
            if (!_fuseActive && newFuse) { _fuseActive = true; ApplyFuseVisuals(); }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, _explodeDistance);
        Gizmos.color = new Color(1f, 0.5f, 0f, 0.4f);
        Gizmos.DrawWireSphere(transform.position, _explodeRadius);
    }
}
