using System;
using UnityEngine;

/// <summary>
/// Модель доски: Grid2D&lt;CellData&gt;, коллизии, очистка линий.
/// Чистый C# — не MonoBehaviour.
/// </summary>
public class TetrisBoard
{
    public Grid2D<CellData> Grid   { get; private set; }
    public int Width               => Grid.Width;
    public int Height              => Grid.Height;

    public event Action<int>  OnLinesCleared;  // кол-во очищенных линий
    public event Action       OnPiecePlaced;

    public TetrisBoard(int width, int height, float cellSize, Vector3 origin)
    {
        Grid = new Grid2D<CellData>(width, height, cellSize, origin,
                                    (x, y) => CellData.Empty);
    }

    // ─── Коллизии ─────────────────────────────────────────────────────────────

    /// <summary>Все клетки фигуры находятся в допустимых позициях.</summary>
    public bool IsValidPosition(ActivePiece piece)
    {
        foreach (var cell in piece.GetCells())
        {
            if (cell.x < 0 || cell.x >= Width) return false;
            if (cell.y < 0)                    return false;
            if (cell.y >= Height)              continue; // выше доски — ок при спавне
            if (!Grid.GetValue(cell.x, cell.y).IsEmpty) return false;
        }
        return true;
    }

    /// <summary>Можно ли заспавнить фигуру (есть ли место).</summary>
    public bool CanSpawn(ActivePiece piece) => IsValidPosition(piece);

    /// <summary>Зафиксировать фигуру на доске.</summary>
    public void PlacePiece(ActivePiece piece)
    {
        foreach (var cell in piece.GetCells())
        {
            if (!Grid.IsValid(cell.x, cell.y)) continue;

            CellData data = CellData.Block(piece.Type + 1, piece.HP);

            Grid.SetValue(cell.x, cell.y, data);
        }
        OnPiecePlaced?.Invoke();
    }

    // ─── Очистка линий ────────────────────────────────────────────────────────

    /// <summary>Очистить заполненные линии. Возвращает количество очищенных.</summary>
    public int ClearLines()
    {
        int cleared = 0;
        for (int y = Height - 1; y >= 0; y--)
        {
            if (!IsLineFull(y)) continue;

            ClearLine(y);
            DropLinesAbove(y);
            y++;       // повторно проверить эту строку (теперь содержит линию сверху)
            cleared++;
        }

        if (cleared > 0)
            OnLinesCleared?.Invoke(cleared);

        return cleared;
    }

    // ─── Повреждение блоков ───────────────────────────────────────────────────

    /// <summary>
    /// Нанести урон блоку в ячейке (x, y).
    /// Возвращает true, если блок разрушен (HP ≤ 0).
    /// </summary>
    public bool DamageCell(int x, int y, int damage = 1)
    {
        if (!Grid.IsValid(x, y)) return false;

        CellData cell = Grid.GetValue(x, y);
        if (cell.IsEmpty) return false;

        cell.HP -= damage;
        if (cell.HP <= 0)
        {
            Grid.SetValue(x, y, CellData.Empty);
            return true;
        }

        Grid.SetValue(x, y, cell);
        return false;
    }

    // ─── Утилиты ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Дополнительная проверка занятости ячейки (например, активная фигура).
    /// Задаётся из GameManager после инициализации.
    /// </summary>
    public System.Func<int, int, bool> ExtraOccupied { get; set; }

    public bool IsCellOccupied(int x, int y)
    {
        if (!Grid.IsValid(x, y)) return false;
        if (!Grid.GetValue(x, y).IsEmpty) return true;
        return ExtraOccupied?.Invoke(x, y) ?? false;
    }

    // ─── Private ──────────────────────────────────────────────────────────────

    private bool IsLineFull(int y)
    {
        for (int x = 0; x < Width; x++)
            if (Grid.GetValue(x, y).IsEmpty) return false;
        return true;
    }

    private void ClearLine(int y)
    {
        for (int x = 0; x < Width; x++)
            Grid.SetValue(x, y, CellData.Empty);
    }

    private void DropLinesAbove(int clearedY)
    {
        for (int y = clearedY; y < Height - 1; y++)
        for (int x = 0; x < Width; x++)
            Grid.SetValue(x, y, Grid.GetValue(x, y + 1));

        // Верхняя строка теперь пустая
        for (int x = 0; x < Width; x++)
            Grid.SetValue(x, Height - 1, CellData.Empty);
    }
}
