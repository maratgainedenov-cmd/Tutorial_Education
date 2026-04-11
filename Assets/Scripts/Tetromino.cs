using UnityEngine;
using DG.Tweening;

public enum TetrominoType { I, O, T, S, Z, J, L, Bomb }

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
        // Bomb — крест
        new[] { new Vector2Int(0, 0), new Vector2Int(-1, 0), new Vector2Int(1, 0), new Vector2Int(0, 1) },
    };

    private static readonly Color[] Colors =
    {
        new Color(0f,    0.941f, 0.941f), // I — #00F0F0 cyan
        new Color(0.941f, 0.941f, 0f),    // O — #F0F000 yellow
        new Color(0.627f, 0f,    0.941f), // T — #A000F0 purple
        new Color(0f,    0.941f, 0f),     // S — #00F000 green
        new Color(0.941f, 0f,    0f),     // Z — #F00000 red
        new Color(0f,    0f,    0.941f),  // J — #0000F0 blue
        new Color(0.941f, 0.627f, 0f),    // L — #F0A000 orange
        new Color(1f,    0.1f,  0.1f),   // Bomb — ярко-красный
    };

    public static Color GetColor(TetrominoType type) => Colors[(int)type];

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
            Block block = Instantiate(_blockPrefab, transform);
            block.transform.localPosition = GridToWorld(gridPos);
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

    public void SetPivot(Vector2Int newPivot)
    {
        _pivot = newPivot;
        RefreshBlockPositions();
    }

    public Vector2Int GetPivot() => _pivot;

    public void SetOffsets(Vector2Int[] offsets)
    {
        _offsets = (Vector2Int[])offsets.Clone();
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

    /// <summary>Обновляет визуальную позицию блоков с дробным смещением по Y для плавного падения.</summary>
    public void UpdateVisualPositions(float yOffset)
    {
        for (int i = 0; i < _blocks.Length; i++)
        {
            Vector2Int gridPos = _pivot + _offsets[i];
            _blocks[i].transform.localPosition = new Vector3(gridPos.x, gridPos.y + yOffset, 0f);
        }
    }

    private void RefreshBlockPositions()
    {
        for (int i = 0; i < _blocks.Length; i++)
        {
            Vector2Int gridPos = _pivot + _offsets[i];
            _blocks[i].transform.localPosition = GridToWorld(gridPos);
        }
    }

    private static Vector3 GridToWorld(Vector2Int gridPos) =>
        new Vector3(gridPos.x, gridPos.y, 0f);
}
