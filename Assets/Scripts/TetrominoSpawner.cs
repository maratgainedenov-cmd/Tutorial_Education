using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class TetrominoSpawner : MonoBehaviour
{
    [SerializeField] private Tetromino _tetrominoPrefab;
    [SerializeField] private Transform _boardTransform;
    [SerializeField] private Vector2Int _spawnPosition = new Vector2Int(4, 18);

    public event Action<Tetromino> OnSpawned;
    public event Action<TetrominoType> OnNextChanged;

    public TetrominoType NextType { get; private set; }
    public Vector2Int SpawnPosition => _spawnPosition;

    private void Awake()
    {
        NextType = PickRandom();
    }

    private static TetrominoType PickRandom()
    {
        return (TetrominoType)Random.Range(0, 7);
    }

    public void SetNextType(TetrominoType type)
    {
        NextType = type;
        OnNextChanged?.Invoke(NextType);
    }

    // Продвигает очередь без спауна тетромино (для BombNPC)
    public void ConsumeNext()
    {
        NextType = PickRandom();
        OnNextChanged?.Invoke(NextType);
    }

    public Tetromino SpawnNext()
    {
        TetrominoType type = NextType;
        NextType = PickRandom();
        OnNextChanged?.Invoke(NextType);
        return Spawn(type);
    }

    public Tetromino Spawn(TetrominoType type)
    {
        Tetromino tetromino = Instantiate(_tetrominoPrefab, _boardTransform);
        tetromino.transform.localPosition = Vector3.zero;
        tetromino.Init(type, _spawnPosition);

        OnSpawned?.Invoke(tetromino);
        return tetromino;
    }
}
