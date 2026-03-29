using UnityEngine;

/// <summary>
/// Базовый класс для всех UI панелей.
/// Наследуй от него каждую панель.
/// </summary>
public abstract class UIPanel : MonoBehaviour
{
    public virtual void Show() => gameObject.SetActive(true);
    public virtual void Hide() => gameObject.SetActive(false);
}
