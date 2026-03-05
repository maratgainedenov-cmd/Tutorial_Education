using UnityEngine;

public class Board : MonoBehaviour
{
    [SerializeField] private int _width = 10;
    [SerializeField] private int _height = 20;
    [SerializeField] private Block _blockPrefab;

    public int Width => _width;
    public int Height => _height;

    private Block[,] _grid;

    private void Awake()
    {
        _grid = new Block[_width, _height];
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
            }
        }

        ApplyGravity();
        ClearLines();
        ApplyGravity();
    }

    private void ClearLines()
    {
        for (int y = 0; y < _height; y++)
        {
            if (!IsLineFull(y)) continue;

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
                    _grid[x, y].transform.localPosition = new Vector3(x, y, 0f);
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
        block.transform.localPosition = new Vector3(target.x, target.y, 0f);

        ApplyGravity();
        return true;
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

        ApplyGravity();
        ClearLines();
        ApplyGravity();
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
}
