using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Экран конца игры для тетриса (P1).
/// </summary>
public class EndGameBlocsPanel : UIPanel
{
    [Header("UI")]
    [SerializeField] private TextMeshProUGUI _resultText;
    [SerializeField] private Button _menuButton;

    private void Start()
    {
        _menuButton.onClick.AddListener(OnMenu);
    }

    public void ShowWin()
    {
        Show();
        if (_resultText != null) _resultText.text = "ТЕТРИС ПОБЕДИЛ!";
    }

    public void ShowLose()
    {
        Show();
        if (_resultText != null) _resultText.text = "ТЕТРИС ПРОИГРАЛ";
    }

    private void OnMenu() => UIManager.Instance.ShowMainMenu();
}
