using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class TetrominoSpawner : MonoBehaviour
{
    [SerializeField] private Tetromino _tetrominoPrefab;
    [SerializeField] private Transform _boardTransform;
    [SerializeField] private Vector2Int _spawnPosition = new Vector2Int(4, 18);

    public event Action<Tetromino> OnSpawned;

    public Tetromino SpawnNext()
    {
        TetrominoType type = (TetrominoType)Random.Range(0, 7);
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
