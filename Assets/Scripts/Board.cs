using UnityEngine;
using DG.Tweening;

public class Board : MonoBehaviour
{
    [SerializeField] private int _width  = 10;
    [SerializeField] private int _height = 20;
    [SerializeField] private Block _blockPrefab;

    public int Width  => _width;
    public int Height => _height;

    private Block[,] _grid;
    private Camera _camera;

    private void Awake()
    {
        _grid = new Block[_width, _height];
        _camera = Camera.main;
    }

    public void Lock(Vector2Int[] positions, Block[] blocks)
    {
        _grid ??= new Block[_width, _height];

        for (int i = 0; i < positions.Length; i++)
        {
            var pos = positions[i];
            if (pos.x >= 0 && pos.x < _width && pos.y >= 0 && pos.y < _height)
            {
                _grid[pos.x, pos.y] = blocks[i];
                blocks[i].transform.SetParent(transform);
                blocks[i].transform.DOKill();
                blocks[i].transform.localPosition = new Vector3(pos.x, pos.y, 0f);
            }
        }

        _camera?.transform.DOShakePosition(0.08f, 0.15f, 10, 90, false, true);

        ClearLines();
        ApplyGravity();
    }

    private void ClearLines()
    {
        for (int y = 0; y < _height; y++)
        {
            if (!IsLineFull(y)) continue;

            _camera?.transform.DOShakePosition(0.3f, 0.5f, 15, 90, false, true);
            ClearLine(y);
            ShiftDown(y);
            y--;
        }
    }

    private bool IsLineFull(int y)
    {
        for (int x = 0; x < _width; x++)
        {
            if (_grid[x, y] == null) return false;
        }
        return true;
    }

    private void ClearLine(int y)
    {
        for (int x = 0; x < _width; x++)
        {
            _grid[x, y].transform.DOKill();
            Destroy(_grid[x, y].gameObject);
            _grid[x, y] = null;
        }
    }

    private void ShiftDown(int clearedY)
    {
        for (int y = clearedY; y < _height - 1; y++)
        {
            for (int x = 0; x < _width; x++)
            {
                _grid[x, y] = _grid[x, y + 1];
                _grid[x, y + 1] = null;

                if (_grid[x, y] != null)
                {
                    _grid[x, y].transform.DOKill();
                    _grid[x, y].transform.localPosition = new Vector3(x, y, 0f);
                }
            }
        }
    }

    private void ApplyGravity()
    {
        for (int x = 0; x < _width; x++)
        {
            int writeY = 0;
            for (int y = 0; y < _height; y++)
            {
                if (_grid[x, y] == null) continue;

                if (writeY != y)
                {
                    _grid[x, writeY] = _grid[x, y];
                    _grid[x, y] = null;
                    _grid[x, writeY].transform.localPosition = new Vector3(x, writeY, 0f);
                }

                writeY++;
            }
        }
    }

    public bool TryPushBlock(Vector2Int pos, Vector2Int dir)
    {
        if (!IsOccupied(pos)) return false;

        Vector2Int target = pos + dir;
        if (!IsInBounds(target) || IsOccupied(target)) return false;

        Block block = _grid[pos.x, pos.y];
        _grid[pos.x, pos.y] = null;
        _grid[target.x, target.y] = block;

        // Отключаем коллайдер на время анимации — иначе он зажимает персонажа
        var col = block.GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        block.transform.DOKill();
        block.transform.DOLocalMove(new Vector3(target.x, target.y, 0f), 0.1f)
            .SetEase(Ease.OutCubic)
            .OnComplete(() =>
            {
                if (col != null) col.enabled = true;
                ClearLines();
                ApplyGravity();
            });

        return true;
    }

