using UnityEngine;

public class Character : MonoBehaviour
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
    [SerializeField] private Vector2 _wallCheckSize  = new Vector2(0.1f, 0.8f);

    private Rigidbody2D _rb;
    private bool _isGrounded;
    private bool _isTouchingLeftWall;
    private bool _isTouchingRightWall;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        CheckCollisions();
        HandleInput();
    }

    private void CheckCollisions()
    {
        Vector2 pos = transform.position;

        _isGrounded = Physics2D.OverlapBox(
            pos + Vector2.down * 0.5f, _groundCheckSize, 0f, _solidLayer);

        _isTouchingLeftWall = Physics2D.OverlapBox(
            pos + Vector2.left * 0.5f, _wallCheckSize, 0f, _solidLayer);

        _isTouchingRightWall = Physics2D.OverlapBox(
            pos + Vector2.right * 0.5f, _wallCheckSize, 0f, _solidLayer);
    }

    private void HandleInput()
    {
        // Horizontal movement
        float h = Input.GetAxisRaw("Horizontal");
        _rb.linearVelocity = new Vector2(h * _moveSpeed, _rb.linearVelocity.y);

        // Jump
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
        {
            if (_isGrounded)
            {
                Jump(Vector2.up * _jumpForce);
            }
            else if (_isTouchingLeftWall)
            {
                Jump(new Vector2(_wallJumpForceX, _wallJumpForceY));
            }
            else if (_isTouchingRightWall)
            {
                Jump(new Vector2(-_wallJumpForceX, _wallJumpForceY));
            }
        }
    }

    private void Jump(Vector2 force)
    {
        _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, 0f);
        _rb.AddForce(force, ForceMode2D.Impulse);
    }
}
