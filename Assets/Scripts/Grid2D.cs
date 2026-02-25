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
    }

    public void Build()
    {
        _cells = new Cell[_width, _height];

        for (int x = 0; x < _width; x++)
        {
            for (int y = 0; y < _height; y++)
            {
                Vector3 position = new Vector3(x * _cellSize, y * _cellSize, 0f);
                Cell cell = Instantiate(_cellPrefab, position, Quaternion.identity, transform);
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
}
