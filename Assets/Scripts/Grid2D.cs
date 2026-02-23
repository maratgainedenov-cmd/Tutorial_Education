using System;
using UnityEngine;

/// <summary>
/// Универсальная 2D-сетка. T — тип данных в каждой ячейке.
/// </summary>
public class Grid2D<T>
{
    public event Action<int, int, T> OnValueChanged;

    public int Width  { get; private set; }
    public int Height { get; private set; }
    public float CellSize { get; private set; }

    private readonly Vector3 _origin;
    private readonly T[,] _grid;

    // ─── Конструктор ──────────────────────────────────────────────────────────

    /// <param name="width">Количество столбцов</param>
    /// <param name="height">Количество строк</param>
    /// <param name="cellSize">Размер одной ячейки в мировых единицах</param>
    /// <param name="origin">Нижний левый угол сетки в мировом пространстве</param>
    /// <param name="createCell">Фабрика для создания начального значения (необязательно)</param>
    public Grid2D(int width, int height, float cellSize, Vector3 origin,
                  Func<int, int, T> createCell = null)
    {
        Width    = width;
        Height   = height;
        CellSize = cellSize;
        _origin  = origin;
        _grid    = new T[width, height];

        if (createCell != null)
        {
            for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                _grid[x, y] = createCell(x, y);
        }
    }

    // ─── Конвертация координат ────────────────────────────────────────────────

    /// <summary>Центр ячейки (x, y) в мировом пространстве.</summary>
    public Vector3 GetCellCenter(int x, int y)
        => _origin + new Vector3(x * CellSize + CellSize * 0.5f,
                                 y * CellSize + CellSize * 0.5f);

    /// <summary>Нижний левый угол ячейки (x, y) в мировом пространстве.</summary>
    public Vector3 GetCellOrigin(int x, int y)
        => _origin + new Vector3(x * CellSize, y * CellSize);

    /// <summary>Преобразует мировую позицию в индексы ячейки.</summary>
    public void WorldToCell(Vector3 worldPos, out int x, out int y)
    {
        x = Mathf.FloorToInt((worldPos.x - _origin.x) / CellSize);
        y = Mathf.FloorToInt((worldPos.y - _origin.y) / CellSize);
    }

    // ─── Доступ к данным ──────────────────────────────────────────────────────

    public bool IsValid(int x, int y)
        => x >= 0 && y >= 0 && x < Width && y < Height;

    public void SetValue(int x, int y, T value)
    {
        if (!IsValid(x, y)) return;
        _grid[x, y] = value;
        OnValueChanged?.Invoke(x, y, value);
    }

    public void SetValue(Vector3 worldPos, T value)
    {
        WorldToCell(worldPos, out int x, out int y);
        SetValue(x, y, value);
    }

    public T GetValue(int x, int y)
        => IsValid(x, y) ? _grid[x, y] : default;

    public T GetValue(Vector3 worldPos)
    {
        WorldToCell(worldPos, out int x, out int y);
        return GetValue(x, y);
    }
}