    public void Explode(Vector2Int center, int radius = 3)
    {
        bool hit = false;
        for (int x = center.x - radius; x <= center.x + radius; x++)
        {
            for (int y = center.y - radius; y <= center.y + radius; y++)
            {
                float dx = x - center.x, dy = y - center.y;
                if (dx * dx + dy * dy > radius * radius) continue;
                var pos = new Vector2Int(x, y);
                if (!IsInBounds(pos) || _grid[x, y] == null) continue;

                var block = _grid[x, y];
                _grid[x, y] = null;
                block.transform.DOKill();
                block.transform.DOScale(0f, 0.2f)
                    .SetEase(Ease.InBack)
                    .OnComplete(() => Destroy(block.gameObject));
                hit = true;
            }
        }

        if (hit)
        {
            _camera?.transform.DOShakePosition(0.5f, 1f, 20, 90, false, true);
            ApplyGravity();
        }
    }

    public bool IsInBounds(Vector2Int pos)
    {
        return pos.x >= 0 && pos.x < _width && pos.y >= 0 && pos.y < _height;
    }

    public bool IsOccupied(Vector2Int pos)
    {
        if (pos.x < 0 || pos.x >= _width || pos.y < 0 || pos.y >= _height)
            return false;
        return _grid[pos.x, pos.y] != null;
    }

    public void LockRemote(Vector2Int[] positions, TetrominoType type)
    {
        _grid ??= new Block[_width, _height];
        Color color = Tetromino.GetColor(type);

        for (int i = 0; i < positions.Length; i++)
        {
            var pos = positions[i];
            if (pos.x < 0 || pos.x >= _width || pos.y < 0 || pos.y >= _height) continue;

            Block b = Instantiate(_blockPrefab, transform);
            b.transform.localPosition = new Vector3(pos.x, pos.y, 0f);
            b.SetColor(color);
            _grid[pos.x, pos.y] = b;
        }

        ClearLines();
        ApplyGravity();
    }

    public bool IsRowFull(int row)
    {
        for (int x = 0; x < _width; x++)
            if (_grid[x, row] == null) return false;
        return true;
    }

    public void RemoveBlock(Vector2Int pos)
    {
        if (pos.x < 0 || pos.x >= _width || pos.y < 0 || pos.y >= _height) return;
        if (_grid[pos.x, pos.y] == null) return;
        Destroy(_grid[pos.x, pos.y].gameObject);
        _grid[pos.x, pos.y] = null;
    }

    /// <summary>Ставит одиночный блок без очистки линий — только для инициализации поля.</summary>
    public void PlaceSetupBlock(Vector2Int pos, TetrominoType type)
    {
        _grid ??= new Block[_width, _height];
        if (pos.x < 0 || pos.x >= _width || pos.y < 0 || pos.y >= _height) return;
        if (_grid[pos.x, pos.y] != null) return;

        Block b = Instantiate(_blockPrefab, transform);
        b.transform.localPosition = new Vector3(pos.x, pos.y, 0f);
        b.SetColor(Tetromino.GetColor(type));
        _grid[pos.x, pos.y] = b;
    }

    public bool DestroyAt(Vector2Int pos)
    {
        if (!IsInBounds(pos) || _grid[pos.x, pos.y] == null) return false;
        var block = _grid[pos.x, pos.y];
        _grid[pos.x, pos.y] = null;
        block.transform.DOKill();
        block.transform.DOScale(0f, 0.15f)
            .SetEase(Ease.InBack)
            .OnComplete(() => Destroy(block.gameObject));
        ApplyGravity();
        return true;
    }

    public bool IsValidPositions(Vector2Int[] positions)
    {
        foreach (var pos in positions)
        {
            if (!IsInBounds(pos)) return false;
            if (IsOccupied(pos)) return false;
        }
        return true;
    }

    /// <summary>Уничтожает все блоки на поле и сбрасывает сетку.</summary>
    public void ClearBoard()
    {
        if (_grid == null) return;
        for (int x = 0; x < _width; x++)
        {
            for (int y = 0; y < _height; y++)
            {
                if (_grid[x, y] == null) continue;
                _grid[x, y].transform.DOKill();
                Destroy(_grid[x, y].gameObject);
                _grid[x, y] = null;
            }
        }
    }
}
