using UnityEngine;

/// <summary>
/// MonoBehaviour: рендерит ячейки доски через пул SpriteRenderer'ов.
/// Вызови Init(board) из GameManager.
/// </summary>
public class BoardRenderer : MonoBehaviour
{
    [Header("Визуал")]
    [SerializeField] private Sprite _cellSprite;
    [SerializeField] private Color  _emptyColor = new Color(0.1f, 0.1f, 0.1f, 0.4f);

    private TetrisBoard       _board;
    private SpriteRenderer[,] _cells;

    // ─── Инициализация ────────────────────────────────────────────────────────

    public void Init(TetrisBoard board)
    {
        _board = board;
        int w = board.Width;
        int h = board.Height;

        _cells = new SpriteRenderer[w, h];

        for (int x = 0; x < w; x++)
        for (int y = 0; y < h; y++)
        {
            var go = new GameObject($"Cell_{x}_{y}");
            go.transform.SetParent(transform);
            go.transform.position = board.Grid.GetCellCenter(x, y);

            var sr         = go.AddComponent<SpriteRenderer>();
            sr.sprite      = _cellSprite;
            sr.color       = _emptyColor;
            sr.sortingOrder = 0;

            float size = board.Grid.CellSize * 0.95f;
            go.transform.localScale = new Vector3(size, size, 1f);

            _cells[x, y] = sr;
        }

        // Подписка на изменения ячеек доски
        board.Grid.OnValueChanged += RefreshCell;
    }

    // ─── Рендер активной фигуры и ghost ──────────────────────────────────────

    /// <summary>
    /// Вызывается каждый кадр из TetrisController: рисует ghost и активную фигуру.
    /// </summary>
    public void RenderActivePiece(ActivePiece piece, ActivePiece ghost)
    {
        // Сначала сбрасываем все ячейки к состоянию доски
        RefreshAll();

        // Ghost (прозрачный)
        if (ghost != null)
        {
            Color ghostColor = TetrominoData.Colors[ghost.Type];
            ghostColor.a = 0.22f;
            foreach (var cell in ghost.GetCells())
                if (_board.Grid.IsValid(cell.x, cell.y))
                    _cells[cell.x, cell.y].color = ghostColor;
        }

        // Активная фигура поверх ghost
        if (piece != null)
        {
            Color c = TetrominoData.Colors[piece.Type];
            if (piece.IsWeak) c = Color.Lerp(c, Color.white, 0.4f);
            foreach (var cell in piece.GetCells())
                if (_board.Grid.IsValid(cell.x, cell.y))
                    _cells[cell.x, cell.y].color = c;
        }
    }

    // ─── Обновление ──────────────────────────────────────────────────────────

    private void RefreshAll()
    {
        if (_cells == null) return;
        for (int x = 0; x < _board.Width; x++)
        for (int y = 0; y < _board.Height; y++)
            RefreshCell(x, y, _board.Grid.GetValue(x, y));
    }

    private void RefreshCell(int x, int y, CellData data)
    {
        if (_cells == null || !_board.Grid.IsValid(x, y)) return;

        var sr = _cells[x, y];

        if (data.IsEmpty)
        {
            sr.color = _emptyColor;
            return;
        }

        // Цвет по типу фигуры (цикличный для слабых блоков)
        int colorIndex = (data.PieceType - 1) % TetrominoData.Colors.Length;
        Color c = TetrominoData.Colors[colorIndex];

        if (data.IsWeak)
        {
            c = Color.Lerp(c, Color.white, 0.4f);

            // Затемнение пропорционально полученному урону
            if (data.MaxHP > 0 && data.HP < data.MaxHP)
            {
                float damageFraction = 1f - (float)data.HP / data.MaxHP;
                c = Color.Lerp(c, Color.black, damageFraction * 0.5f);
            }
        }

        sr.color = c;
    }
}
