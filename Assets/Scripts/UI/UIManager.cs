using UnityEngine;

/// <summary>
/// Центральный менеджер UI. Переключает панели.
/// Повесить на Canvas.
/// </summary>
public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("Panels")]
    [SerializeField] private UIPanel _mainMenuPanel;
    [SerializeField] private UIPanel _gameHudPanel;
    [SerializeField] private UIPanel _lobbyPanel;
    [SerializeField] private UIPanel _waitingPanel;
    [SerializeField] private UIPanel _roleSelectPanel;
    [SerializeField] private UIPanel _settingsPanel;
    [SerializeField] private UIPanel _endGameCharacterPanel;
    [SerializeField] private UIPanel _endGameBlocsPanel;

    private UIPanel _current;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        ShowMainMenu();
    }

    public void ShowMainMenu()         => Show(_mainMenuPanel);
    public void ShowGameHud()          => Show(_gameHudPanel);
    public void ShowLobby()            => Show(_lobbyPanel);
    public void ShowWaiting(bool show) => Show(show ? _waitingPanel : _roleSelectPanel);
    public void ShowRoleSelect()       => Show(_roleSelectPanel);
    public void ShowSettings()         => Show(_settingsPanel);
    public void ShowEndGameCharacter() => Show(_endGameCharacterPanel);
    public void ShowEndGameBlocs()     => Show(_endGameBlocsPanel);

    private void Show(UIPanel panel)
    {
        _current?.Hide();
        _current = panel;
        _current?.Show();
    }
}
