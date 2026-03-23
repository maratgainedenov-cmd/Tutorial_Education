using UnityEngine;
using DG.Tweening;

public class NextPiecePreview : MonoBehaviour
{
    [SerializeField] private TetrominoSpawner  _spawner;
    [SerializeField] private Block             _blockPrefab;
    [SerializeField] private TetrisController  _controller;
    [SerializeField] private Board             _board;
    [SerializeField] private BombNPC           _bombNpcPrefab;

    private static readonly Vector2Int[][] Shapes =
    {
        new[] { new Vector2Int(-1, 0), new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(2, 0) }, // I
        new[] { new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(0, 1), new Vector2Int(1, 1) },  // O
        new[] { new Vector2Int(-1, 0), new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(0, 1) }, // T
        new[] { new Vector2Int(-1, 0), new Vector2Int(0, 0), new Vector2Int(0, 1), new Vector2Int(1, 1) }, // S
        new[] { new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(-1, 1), new Vector2Int(0, 1) }, // Z
        new[] { new Vector2Int(-1, 0), new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(-1, 1) },// J
        new[] { new Vector2Int(-1, 0), new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(1, 1) }, // L
        new[] { new Vector2Int(0, 0), new Vector2Int(0, 0), new Vector2Int(0, 0), new Vector2Int(0, 0) }, // Bomb — 1 блок
    };

    private Block[]      _previewBlocks;
    private Block[]      _ghostBlocks;
    private SpriteRenderer[][] _ghostRenderers;
    private bool         _isDragging;
    private Vector3      _homePosition;
    private TetrominoType _currentType;
    private Vector2Int[] _currentOffsets;
    private GameObject   _bombPreviewVisual;

    private void OnEnable()  { if (_spawner != null) _spawner.OnNextChanged += Show; }
    private void OnDisable()
    {
        if (_spawner != null) _spawner.OnNextChanged -= Show;
        SetGhostActive(false);
        if (_bombPreviewVisual != null) { Destroy(_bombPreviewVisual); _bombPreviewVisual = null; }
    }

    private void Awake()
    {
        if (_controller == null) _controller = FindObjectOfType<TetrisController>();
        if (_board      == null) _board      = FindObjectOfType<Board>();
    }

    private void Start()
    {
        _homePosition = transform.position;
        CreateBlocks();
        CreateGhostBlocks();
        if (_spawner != null) Show(_spawner.NextType);
    }

    private void Update()
    {
        if (_previewBlocks == null) return;

        if (!_isDragging && Input.GetMouseButtonDown(0))
            _isDragging = true;

        if (!_isDragging) return;

        Vector3 world = MouseWorldPos();

        if (Input.GetKeyDown(KeyCode.W))
            RotatePreview();

        if (IsMouseOverBoard())
        {
            Vector3 local = _board.transform.InverseTransformPoint(world);
            int gridX     = ClampGridX(Mathf.FloorToInt(local.x));
            int snapY     = _spawner != null ? _spawner.SpawnPosition.y : _board.Height - 2;

            // Превью снэпится к колонке вверху
            Vector2 center    = GetOffsetCenter();
            Vector3 snapWorld = _board.transform.TransformPoint(
                new Vector3(gridX + center.x, snapY + center.y, 0f));
            snapWorld.z = transform.position.z;
            transform.position = snapWorld;

            // Ghost показывает куда упадёт
            UpdateGhost(gridX, snapY);
        }
        else
        {
            world.z = transform.position.z;
            transform.position = world;
            SetGhostActive(false);
        }

        if (Input.GetMouseButtonUp(0))
        {
            _isDragging = false;
            SetGhostActive(false);

            if (IsMouseOverBoard())
            {
                Vector3 local = _board.transform.InverseTransformPoint(world);
                int gridX = ClampGridX(Mathf.FloorToInt(local.x));

                if (_currentType == TetrominoType.Bomb)
                    _controller?.SpawnBombNpcAt(gridX);
                else
                    _controller?.SpawnAtColumn(gridX, _currentOffsets);

                transform.position = _homePosition;
                Show(_spawner.NextType);
            }
            else
            {
                transform.DOMove(_homePosition, 0.25f).SetEase(Ease.OutCubic);
            }
        }
    }

    // ── Ghost ────────────────────────────────────────────────────────────

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

