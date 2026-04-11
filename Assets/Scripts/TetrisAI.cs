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

        float roll = Random.value;

        // 50% — атака: кидаем рядом с персонажем, но не прямо на него
        if (roll < 0.50f && _target != null)
        {
            Vector3 local = _board.transform.InverseTransformPoint(_target.position);
            int charX  = Mathf.RoundToInt(local.x);
            // Смещение ±2..3 — давим, но не всегда в упор
            int offset = Random.value < 0.5f ? Random.Range(2, 4) : -Random.Range(2, 4);
            int col    = Mathf.Clamp(charX + offset, 1, _board.Width - 2);
            _controller.SpawnAtColumnAI(col);
        }
        // 30% — случайная колонка в средней зоне
        else if (roll < 0.80f)
        {
            int col = Random.Range(1, _board.Width - 1);
            _controller.SpawnAtColumnAI(col);
        }
        // 20% — край
        else
        {
            int col = Random.value < 0.5f ? 0 : _board.Width - 1;
            _controller.SpawnAtColumnAI(col);
        }
    }
}
