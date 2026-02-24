using System;
using UnityEngine;

/// <summary>
/// MonoBehaviour: ввод тетрис-игрока, автопадение, спавн фигур.
/// State machine: Playing / Paused / GameOver.
/// Вызови Init(renderer) из GameManager.
/// </summary>
public class TetrisController : MonoBehaviour
{
    // ─── Настройки (Inspector) ────────────────────────────────────────────────

    [Header("Размер доски")]
    [SerializeField] private int   _boardWidth   = 10;
    [SerializeField] private int   _boardHeight  = 20;
    [SerializeField] private float _cellSize     = 1f;

    [Header("Падение")]
    [SerializeField] private float _fallInterval = 1f;      // секунд до шага вниз
    [SerializeField] private float _softDropMult = 0.08f;   // ускорение при удержании ↓

    [Header("Клавиши")]
    [SerializeField] private KeyCode _keyLeft      = KeyCode.LeftArrow;
    [SerializeField] private KeyCode _keyRight     = KeyCode.RightArrow;
    [SerializeField] private KeyCode _keyRotateCW  = KeyCode.UpArrow;
    [SerializeField] private KeyCode _keyRotateCCW = KeyCode.Z;
    [SerializeField] private KeyCode _keySoftDrop  = KeyCode.DownArrow;
    [SerializeField] private KeyCode _keyHardDrop  = KeyCode.Space;

    [Header("Блоки")]
    [SerializeField] private int _blockHP = 3;

    // ─── Публичный доступ ─────────────────────────────────────────────────────

    public TetrisBoard  Board         { get; private set; }
    public ActivePiece  Current       { get; private set; }
    public ActivePiece  Ghost         { get; private set; }

    public event Action OnStateChanged;

    // ─── State Machine ────────────────────────────────────────────────────────

    public enum State { Playing, Paused, GameOver }
    public State CurrentState { get; private set; } = State.Playing;

    // ─── Private ──────────────────────────────────────────────────────────────

    private float         _fallTimer;
    private BoardRenderer _renderer;

    // ─── Инициализация ────────────────────────────────────────────────────────

    public void Init(BoardRenderer renderer)
    {
        _renderer = renderer;
        Board     = new TetrisBoard(_boardWidth, _boardHeight, _cellSize, transform.position);
        renderer.Init(Board);
        SpawnPiece();
    }

    // ─── Unity Update ─────────────────────────────────────────────────────────

    private void Update()
    {
        if (CurrentState != State.Playing) return;

        HandleInput();

        float interval = Input.GetKey(_keySoftDrop)
            ? _fallInterval * _softDropMult
            : _fallInterval;

        _fallTimer += Time.deltaTime;
        if (_fallTimer >= interval)
        {
            _fallTimer = 0f;
            StepDown();
        }

        UpdateGhost();
        _renderer?.RenderActivePiece(Current, Ghost);
    }

    // ─── Ввод ─────────────────────────────────────────────────────────────────

    private void HandleInput()
    {
        if (Input.GetKeyDown(_keyLeft))      TryMove(-1, 0);
        if (Input.GetKeyDown(_keyRight))     TryMove(1,  0);
        if (Input.GetKeyDown(_keyRotateCW))  TryRotate(1);
        if (Input.GetKeyDown(_keyRotateCCW)) TryRotate(-1);
        if (Input.GetKeyDown(_keyHardDrop))  HardDrop();
    }

    // ─── Движение ─────────────────────────────────────────────────────────────

    private void TryMove(int dx, int dy)
    {
        Current.Move(dx, dy);
        if (!Board.IsValidPosition(Current))
            Current.Move(-dx, -dy);
    }

    private void TryRotate(int dir)
    {
        Current.Rotate(dir);
        if (!Board.IsValidPosition(Current))
            Current.Rotate(-dir);
    }

    private void StepDown()
    {
        Current.Move(0, -1);
        if (!Board.IsValidPosition(Current))
        {
            Current.Move(0, 1);
            LockPiece();
        }
    }

    private void HardDrop()
    {
        while (Board.IsValidPosition(Current))
            Current.Move(0, -1);
        Current.Move(0, 1);
        LockPiece();
    }

    private void LockPiece()
    {
        Board.PlacePiece(Current);
        Board.ClearLines();
        SpawnPiece();
    }

    // ─── Ghost piece ──────────────────────────────────────────────────────────

    private void UpdateGhost()
    {
        if (Current == null) return;
        Ghost = new ActivePiece(Current.Type, Current.Pos, Current.HP);
        Ghost.SetRotation(Current.Rotation);

        while (Board.IsValidPosition(Ghost))
            Ghost.Move(0, -1);
        Ghost.Move(0, 1);
    }

    // ─── Спавн ────────────────────────────────────────────────────────────────

    private void SpawnPiece()
    {
        int type     = UnityEngine.Random.Range(0, TetrominoData.Count);
        var spawnPos = new Vector2Int(_boardWidth / 2 - 2, _boardHeight - 4);

        Current = new ActivePiece(type, spawnPos, _blockHP);

        if (!Board.CanSpawn(Current))
        {
            CurrentState = State.GameOver;
            OnStateChanged?.Invoke();
        }
    }

    // ─── Вспомогательные ─────────────────────────────────────────────────────

    /// <summary>
    /// Проверить, занята ли ячейка активной (ещё не зафиксированной) фигурой.
    /// Используется в TetrisBoard.ExtraOccupied для коллизий персонажа.
    /// </summary>
    public bool IsActivePieceCell(int x, int y)
    {
        if (Current == null) return false;
        foreach (var cell in Current.GetCells())
            if (cell.x == x && cell.y == y) return true;
        return false;
    }

    // ─── Управление состоянием (GameManager) ─────────────────────────────────

    public void Pause()      { CurrentState = State.Paused;   OnStateChanged?.Invoke(); }
    public void Resume()     { CurrentState = State.Playing;  OnStateChanged?.Invoke(); }
    public void SetGameOver(){ CurrentState = State.GameOver; OnStateChanged?.Invoke(); }
}
