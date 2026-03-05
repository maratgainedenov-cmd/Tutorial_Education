using UnityEngine;
using Photon.Pun;

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

    private Rigidbody2D _rb;
    private bool _isGrounded;
    private bool _isTouchingLeftWall;
    private bool _isTouchingRightWall;
    private int _lastWallJumpDir; // -1 = от левой, 1 = от правой, 0 = не было
    private int _facingDir = 1;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        if (_board == null) _board = FindObjectOfType<Board>();
    }

    private void Update()
    {
        if (!photonView.IsMine) return;
        CheckCollisions();
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
        if (h != 0) _facingDir = (int)Mathf.Sign(h);
        UpdateFacingLine();

        // Push
        if (Input.GetKeyDown(KeyCode.Keypad0) || Input.GetKeyDown(KeyCode.Insert))
            TryPush();

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
        if (_board == null) return;
        Vector2Int myPos = Vector2Int.RoundToInt(
            (Vector2)transform.position - (Vector2)_board.transform.position);
        Vector2Int pushPos = myPos + new Vector2Int(_facingDir, 0);
        Vector2Int pushDir = new Vector2Int(_facingDir, 0);
        photonView.RPC(nameof(RpcPushBlock), RpcTarget.All,
            pushPos.x, pushPos.y, pushDir.x, pushDir.y);
    }

    [PunRPC]
    private void RpcPushBlock(int px, int py, int dx, int dy)
    {
        if (_board == null) return;
        _board.TryPushBlock(new Vector2Int(px, py), new Vector2Int(dx, dy));
    }

    private void Jump(Vector2 force)
    {
        _rb.velocity = new Vector2(_rb.velocity.x, 0f);
        _rb.AddForce(force, ForceMode2D.Impulse);
    }

    public void Die()
    {
        photonView.RPC(nameof(RpcDie), RpcTarget.All);
    }

    [PunRPC]
    private void RpcDie()
    {
        gameObject.SetActive(false);
        GameManager.Instance?.GameOver();
    }
}
