using UnityEngine;

public class Cell : MonoBehaviour
{
    [SerializeField] private SpriteRenderer _fill;

    public int X { get; private set; }
    public int Y { get; private set; }

    public void Init(int x, int y)
    {
        X = x;
        Y = y;
    }

    public void SetColor(Color color)
    {
        _fill.color = color;
    }

    public Color GetColor() => _fill.color;
}
