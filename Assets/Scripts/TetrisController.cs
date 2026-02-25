using UnityEngine;

public class TetrisController : MonoBehaviour
{
    [SerializeField] private TetrominoSpawner _spawner;
    [SerializeField] private Board _board;
    [SerializeField] private float _fallInterval = 1f;

    private Tetromino _current;
    private float _timer;

    private void OnEnable()
    {
        _spawner.OnSpawned += OnSpawned;
    }

    private void OnDisable()
    {
        _spawner.OnSpawned -= OnSpawned;
    }

    private void Update()
    {
        if (_current == null) return;

        HandleInput();

        _timer += Time.deltaTime;
        if (_timer >= _fallInterval)
        {
            _timer = 0f;
            if (!TryMove(Vector2Int.down))
                LockCurrent();
        }
    }

    private void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
            TryMove(Vector2Int.left);

        if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
            TryMove(Vector2Int.right);

        if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
        {
            if (TryMove(Vector2Int.down))
                _timer = 0f;
        }

        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
            TryRotate();
    }

    private bool TryMove(Vector2Int direction)
    {
        _current.Move(direction);

        if (_board.IsValidPositions(_current.GetPositions()))
            return true;

        _current.Move(-direction);
        return false;
    }

    private void TryRotate()
    {
        _current.Rotate();

        if (_board.IsValidPositions(_current.GetPositions()))
            return;

        _current.RotateBack();
    }

    private void LockCurrent()
    {
        _board.Lock(_current.GetPositions(), _current.GetBlocks());
        Destroy(_current.gameObject);
        _current = null;
        _spawner.SpawnNext();
    }

    private void OnSpawned(Tetromino tetromino)
    {
        _current = tetromino;
        _timer = 0f;
    }
}
