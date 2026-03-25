using UnityEngine;

/// <summary>
/// Мост между Animation Events на Visual и методами Character на корневом объекте.
/// Добавь этот компонент на тот же GameObject, где стоит Animator персонажа.
/// </summary>
public class CharacterAnimationEvents : MonoBehaviour
{
    private Character _character;

    private void Awake()
    {
        _character = GetComponentInParent<Character>();
    }

    public void TryPush()         => _character?.TryPush();
    public void TryDestroySide()  => _character?.TryDestroySide();
    public void TryDestroyDown()  => _character?.TryDestroyDown();
}
