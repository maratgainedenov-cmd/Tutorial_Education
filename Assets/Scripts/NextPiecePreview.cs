using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

/// <summary>
/// Превью следующей фигуры в UI.
/// Показывает тетромино через 4 Image-ячейки в UI панели.
/// Drag с панели на доску — фигура спаунится в нужной колонке.
/// </summary>
public class NextPiecePreview : MonoBehaviour
{
    [SerializeField] private TetrominoSpawner _spawner;
    [SerializeField] private TetrisController _controller;
    [SerializeField] private Board            _board;
    [SerializeField] private Block            _blockPrefab;   // только для ghost
    [SerializeField] private Image[]          _uiCells;       // ровно 4 Image в UI панели
    [SerializeField] private float            _cellSize = 36f;

    private static readonly Vector2Int[][] Shapes =
    {
        new[] { new Vector2Int(-1, 0), new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(2, 0) }, // I
        new[] { new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(0, 1), new Vector2Int(1, 1) },  // O
        new[] { new Vector2Int(-1, 0), new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(0, 1) }, // T
        new[] { new Vector2Int(-1, 0), new Vector2Int(0, 0), new Vector2Int(0, 1), new Vector2Int(1, 1) }, // S
        new[] { new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(-1, 1), new Vector2Int(0, 1) }, // Z
        new[] { new Vector2Int(-1, 0), new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(-1, 1) },// J
        new[] { new Vector2Int(-1, 0), new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(1, 1) }, // L
    };

    private Block[]            _ghostBlocks;
    private SpriteRenderer[][] _ghostRenderers;
    private bool               _isDragging;
    private TetrominoType      _currentType;
    private Vector2Int[]       _currentOffsets;

    private void OnEnable()  { if (_spawner != null) _spawner.OnNextChanged += Show; }
    private void OnDisable() { if (_spawner != null) _spawner.OnNextChanged -= Show; SetGhostActive(false); }

    private void Start()
    {
        CreateGhostBlocks();
        if (_spawner != null) Show(_spawner.NextType);
    }

    private void Update()
    {
        if (!_isDragging && Input.GetMouseButtonDown(0) && IsMouseOverPreviewPanel())
        {
            _isDragging = true;
            SetUICellsVisible(false);
        }

        if (!_isDragging) return;

        if (Input.GetKeyDown(KeyCode.W))
            RotateOffsets();

        if (IsMouseOverBoard())
        {
            int gridX = GetGridXFromMouse();
            UpdateGhost(gridX, _spawner != null ? _spawner.SpawnPosition.y : _board.Height - 2);
        }
        else
        {
            SetGhostActive(false);
        }

        if (Input.GetMouseButtonUp(0))
        {
            _isDragging = false;
            SetGhostActive(false);
            SetUICellsVisible(true);

            if (IsMouseOverBoard())
            {
                int gridX = GetGridXFromMouse();
                _controller?.SpawnAtColumn(gridX, _currentOffsets);
                Show(_spawner.NextType);
            }
        }
    }

    public void Show(TetrominoType type)
    {
        // Bomb убрана из спауна — пропускаем если вдруг пришла
        if ((int)type >= Shapes.Length) return;

        _currentType    = type;
        _currentOffsets = (Vector2Int[])Shapes[(int)type].Clone();

        if (_uiCells == null || _uiCells.Length < 4) return;

        Color color = Tetromino.GetColor(type);

        // Центрируем форму в панели
        Vector2 center = Vector2.zero;
        foreach (var o in _currentOffsets) center += (Vector2)o;
        center /= _currentOffsets.Length;

        for (int i = 0; i < 4; i++)
        {
            _uiCells[i].color = color;
            _uiCells[i].rectTransform.anchoredPosition = new Vector2(
                (_currentOffsets[i].x - center.x) * _cellSize,
                (_currentOffsets[i].y - center.y) * _cellSize);
            _uiCells[i].gameObject.SetActive(true);
        }
    }

    // ── Ghost ─────────────────────────────────────────────────────────────

