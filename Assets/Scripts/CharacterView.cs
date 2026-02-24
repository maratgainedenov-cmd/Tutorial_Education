using UnityEngine;

/// <summary>
/// MonoBehaviour: визуальное представление персонажа (flip, анимации).
/// Требует SpriteRenderer. Animator — опционально.
/// </summary>
public class CharacterView : MonoBehaviour
{
    [Header("Компоненты")]
    [SerializeField] private SpriteRenderer _spriteRenderer;
    [SerializeField] private Animator       _animator;  // необязательно

    // Хэши параметров аниматора
    private static readonly int HashSpeed     = Animator.StringToHash("Speed");
    private static readonly int HashGrounded  = Animator.StringToHash("Grounded");
    private static readonly int HashWallSlide = Animator.StringToHash("WallSlide");
    private static readonly int HashJump      = Animator.StringToHash("Jump");
    private static readonly int HashAttack    = Animator.StringToHash("Attack");
    private static readonly int HashHitStrong = Animator.StringToHash("HitStrong");

    private void Awake()
    {
        if (_spriteRenderer == null)
            _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    // ─── API для CharacterController2D ───────────────────────────────────────

    public void SetFacing(float velocityX)
    {
        if (Mathf.Abs(velocityX) > 0.01f)
            _spriteRenderer.flipX = velocityX < 0f;
    }

    public void UpdateAnimations(float speedX, bool grounded, bool wallSliding)
    {
        if (_animator == null) return;
        _animator.SetFloat(HashSpeed,     Mathf.Abs(speedX));
        _animator.SetBool(HashGrounded,   grounded);
        _animator.SetBool(HashWallSlide,  wallSliding);
    }

    public void PlayJump()
    {
        _animator?.SetTrigger(HashJump);
    }

    public void PlayAttack()
    {
        _animator?.SetTrigger(HashAttack);
    }

    /// <summary>Удар пришёлся по сильному блоку — анимация отдачи.</summary>
    public void PlayHitStrong()
    {
        _animator?.SetTrigger(HashHitStrong);
    }

    public void SetPosition(Vector3 worldPos)
    {
        transform.position = worldPos;
    }
}
