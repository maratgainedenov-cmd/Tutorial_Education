using UnityEngine;

public class Block : MonoBehaviour
{
    [SerializeField] private SpriteRenderer _renderer;

    public void SetColor(Color color)
    {
        _renderer.color = color;
    }
}
