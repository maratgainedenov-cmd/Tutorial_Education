using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Отображает состояние cooldown способностей разрушения персонажа (X/Z).
/// Зелёный = готово, красный = на перезарядке, полоса заполняется по мере восстановления.
/// Персонаж ищется автоматически в момент появления на сцене.
/// </summary>
public class CooldownUI : MonoBehaviour
{
    [SerializeField] private Image _fillImage;
    [SerializeField] private TMP_Text _label;

    private Character _character;

    private static readonly Color ColorReady    = new Color(0.2f, 0.9f, 0.3f);
    private static readonly Color ColorCooldown = new Color(0.9f, 0.2f, 0.2f);

    private void Update()
    {
        if (_character == null)
        {
            _character = FindObjectOfType<Character>();
            return;
        }

        float t = _character.DestroyCooldownNormalized;

        if (_fillImage != null)
        {
            _fillImage.fillAmount = t;
            _fillImage.color      = _character.IsDestroyReady ? ColorReady : ColorCooldown;
        }

        if (_label != null)
            _label.text = _character.IsDestroyReady ? "ГОТОВО" : $"{(_character.DestroyCooldownNormalized * 5f):F1}s";
    }
}
