using UnityEngine;
using TMPro;

/// <summary>
/// MonoBehaviour: счётчик блоков, таймер, экран победы.
/// Прикрепи к Canvas; вызови Init(victory) из GameManager.
/// </summary>
public class GameUI : MonoBehaviour
{
    [Header("Счётчик блоков")]
    [SerializeField] private TextMeshProUGUI _blocksDestroyedText;
    [SerializeField] private TextMeshProUGUI _blocksToWinText;

    [Header("Таймер")]
    [SerializeField] private TextMeshProUGUI _timerText;

    [Header("Game Over")]
    [SerializeField] private GameObject      _gameOverPanel;
    [SerializeField] private TextMeshProUGUI _winnerText;

    private float _elapsed;
    private bool  _running;

    // ─── Инициализация ────────────────────────────────────────────────────────

    public void Init(VictoryModel victory)
    {
        _blocksToWinText.text = $"/ {victory.BlocksToWin}";
        SetBlocksCount(0);
        _gameOverPanel.SetActive(false);
        _running = true;

        victory.OnBlocksDestroyedChanged += SetBlocksCount;
        victory.OnCharacterWin += () => ShowWinner("Hero wins!");
        victory.OnTetrisWin    += () => ShowWinner("Tetris wins!");
    }

    // ─── Update ───────────────────────────────────────────────────────────────

    private void Update()
    {
        if (!_running) return;
        _elapsed += Time.deltaTime;
        int m = (int)(_elapsed / 60f);
        int s = (int)(_elapsed % 60f);
        _timerText.text = $"{m:00}:{s:00}";
    }

    // ─── Private ──────────────────────────────────────────────────────────────

    private void SetBlocksCount(int count)
    {
        _blocksDestroyedText.text = count.ToString();
    }

    private void ShowWinner(string message)
    {
        _running = false;
        _gameOverPanel.SetActive(true);
        _winnerText.text = message;
    }
}
