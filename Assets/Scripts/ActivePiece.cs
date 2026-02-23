using UnityEngine;

/// <summary>
/// Текущая падающая фигура. Передаётся по ссылке — безопасное изменение состояния.
/// </summary>
public class ActivePiece
{
    public int       Type     { get; private set; }
    public int       Rotation { get; private set; }
    public Vector2Int Pos     { get; private set; }
    public bool      IsWeak   { get; private set; }
    public int       WeakHP   { get; private set; }

    public ActivePiece(int type, Vector2Int spawnPos, bool isWeak = false, int weakHP = 2)
    {
        Type     = type;
        Rotation = 0;
        Pos      = spawnPos;
        IsWeak   = isWeak;
        WeakHP   = weakHP;
    }

    /// <summary>Абсолютные координаты всех 4 клеток фигуры.</summary>
    public Vector2Int[] GetCells()
    {
        var offsets = TetrominoData.Shapes[Type][Rotation];
        var cells   = new Vector2Int[offsets.Length];
        for (int i = 0; i < offsets.Length; i++)
            cells[i] = Pos + offsets[i];
        return cells;
    }

    public void Move(int dx, int dy)
    {
        Pos = new Vector2Int(Pos.x + dx, Pos.y + dy);
    }

    /// <summary>dir: +1 по часовой, -1 против часовой.</summary>
    public void Rotate(int dir)
    {
        int count    = TetrominoData.Shapes[Type].Length;
        Rotation     = ((Rotation + dir) % count + count) % count;
    }

    /// <summary>Явно задать поворот (для ghost-фигуры).</summary>
    public void SetRotation(int rotation)
    {
        int count = TetrominoData.Shapes[Type].Length;
        Rotation  = ((rotation % count) + count) % count;
    }

    public void SetPosition(Vector2Int pos) => Pos = pos;
}
