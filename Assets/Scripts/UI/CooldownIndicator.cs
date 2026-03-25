using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

/// <summary>
/// Один индикатор cooldown — иконка + radial fill + текст таймера.
/// Тип задаётся через enum: DestroySide, DestroyDown, Push.
/// </summary>
public class CooldownIndicator : MonoBehaviour
{
    public enum CooldownType { DestroySide, DestroyDown, Push }

    [SerializeField] private CooldownType _type;
    [SerializeField] private Image        _iconImage;
    [SerializeField] private Image        _fillImage;
    [SerializeField] private TMP_Text     _timerLabel;
    [SerializeField] private Sprite       _iconReady;
    [SerializeField] private Sprite       _iconCooldown;

    private static readonly Color ColorReady    = new Color(0.502f, 1f,    0.690f); // #80FFB0
    private static readonly Color ColorCooldown = new Color(0.376f, 0.376f, 0.667f); // #6060AA

    private Character _character;
    private bool      _wasReady = true;

    private void Update()
    {
        if (_character == null)
        {
            _character = FindObjectOfType<Character>();
            return;
        }

        float normalized;
        bool  isReady;

        switch (_type)
        {
            case CooldownType.DestroyDown:
                normalized = _character.DestroyDownCooldownNormalized;
                isReady    = _character.IsDestroyDownReady;
                break;
            case CooldownType.Push:
                normalized = _character.PushCooldownNormalized;
                isReady    = _character.IsPushReady;
                break;
            default:
                normalized = _character.DestroyCooldownNormalized;
                isReady    = _character.IsDestroyReady;
                break;
        }

        if (_fillImage != null)
        {
            _fillImage.fillAmount = normalized;
            _fillImage.color      = isReady ? ColorReady : ColorCooldown;
        }

        if (_iconImage != null)
            _iconImage.sprite = isReady ? _iconReady : _iconCooldown;

        if (_timerLabel != null)
            _timerLabel.text = isReady ? "ГОТОВО" : $"{(1f - normalized) * GetMaxCooldown():F1}s";

        // Анимация — иконка "прыгает" когда становится ready
        if (isReady && !_wasReady)
            _iconImage?.transform.DOPunchScale(Vector3.one * 0.3f, 0.25f, 5, 0.5f);

        _wasReady = isReady;
    }

    private float GetMaxCooldown()
    {
        // Обратный расчёт секунд из нормализованного значения
        // Используем фиксированные значения из Character (5s для X/Z, 1s для Push)
        return _type == CooldownType.Push ? 1f : 5f;
    }
}
