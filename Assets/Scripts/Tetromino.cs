using UnityEngine;

public enum TetrominoType { I, O, T, S, Z, J, L }

public class Tetromino : MonoBehaviour
{
    [SerializeField] private Block _blockPrefab;

    public TetrominoType Type { get; private set; }

    private Vector2Int _pivot;
    private Vector2Int[] _offsets;
    private Block[] _blocks;

    // Relative offsets for each type (pivot = (0,0))
    private static readonly Vector2Int[][] Shapes =
    {
        // I
        new[] { new Vector2Int(-1, 0), new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(2, 0) },
        // O
        new[] { new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(0, 1), new Vector2Int(1, 1) },
        // T
        new[] { new Vector2Int(-1, 0), new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(0, 1) },
        // S
        new[] { new Vector2Int(-1, 0), new Vector2Int(0, 0), new Vector2Int(0, 1), new Vector2Int(1, 1) },
        // Z
        new[] { new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(-1, 1), new Vector2Int(0, 1) },
        // J
        new[] { new Vector2Int(-1, 0), new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(-1, 1) },
        // L
        new[] { new Vector2Int(-1, 0), new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(1, 1) },
    };

    private static readonly Color[] Colors =
    {
        Color.cyan,                    // I
        Color.yellow,                  // O
        new Color(0.6f, 0f, 1f),       // T — purple
        Color.green,                   // S
        Color.red,                     // Z
        Color.blue,                    // J
        new Color(1f, 0.5f, 0f),       // L — orange
    };

    public void Init(TetrominoType type, Vector2Int spawnPivot)
    {
        Type = type;
        _pivot = spawnPivot;
        _offsets = (Vector2Int[])Shapes[(int)type].Clone();

        Color color = Colors[(int)type];
        _blocks = new Block[_offsets.Length];

        for (int i = 0; i < _offsets.Length; i++)
        {
            Vector2Int gridPos = _pivot + _offsets[i];
            Block block = Instantiate(_blockPrefab, GridToWorld(gridPos), Quaternion.identity, transform);
            block.SetColor(color);
            _blocks[i] = block;
        }
    }

    public Block[] GetBlocks() => _blocks;

    // Returns current world grid positions of all 4 blocks
    public Vector2Int[] GetPositions()
    {
        var result = new Vector2Int[_offsets.Length];
        for (int i = 0; i < _offsets.Length; i++)
            result[i] = _pivot + _offsets[i];
        return result;
    }

    public void Move(Vector2Int direction)
    {
        _pivot += direction;
        RefreshBlockPositions();
    }

    // Clockwise 90° rotation: (x, y) → (y, -x)
    public void Rotate()
    {
        if (Type == TetrominoType.O) return;

        for (int i = 0; i < _offsets.Length; i++)
        {
            int x = _offsets[i].x;
            int y = _offsets[i].y;
            _offsets[i] = new Vector2Int(y, -x);
        }

        RefreshBlockPositions();
    }

    // Undo last rotation (counterclockwise): (x, y) → (-y, x)
    public void RotateBack()
    {
        if (Type == TetrominoType.O) return;

        for (int i = 0; i < _offsets.Length; i++)
        {
            int x = _offsets[i].x;
            int y = _offsets[i].y;
            _offsets[i] = new Vector2Int(-y, x);
        }

        RefreshBlockPositions();
    }

    private void RefreshBlockPositions()
    {
        for (int i = 0; i < _blocks.Length; i++)
        {
            Vector2Int gridPos = _pivot + _offsets[i];
            _blocks[i].transform.position = GridToWorld(gridPos);
        }
    }

    private static Vector3 GridToWorld(Vector2Int gridPos) =>
        new Vector3(gridPos.x, gridPos.y, 0f);
}
