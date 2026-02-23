using System;

/// <summary>
/// Счётчик разбитых блоков и условия победы.
/// Чистый C# — не MonoBehaviour.
/// </summary>
public class VictoryModel
{
    public int BlocksDestroyed { get; private set; }
    public int BlocksToWin     { get; private set; }

    public event Action<int> OnBlocksDestroyedChanged;
    public event Action      OnCharacterWin;
    public event Action      OnTetrisWin;

    public VictoryModel(int blocksToWin)
    {
        BlocksToWin = blocksToWin;
    }

    /// <summary>Добавить разбитые блоки. Если достигнут порог — победа персонажа.</summary>
    public void AddDestroyedBlocks(int count)
    {
        BlocksDestroyed += count;
        OnBlocksDestroyedChanged?.Invoke(BlocksDestroyed);

        if (BlocksDestroyed >= BlocksToWin)
            OnCharacterWin?.Invoke();
    }

    /// <summary>Вызвать победу Тетриса (персонаж раздавлен).</summary>
    public void TriggerTetrisWin() => OnTetrisWin?.Invoke();
}