        // Собираем позиции фигуры у верха сетки
        var positions = new Vector2Int[4];
        for (int i = 0; i < 4; i++)
            positions[i] = new Vector2Int(pivotX + _currentOffsets[i].x,
                                          pivotY + _currentOffsets[i].y);

        // Опускаем вниз пока можно
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
            _ghostBlocks[i].transform.localPosition =
                new Vector3(ghostPos[i].x, ghostPos[i].y, 0.1f);
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

    private static Vector2Int[] Shifted(Vector2Int[] pos, int dy)
    {
        var r = new Vector2Int[pos.Length];
        for (int i = 0; i < pos.Length; i++) r[i] = pos[i] + new Vector2Int(0, dy);
        return r;
    }

    // ── Helpers ──────────────────────────────────────────────────────────

    private void RotatePreview()
    {
        if (_currentType == TetrominoType.O) return;
        var rotated = new Vector2Int[_currentOffsets.Length];
        for (int i = 0; i < _currentOffsets.Length; i++)
        {
            int x = _currentOffsets[i].x; int y = _currentOffsets[i].y;
            rotated[i] = new Vector2Int(y, -x);
        }
        _currentOffsets = rotated;
        RefreshBlockPositions();
    }

    private Vector2 GetOffsetCenter()
    {
        Vector2 c = Vector2.zero;
        foreach (var o in _currentOffsets) c += (Vector2)o;
        return c / _currentOffsets.Length;
    }

    private void RefreshBlockPositions()
    {
        Vector2 center = GetOffsetCenter();
        for (int i = 0; i < 4; i++)
        {
            Vector2 local = (Vector2)_currentOffsets[i] - center;
            _previewBlocks[i].transform.localPosition = new Vector3(local.x, local.y, 0f);
        }
    }

    private int ClampGridX(int gridX)
    {
        int minOX = int.MaxValue, maxOX = int.MinValue;
        foreach (var o in _currentOffsets)
        {
            if (o.x < minOX) minOX = o.x;
            if (o.x > maxOX) maxOX = o.x;
        }
        return Mathf.Clamp(gridX, -minOX, _board.Width - 1 - maxOX);
    }

    private Vector3 MouseWorldPos()
    {
        var mp = Input.mousePosition;
        mp.z   = -Camera.main.transform.position.z;
        return Camera.main.ScreenToWorldPoint(mp);
    }

    private bool IsMouseOverBoard()
    {
        if (Camera.main == null || _board == null) return false;
        Vector3 local = _board.transform.InverseTransformPoint(MouseWorldPos());
        return local.x >= 0 && local.x < _board.Width &&
               local.y >= 0 && local.y < _board.Height;
    }

    private void CreateBlocks()
    {
        _previewBlocks = new Block[4];
        for (int i = 0; i < 4; i++)
        {
            _previewBlocks[i] = Instantiate(_blockPrefab, transform);
            _previewBlocks[i].gameObject.SetActive(false);
        }
    }

    public void Show(TetrominoType type)
    {
        if (_previewBlocks == null) CreateBlocks();
        _currentType    = type;
        _currentOffsets = (Vector2Int[])Shapes[(int)type].Clone();
        // Удаляем старый визуал бомбы если был
        if (_bombPreviewVisual != null) { Destroy(_bombPreviewVisual); _bombPreviewVisual = null; }

        bool isBomb = type == TetrominoType.Bomb;

        if (isBomb)
        {
            // Прячем блоки, показываем спрайт персонажа
            foreach (var b in _previewBlocks) b.gameObject.SetActive(false);
            if (_bombNpcPrefab != null)
            {
                _bombPreviewVisual = Instantiate(_bombNpcPrefab.gameObject, transform);
                _bombPreviewVisual.transform.localPosition = Vector3.zero;
                // Отключаем всё кроме визуала
                foreach (var comp in _bombPreviewVisual.GetComponentsInChildren<Rigidbody2D>()) comp.simulated = false;
                foreach (var comp in _bombPreviewVisual.GetComponentsInChildren<Collider2D>())  comp.enabled   = false;
                foreach (var comp in _bombPreviewVisual.GetComponentsInChildren<MonoBehaviour>()) comp.enabled = false;
            }
        }
        else
        {
            Color color = Tetromino.GetColor(type);
            for (int i = 0; i < 4; i++) _previewBlocks[i].SetColor(color);
            RefreshBlockPositions();
            foreach (var b in _previewBlocks) b.gameObject.SetActive(true);
        }
    }
}
