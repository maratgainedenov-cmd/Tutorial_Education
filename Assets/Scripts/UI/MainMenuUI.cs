using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using Photon.Pun;

/// <summary>
/// Главное меню: ИГРАТЬ, ЛОББИ, НАСТРОЙКИ, ВЫЙТИ.
/// </summary>
public class MainMenuUI : UIPanel
{
    [Header("Title")]
    [SerializeField] private TMP_Text _titleText;
    [SerializeField] private TMP_Text _versionText;

    [Header("Buttons")]
    [SerializeField] private Button _playButton;
    [SerializeField] private Button _soloButton;
    [SerializeField] private Button _lobbyButton;
    [SerializeField] private Button _settingsButton;
    [SerializeField] private Button _quitButton;

    private void Start()
    {
        if (_versionText != null)
            _versionText.text = $"v{Application.version}  Unity {Application.unityVersion.Split('.')[0]}.x  Photon PUN2";

        _playButton?.onClick.AddListener(OnPlay);
        _soloButton?.onClick.AddListener(OnSolo);
        _lobbyButton?.onClick.AddListener(OnLobby);
        _settingsButton?.onClick.AddListener(OnSettings);
        _quitButton?.onClick.AddListener(OnQuit);

        _titleText?.transform
            .DOScale(1.03f, 1.5f)
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(Ease.InOutSine);
    }

    private void OnPlay()
    {
        if (GameManager.LocalDebug)
            GameManager.Instance?.StartGame();
        else if (PhotonNetwork.IsConnected)
            PhotonNetwork.JoinRandomOrCreateRoom();
    }

    private void OnSolo()     => GameManager.Instance?.StartSolo();
    private void OnLobby()    => UIManager.Instance.ShowLobby();
    private void OnSettings() => UIManager.Instance.ShowSettings();

    private void OnQuit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
