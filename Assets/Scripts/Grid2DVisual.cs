using UnityEngine;

/// <summary>
/// MonoBehaviour: создаёт Grid2D&lt;int&gt; и отрисовывает его.
/// Прикрепи к пустому GameObject на сцене.
/// </summary>
public class Grid2DVisual : MonoBehaviour
{
    [Header("Размер сетки")]
    [SerializeField] private int   width    = 10;
    [SerializeField] private int   height   = 10;
    [SerializeField] private float cellSize =  1f;

    [Header("Визуал")]
    [SerializeField] private Color  lineColor  = Color.white;
    [SerializeField] private bool   showCoords = true;
    [SerializeField] private Color  textColor  = Color.yellow;

    // Публичное свойство — другие скрипты могут получить сетку
    public Grid2D<int> Grid { get; private set; }

    // ─── Текстовые метки ──────────────────────────────────────────────────────

    private GameObject _labelsRoot;

    // ─── Инициализация ────────────────────────────────────────────────────────

    private void Awake()
    {
        Vector3 origin = transform.position;
        Grid = new Grid2D<int>(width, height, cellSize, origin);

        if (showCoords)
            CreateLabels(origin);
    }

    // ─── Отрисовка (Runtime) ─────────────────────────────────────────────────

    private void Update()
    {
        DrawGrid();
    }

    private void DrawGrid()
    {
        Vector3 origin = transform.position;
        float   w      = width  * cellSize;
        float   h      = height * cellSize;

        // Горизонтальные линии
        for (int y = 0; y <= height; y++)
        {
            Vector3 start = origin + new Vector3(0,     y * cellSize);
            Vector3 end   = origin + new Vector3(w,     y * cellSize);
            Debug.DrawLine(start, end, lineColor);
        }

        // Вертикальные линии
        for (int x = 0; x <= width; x++)
        {
            Vector3 start = origin + new Vector3(x * cellSize, 0);
            Vector3 end   = origin + new Vector3(x * cellSize, h);
            Debug.DrawLine(start, end, lineColor);
        }
    }

    // ─── Текстовые координаты ────────────────────────────────────────────────

    private void CreateLabels(Vector3 origin)
    {
        _labelsRoot = new GameObject("Grid_Labels");
        _labelsRoot.transform.SetParent(transform);

        for (int x = 0; x < width; x++)
        for (int y = 0; y < height; y++)
        {
            GameObject go = new GameObject($"Label_{x}_{y}");
            go.transform.SetParent(_labelsRoot.transform);

            Vector3 center = origin + new Vector3(
                x * cellSize + cellSize * 0.5f,
                y * cellSize + cellSize * 0.5f
            );
            go.transform.position = center;

            var tm         = go.AddComponent<TextMesh>();
            tm.text        = $"{x},{y}";
            tm.fontSize    = 20;
            tm.characterSize = cellSize * 0.06f;
            tm.anchor      = TextAnchor.MiddleCenter;
            tm.alignment   = TextAlignment.Center;
            tm.color       = textColor;
        }
    }

    // ─── Отрисовка в редакторе (Editor Gizmos) ───────────────────────────────

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Gizmos.color = lineColor;
        Vector3 origin = transform.position;

        for (int y = 0; y <= height; y++)
            Gizmos.DrawLine(
                origin + new Vector3(0,           y * cellSize),
                origin + new Vector3(width * cellSize, y * cellSize));

        for (int x = 0; x <= width; x++)
            Gizmos.DrawLine(
                origin + new Vector3(x * cellSize, 0),
                origin + new Vector3(x * cellSize, height * cellSize));
    }
#endif
}
