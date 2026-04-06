using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Solo режим: заполняет нижние ряды случайными тетромино
/// и прячет выход под блоками в самом низу.
/// </summary>
public class SoloBoardSetup : MonoBehaviour
{
    [SerializeField] private Board     _board;
    [SerializeField] private Block     _blockPrefab;
    [SerializeField] private Transform _exit;

    [Header("Fill Settings")]
    [SerializeField] private int   _fillRows    = 10;   // заполняем до середины (борд 20 рядов)
    [SerializeField] private float _fillDensity = 0.65f; // плотность заполнения

    private static readonly Vector2Int[][] Shapes =
    {
        new[] { new Vector2Int(-1,0), new Vector2Int(0,0), new Vector2Int(1,0), new Vector2Int(2,0) }, // I
        new[] { new Vector2Int(0,0),  new Vector2Int(1,0), new Vector2Int(0,1), new Vector2Int(1,1) }, // O
        new[] { new Vector2Int(-1,0), new Vector2Int(0,0), new Vector2Int(1,0), new Vector2Int(0,1) }, // T
        new[] { new Vector2Int(-1,0), new Vector2Int(0,0), new Vector2Int(0,1), new Vector2Int(1,1) }, // S
        new[] { new Vector2Int(0,0),  new Vector2Int(1,0), new Vector2Int(-1,1),new Vector2Int(0,1) }, // Z
        new[] { new Vector2Int(-1,0), new Vector2Int(0,0), new Vector2Int(1,0), new Vector2Int(-1,1)}, // J
        new[] { new Vector2Int(-1,0), new Vector2Int(0,0), new Vector2Int(1,0), new Vector2Int(1,1) }, // L
    };

    public void Setup()
    {
        HideExit();
        FillBottomRows(-1);
        BreakFullRows();
    }

    private void BreakFullRows()
    {
        // Идём сверху вниз — укорачиваем колонку целиком, без дырок внутри
        for (int row = _fillRows - 1; row >= 0; row--)
        {
            if (!_board.IsRowFull(row)) continue;

            int col = Random.Range(0, _board.Width);
            // Убираем все блоки этой колонки начиная с row и выше
            for (int r = row; r < _fillRows; r++)
                _board.RemoveBlock(new Vector2Int(col, r));
        }
    }

    /// <summary>
    /// Прячет выход под O-блоком в самом низу.
    /// Возвращает X колонку выхода.
    /// </summary>
    private int HideExit()
    {
        if (_exit == null) return -1;

        var exitScript = _exit.GetComponent<Exit>();
        if (exitScript == null) return -1;

        int exitX = Random.Range(1, _board.Width - 2);
        exitScript.SetGridPos(new Vector2Int(exitX, 0));

        return exitX;
    }

    /// <summary>
    /// Заполняет нижние ряды случайными тетромино, оставляя проходы.
    /// </summary>
    private void FillBottomRows(int reservedX)
    {
        int minH = Mathf.RoundToInt(_fillRows * 0.6f);
        int h    = Random.Range(minH, _fillRows + 1);

        for (int col = 0; col < _board.Width; col++)
        {
            h += Random.Range(-2, 3); // ±2 — плавный рельеф
            h  = Mathf.Clamp(h, minH, _fillRows);

            for (int row = 0; row < h; row++)
            {
                TetrominoType type = (TetrominoType)Random.Range(0, 7);
                _board.PlaceSetupBlock(new Vector2Int(col, row), type);
            }
        }
    }

    private bool AllInBoundsAndFree(Vector2Int[] positions)
    {
        foreach (var p in positions)
            if (!_board.IsInBounds(p) || _board.IsOccupied(p))
                return false;
        return true;
    }
}
