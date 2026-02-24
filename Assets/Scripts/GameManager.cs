using UnityEngine;

/// <summary>
/// MonoBehaviour: связывает все системы, следит за победой и поражением.
/// Прикрепи к пустому GameObject на сцене.
/// В Inspector привяжи ссылки на все компоненты.
/// </summary>
public class GameManager : MonoBehaviour
{
    [Header("Tetris")]
    [SerializeField] private TetrisController _tetrisController;
    [SerializeField] private BoardRenderer    _boardRenderer;

    [Header("Character")]
    [SerializeField] private CharacterController2D _character;
    [SerializeField] private BlockInteraction      _blockInteraction;

    [Header("UI")]
    [SerializeField] private GameUI _gameUI;

    [Header("Условие победы")]
    [SerializeField] private int _blocksToWin = 30;

    // ─── Private ──────────────────────────────────────────────────────────────

    private VictoryModel _victory;
    private bool         _gameEnded;

    // ─── Awake ────────────────────────────────────────────────────────────────

    private void Awake()
    {
        _victory = new VictoryModel(_blocksToWin);

        // Инициализация в порядке зависимостей
        _tetrisController.Init(_boardRenderer);
        _character.Init(_tetrisController.Board);
        _blockInteraction.Init(_tetrisController.Board, _victory, _character);
        _gameUI.Init(_victory);

        // Подписки
        _tetrisController.OnStateChanged      += OnTetrisStateChanged;
        _victory.OnCharacterWin               += OnCharacterWin;
        _victory.OnTetrisWin                  += OnTetrisWin;
        _tetrisController.Board.OnPiecePlaced += CheckCrush;

        // Персонаж не проходит сквозь активную (ещё падающую) фигуру
        _tetrisController.Board.ExtraOccupied =
            (x, y) => _tetrisController.IsActivePieceCell(x, y);
    }

    // ─── Update ───────────────────────────────────────────────────────────────

    private void Update()
    {
        if (_gameEnded) return;
        CheckActivePieceCrush();
    }

    // ─── Обработчики событий ──────────────────────────────────────────────────

    /// <summary>
    /// Доска переполнена — новую фигуру негде заспавнить.
    /// Фоллбэк: если CheckCrush не поймал crush раньше, фиксируем победу Тетриса.
    /// </summary>
    private void OnTetrisStateChanged()
    {
        if (_tetrisController.CurrentState == TetrisController.State.GameOver)
            TriggerTetrisWin();
    }

    /// <summary>
    /// Проверить crush: активная фигура занимает ячейку персонажа прямо сейчас.
    /// Вызывается каждый кадр из Update.
    /// </summary>
    private void CheckActivePieceCrush()
    {
        ActivePiece piece = _tetrisController.Current;
        if (piece == null) return;

        _tetrisController.Board.Grid.WorldToCell(
            _character.transform.position, out int cx, out int cy);

        foreach (var cell in piece.GetCells())
        {
            if (cell.x == cx && (cell.y == cy || cell.y == cy + 1))
            {
                TriggerTetrisWin();
                return;
            }
        }
    }

    /// <summary>
    /// Проверить crush: блок зафиксирован прямо на персонаже.
    /// Вызывается после каждого PlacePiece.
    /// </summary>
    private void CheckCrush()
    {
        if (_gameEnded) return;

        _tetrisController.Board.Grid.WorldToCell(
            _character.transform.position, out int cx, out int cy);

        // Персонаж занимает ~1 колонку и ~2 строки
        for (int dy = 0; dy <= 1; dy++)
        {
            if (_tetrisController.Board.IsCellOccupied(cx, cy + dy))
            {
                TriggerTetrisWin();
                return;
            }
        }
    }

    private void OnCharacterWin()
    {
        if (_gameEnded) return;
        _gameEnded = true;
        _tetrisController.Pause();
        Debug.Log("[GameManager] Hero wins!");
    }

    private void OnTetrisWin()
    {
        if (_gameEnded) return;
        _gameEnded = true;
        _tetrisController.SetGameOver();
        Debug.Log("[GameManager] Tetris wins!");
    }

    private void TriggerTetrisWin()
    {
        if (_gameEnded) return;
        _victory.TriggerTetrisWin();
    }
}
