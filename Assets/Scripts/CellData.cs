/// <summary>
/// Данные одной ячейки доски.
/// PieceType: 0 = пусто, 1-7 = обычный блок (тип фигуры), 8+ = слабый блок.
/// </summary>
public struct CellData
{
    public int  PieceType;
    public bool IsWeak;
    public int  HP;

    public bool IsEmpty => PieceType == 0;

    public static CellData Empty => new CellData { PieceType = 0, IsWeak = false, HP = 0 };

    public static CellData Strong(int type) =>
        new CellData { PieceType = type, IsWeak = false, HP = 0 };

    public static CellData Weak(int type, int hp) =>
        new CellData { PieceType = type, IsWeak = true, HP = hp };
}
