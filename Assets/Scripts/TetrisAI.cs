using UnityEngine;

/// <summary>
/// Агрессивный AI для режима Solo.
/// Каждые N секунд кидает фигуру в колонку где стоит персонаж.
/// </summary>
public class TetrisAI : MonoBehaviour
{
    [SerializeField] private TetrisController _controller;
    [SerializeField] private Board            _board;
    [SerializeField] private float            _thinkInterval = 3f;

    private float     _timer;
    private bool      _active;
    private Transform _target; // персонаж — находим в рантайме

    public void StartAI()
    {
        _active = true;
        _timer  = _thinkInterval;
    }

    private void Update()
    {
        if (!_active) return;

        // Ищем персонажа если ещё не нашли
        if (_target == null)
        {
            var ch = FindObjectOfType<Character>();
            if (ch != null) _target = ch.transform;
        }

        _timer -= Time.deltaTime;
        if (_timer <= 0f)
        {
            _timer = _thinkInterval;
            PlacePiece();
        }
    }

    private void PlacePiece()
    {
        if (_controller == null || _board == null) return;
        int col = PickColumn();
        _controller.SpawnAtColumnAI(col);
    }

    private int PickColumn()
    {
        if (_target == null)
            return Random.Range(0, _board.Width);

        // Агрессивно: целимся в колонку персонажа ± 1
        Vector3 local = _board.transform.InverseTransformPoint(_target.position);
        int charX  = Mathf.RoundToInt(local.x);
        int offset = Random.Range(-1, 2); // -1, 0, +1
        return Mathf.Clamp(charX + offset, 0, _board.Width - 1);
    }
}
