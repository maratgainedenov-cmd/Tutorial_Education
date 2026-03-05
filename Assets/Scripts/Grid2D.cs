using UnityEngine;

public class Grid2D : MonoBehaviour
{
    [SerializeField] private Cell _cellPrefab;
    [SerializeField] private int _width = 10;
    [SerializeField] private int _height = 20;
    [SerializeField] private float _cellSize = 1f;

    private Cell[,] _cells;

    private void Start()
    {
        Build();
        BuildBorder();
    }

    public void Build()
    {
        _cells = new Cell[_width, _height];

        for (int x = 0; x < _width; x++)
        {
            for (int y = 0; y < _height; y++)
            {
                Cell cell = Instantiate(_cellPrefab, transform);
                cell.transform.localPosition = new Vector3(x * _cellSize, y * _cellSize, 0f);
                cell.Init(x, y);
                _cells[x, y] = cell;
            }
        }
    }

    public Cell GetCell(int x, int y)
    {
        if (x < 0 || x >= _width || y < 0 || y >= _height) return null;
        return _cells[x, y];
    }

    public bool IsInBounds(int x, int y) =>
        x >= 0 && x < _width && y >= 0 && y < _height;

    private void BuildBorder()
    {
        float totalW  = _width  * _cellSize;
        float totalH  = _height * _cellSize;
        float centerX = (_width  - 1) * _cellSize * 0.5f;
        float centerY = (_height - 1) * _cellSize * 0.5f;

        // Пол
        CreateWall("Floor",
            new Vector3(centerX, -_cellSize, 0f),
            new Vector3(totalW, _cellSize, 1f));

        // Левая стена
        CreateWall("WallLeft",
            new Vector3(-_cellSize, centerY, 0f),
            new Vector3(_cellSize, totalH, 1f));

        // Правая стена
        CreateWall("WallRight",
            new Vector3(_width * _cellSize, centerY, 0f),
            new Vector3(_cellSize, totalH, 1f));
    }

    private void CreateWall(string wallName, Vector3 localPos, Vector3 scale)
    {
        var go = new GameObject(wallName);
        go.transform.SetParent(transform);
        go.transform.localPosition = localPos;
        go.transform.localScale    = scale;

        go.layer = LayerMask.NameToLayer("Ground");

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = CreateWhiteSprite();
        sr.color  = new Color(0.25f, 0.25f, 0.25f);

        go.AddComponent<BoxCollider2D>();
    }

    private static Sprite CreateWhiteSprite()
    {
        var tex = new Texture2D(1, 1);
        tex.SetPixel(0, 0, Color.white);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
    }
}