    private void CreateGhostBlocks()
    {
        if (_blockPrefab == null || _board == null) return;
        _ghostBlocks    = new Block[4];
        _ghostRenderers = new SpriteRenderer[4][];

        for (int i = 0; i < 4; i++)
        {
            _ghostBlocks[i] = Instantiate(_blockPrefab, _board.transform);
            _ghostBlocks[i].gameObject.SetActive(false);

            foreach (var col in _ghostBlocks[i].GetComponentsInChildren<Collider2D>())
                col.enabled = false;

            _ghostRenderers[i] = _ghostBlocks[i].GetComponentsInChildren<SpriteRenderer>();
            foreach (var sr in _ghostRenderers[i]) sr.sortingOrder = 5;
        }
    }

    private void UpdateGhost(int pivotX, int pivotY)
    {
        if (_ghostBlocks == null) return;

        var positions = new Vector2Int[4];
        for (int i = 0; i < 4; i++)
            positions[i] = new Vector2Int(pivotX + _currentOffsets[i].x,
                                          pivotY + _currentOffsets[i].y);

        int drop = 0;
        while (_board.IsValidPositions(Shifted(positions, -drop - 1)))
            drop++;

        if (drop == 0) { SetGhostActive(false); return; }

        var ghostPos   = Shifted(positions, -drop);
        Color base_    = Tetromino.GetColor(_currentType);
        Color ghostCol = new Color(base_.r * 0.75f, base_.g * 0.75f, base_.b * 0.75f, 0.6f);

        for (int i = 0; i < 4; i++)
        {
            _ghostBlocks[i].gameObject.SetActive(true);
            _ghostBlocks[i].transform.localPosition = new Vector3(ghostPos[i].x, ghostPos[i].y, 0.1f);
            if (_ghostRenderers[i] != null)
                foreach (var sr in _ghostRenderers[i]) sr.color = ghostCol;
        }
    }

    private void SetGhostActive(bool active)
    {
        if (_ghostBlocks == null) return;
        foreach (var b in _ghostBlocks)
            if (b != null) b.gameObject.SetActive(active);
    }

    // ── Helpers ───────────────────────────────────────────────────────────

    private void RotateOffsets()
    {
        if (_currentType == TetrominoType.O) return;
        for (int i = 0; i < _currentOffsets.Length; i++)
        {
            int x = _currentOffsets[i].x, y = _currentOffsets[i].y;
            _currentOffsets[i] = new Vector2Int(y, -x);
        }
        Show(_currentType);
    }

    private int GetGridXFromMouse()
    {
        Vector3 world = MouseWorldPos();
        Vector3 local = _board.transform.InverseTransformPoint(world);
        int minOX = int.MaxValue, maxOX = int.MinValue;
        foreach (var o in _currentOffsets)
        {
            if (o.x < minOX) minOX = o.x;
            if (o.x > maxOX) maxOX = o.x;
        }
        return Mathf.Clamp(Mathf.FloorToInt(local.x), -minOX, _board.Width - 1 - maxOX);
    }

    private void SetUICellsVisible(bool visible)
    {
        if (_uiCells == null) return;
        foreach (var cell in _uiCells)
            if (cell != null) cell.gameObject.SetActive(visible);
    }

    private bool IsMouseOverPreviewPanel()
    {
        if (_uiCells == null || _uiCells.Length == 0 || _uiCells[0] == null) return false;
        var panel = _uiCells[0].transform.parent?.GetComponent<RectTransform>();
        if (panel == null) return false;
        return RectTransformUtility.RectangleContainsScreenPoint(panel, Input.mousePosition);
    }

    private bool IsMouseOverBoard()
    {
        if (Camera.main == null || _board == null) return false;
        Vector3 local = _board.transform.InverseTransformPoint(MouseWorldPos());
        return local.x >= 0 && local.x < _board.Width &&
               local.y >= 0 && local.y < _board.Height;
    }

    private Vector3 MouseWorldPos()
    {
        var mp = Input.mousePosition;
        mp.z   = -Camera.main.transform.position.z;
        return Camera.main.ScreenToWorldPoint(mp);
    }

    private static Vector2Int[] Shifted(Vector2Int[] pos, int dy)
    {
        var r = new Vector2Int[pos.Length];
        for (int i = 0; i < pos.Length; i++) r[i] = pos[i] + new Vector2Int(0, dy);
        return r;
    }
}
