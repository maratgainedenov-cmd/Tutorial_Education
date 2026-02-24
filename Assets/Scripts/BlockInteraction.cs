using UnityEngine;

/// <summary>
/// MonoBehaviour: связывает атаку персонажа с Grid2D и VictoryModel.
/// Слабые блоки → удар → разрушение → очки.
/// Очищенные линии тоже засчитываются персонажу.
/// Вызови Init(...) из GameManager.
/// </summary>
public class BlockInteraction : MonoBehaviour
{
    private TetrisBoard           _board;
    private VictoryModel          _victory;
    private CharacterController2D _character;

    public void Init(TetrisBoard board, VictoryModel victory, CharacterController2D character)
    {
        _board     = board;
        _victory   = victory;
        _character = character;

        character.OnAttack       += HandleAttack;
        board.OnLinesCleared     += HandleLinesCleared;
    }

    // ─── Обработчики ──────────────────────────────────────────────────────────

    /// <summary>
    /// Персонаж ударил в направлении dir.
    /// Проверяем ячейки (x, y) и (x, y+1) — персонаж занимает ~2 клетки в высоту.
    /// Если попал по сильному блоку — вызывает анимацию отдачи.
    /// </summary>
    private void HandleAttack(int x, int y, int dir)
    {
        bool hitStrong = false;

        for (int dy = 0; dy <= 1; dy++)
        {
            if (!_board.Grid.IsValid(x, y + dy)) continue;

            CellData cell = _board.Grid.GetValue(x, y + dy);
            if (cell.IsEmpty) continue;

            if (!cell.IsWeak)
            {
                hitStrong = true;
                continue;
            }

            bool destroyed = _board.DamageCell(x, y + dy);
            if (destroyed)
                _victory.AddDestroyedBlocks(1);
        }

        if (hitStrong)
            _character.PlayHitStrong();
    }

    /// <summary>
    /// Тетрис очистил линии → засчитываем персонажу (ширина × кол-во линий блоков).
    /// Геймплейный нюанс: Тетрис, очищая линии, помогает персонажу.
    /// </summary>
    private void HandleLinesCleared(int linesCount)
    {
        int blocks = _board.Width * linesCount;
        _victory.AddDestroyedBlocks(blocks);
    }
}
