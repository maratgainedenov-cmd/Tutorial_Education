using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using Photon.Pun;

/// <summary>
/// Главное меню: ИГРАТЬ, ЛОББИ, НАСТРОЙКИ, ВЫЙТИ.
/// TETRIS vs CHARACTER заголовок с двухцветным текстом (P1 синий + P2 оранжевый).
/// </summary>
public class MainMenuUI : MonoBehaviour
{
    [Header("Title")]
    [SerializeField] private TMP_Text _titleText;
    [SerializeField] private TMP_Text _versionText;

    [Header("Buttons")]
    [SerializeField] private Button _playButton;
    [SerializeField] private Button _lobbyButton;
    [SerializeField] private Button _settingsButton;
    [SerializeField] private Button _quitButton;

    [Header("Panels")]
    [SerializeField] private GameObject _mainPanel;
    [SerializeField] private GameObject _settingsPanel;
    [SerializeField] private GameObject _lobbyPanel;

    private void Start()
    {
        if (_versionText != null)
            _versionText.text = $"v{Application.version}  Unity {Application.unityVersion.Split('.')[0]}.x  Photon PUN2";

        _playButton?.onClick.AddListener(OnPlay);
        _lobbyButton?.onClick.AddListener(OnLobby);
        _settingsButton?.onClick.AddListener(OnSettings);
        _quitButton?.onClick.AddListener(OnQuit);

        // Пульсация заголовка
        _titleText?.transform
            .DOScale(1.03f, 1.5f)
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(Ease.InOutSine);
    }

    private void OnPlay()
    {
        if (GameManager.LocalDebug)
        {
            GameManager.Instance?.StartGame();
        }
        else
        {
            // Быстрая игра — создать или зайти в случайную комнату
            if (PhotonNetwork.IsConnected)
                PhotonNetwork.JoinRandomOrCreateRoom();
        }
    }

    private void OnLobby()
    {
        _mainPanel?.SetActive(false);
        _lobbyPanel?.SetActive(true);
    }

    private void OnSettings()
    {
        _mainPanel?.SetActive(false);
        _settingsPanel?.SetActive(true);
    }

    private void OnQuit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public void BackToMain()
    {
        _settingsPanel?.SetActive(false);
        _lobbyPanel?.SetActive(false);
        _mainPanel?.SetActive(true);
    }
}
