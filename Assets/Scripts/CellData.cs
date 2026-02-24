/// <summary>
/// Данные одной ячейки доски.
/// PieceType: 0 = пусто, 1-7 = блок (тип фигуры).
/// HP / MaxHP: текущее и начальное здоровье блока.
/// </summary>
public struct CellData
{
    public int PieceType;
    public int HP;
    public int MaxHP;

    public bool IsEmpty => PieceType == 0;

    public static CellData Empty => new CellData { PieceType = 0, HP = 0, MaxHP = 0 };

    public static CellData Block(int type, int hp) =>
        new CellData { PieceType = type, HP = hp, MaxHP = hp };
}
