using UnityEngine;

// Показывает следующую фигуру тетриса в области превью.
// Привяжи к GameObject'у справа от сетки.
// Назначь в инспекторе: Block Prefab и ссылку на TetrominoSpawner.
public class NextPiecePreview : MonoBehaviour
{
    [SerializeField] private TetrominoSpawner _spawner;
    [SerializeField] private Block _blockPrefab;

    // Смещения для каждого типа (те же что в Tetromino.cs, но центрированные вокруг 0,0)
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

    private Block[] _previewBlocks;

    private void OnEnable()
    {
        if (_spawner != null)
            _spawner.OnNextChanged += Show;
    }

    private void OnDisable()
    {
        if (_spawner != null)
            _spawner.OnNextChanged -= Show;
    }

    private void Start()
    {
        CreateBlocks();
        if (_spawner != null)
            Show(_spawner.NextType);
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

    private void Show(TetrominoType type)
    {
        if (_previewBlocks == null) CreateBlocks();

        var offsets = Shapes[(int)type];
        Color color = Tetromino.GetColor(type);

        // Считаем центр формы чтобы центрировать превью вокруг pivot
        Vector2 center = Vector2.zero;
        foreach (var o in offsets) center += (Vector2)o;
        center /= offsets.Length;

        for (int i = 0; i < 4; i++)
        {
            _previewBlocks[i].gameObject.SetActive(true);
            Vector2 local = (Vector2)offsets[i] - center;
            _previewBlocks[i].transform.localPosition = new Vector3(local.x, local.y, 0f);
            _previewBlocks[i].SetColor(color);
        }
    }
}
