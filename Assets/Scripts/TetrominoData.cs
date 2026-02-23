using UnityEngine;

/// <summary>
/// Статические данные всех 7 тетромино: цвета и формы (4 поворота × 4 клетки).
/// Индексы: 0=I, 1=O, 2=T, 3=S, 4=Z, 5=J, 6=L.
/// </summary>
public static class TetrominoData
{
    public const int Count = 7;

    public static readonly Color[] Colors =
    {
        new Color(0.00f, 1.00f, 1.00f), // 0: I  — Cyan
        new Color(1.00f, 1.00f, 0.00f), // 1: O  — Yellow
        new Color(0.60f, 0.00f, 1.00f), // 2: T  — Purple
        new Color(0.00f, 1.00f, 0.00f), // 3: S  — Green
        new Color(1.00f, 0.00f, 0.00f), // 4: Z  — Red
        new Color(0.00f, 0.40f, 1.00f), // 5: J  — Blue
        new Color(1.00f, 0.55f, 0.00f), // 6: L  — Orange
    };

    // [тип][поворот][клетка] — смещения от опорной позиции (Pos)
    public static readonly Vector2Int[][][] Shapes = new Vector2Int[][][]
    {
        // 0: I
        new Vector2Int[][]
        {
            new[] { new Vector2Int(0,1), new Vector2Int(1,1), new Vector2Int(2,1), new Vector2Int(3,1) },
            new[] { new Vector2Int(2,0), new Vector2Int(2,1), new Vector2Int(2,2), new Vector2Int(2,3) },
            new[] { new Vector2Int(0,2), new Vector2Int(1,2), new Vector2Int(2,2), new Vector2Int(3,2) },
            new[] { new Vector2Int(1,0), new Vector2Int(1,1), new Vector2Int(1,2), new Vector2Int(1,3) },
        },
        // 1: O
        new Vector2Int[][]
        {
            new[] { new Vector2Int(1,0), new Vector2Int(2,0), new Vector2Int(1,1), new Vector2Int(2,1) },
            new[] { new Vector2Int(1,0), new Vector2Int(2,0), new Vector2Int(1,1), new Vector2Int(2,1) },
            new[] { new Vector2Int(1,0), new Vector2Int(2,0), new Vector2Int(1,1), new Vector2Int(2,1) },
            new[] { new Vector2Int(1,0), new Vector2Int(2,0), new Vector2Int(1,1), new Vector2Int(2,1) },
        },
        // 2: T
        new Vector2Int[][]
        {
            new[] { new Vector2Int(0,1), new Vector2Int(1,1), new Vector2Int(2,1), new Vector2Int(1,2) },
            new[] { new Vector2Int(1,0), new Vector2Int(1,1), new Vector2Int(1,2), new Vector2Int(2,1) },
            new[] { new Vector2Int(0,1), new Vector2Int(1,1), new Vector2Int(2,1), new Vector2Int(1,0) },
            new[] { new Vector2Int(1,0), new Vector2Int(1,1), new Vector2Int(1,2), new Vector2Int(0,1) },
        },
        // 3: S
        new Vector2Int[][]
        {
            new[] { new Vector2Int(0,0), new Vector2Int(1,0), new Vector2Int(1,1), new Vector2Int(2,1) },
            new[] { new Vector2Int(1,1), new Vector2Int(1,2), new Vector2Int(2,0), new Vector2Int(2,1) },
            new[] { new Vector2Int(0,0), new Vector2Int(1,0), new Vector2Int(1,1), new Vector2Int(2,1) },
            new[] { new Vector2Int(1,1), new Vector2Int(1,2), new Vector2Int(2,0), new Vector2Int(2,1) },
        },
        // 4: Z
        new Vector2Int[][]
        {
            new[] { new Vector2Int(0,1), new Vector2Int(1,0), new Vector2Int(1,1), new Vector2Int(2,0) },
            new[] { new Vector2Int(1,0), new Vector2Int(1,1), new Vector2Int(2,1), new Vector2Int(2,2) },
            new[] { new Vector2Int(0,1), new Vector2Int(1,0), new Vector2Int(1,1), new Vector2Int(2,0) },
            new[] { new Vector2Int(1,0), new Vector2Int(1,1), new Vector2Int(2,1), new Vector2Int(2,2) },
        },
        // 5: J
        new Vector2Int[][]
        {
            new[] { new Vector2Int(0,1), new Vector2Int(0,2), new Vector2Int(1,1), new Vector2Int(2,1) },
            new[] { new Vector2Int(1,0), new Vector2Int(1,1), new Vector2Int(1,2), new Vector2Int(2,2) },
            new[] { new Vector2Int(0,1), new Vector2Int(1,1), new Vector2Int(2,0), new Vector2Int(2,1) },
            new[] { new Vector2Int(0,0), new Vector2Int(1,0), new Vector2Int(1,1), new Vector2Int(1,2) },
        },
        // 6: L
        new Vector2Int[][]
        {
            new[] { new Vector2Int(0,1), new Vector2Int(1,1), new Vector2Int(2,1), new Vector2Int(2,2) },
            new[] { new Vector2Int(1,0), new Vector2Int(1,1), new Vector2Int(1,2), new Vector2Int(2,0) },
            new[] { new Vector2Int(0,0), new Vector2Int(0,1), new Vector2Int(1,1), new Vector2Int(2,1) },
            new[] { new Vector2Int(0,2), new Vector2Int(1,0), new Vector2Int(1,1), new Vector2Int(1,2) },
        },
    };
}
