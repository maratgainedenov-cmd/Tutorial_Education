using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] private GameObject _startPanel;
    [SerializeField] private VictoryPanel _victoryPanel;
    [SerializeField] private GameObject _pausePanel;
    [SerializeField] private TetrisController _tetrisController;
    [SerializeField] private CharacterSpawner _characterSpawner;

    [Header("Debug")]
    [SerializeField] private bool _localDebugMode;
    public static bool LocalDebug { get; private set; }

    private bool _isPaused;
    private bool _isPlaying;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        LocalDebug = _localDebugMode;
        _pausePanel?.SetActive(false);
        _startPanel?.SetActive(true);
        Time.timeScale = 1f;
    }

    private void Update()
    {
        if (_isPlaying && Input.GetKeyDown(KeyCode.Escape))
            TogglePause();
    }

    public void StartGame()
    {
        _startPanel?.SetActive(false);
        _isPlaying = true;
        Time.timeScale = 1f;
        GameHUD.Instance?.StartTimer();
        _tetrisController?.StartGame();
        if (LocalDebug || !Photon.Pun.PhotonNetwork.IsMasterClient)
            _characterSpawner?.StartGame();
    }

    public void GameOver()
    {
        if (!_isPlaying) return;
        _isPlaying = false;
        Time.timeScale = 0f;
        GameHUD.Instance?.StopTimer();
        _victoryPanel?.ShowP1Wins();
    }

    public void Win()
    {
        if (!_isPlaying) return;
        _isPlaying = false;
        Time.timeScale = 0f;
        GameHUD.Instance?.StopTimer();
        _victoryPanel?.ShowP2Wins();
    }

    public void TogglePause()
    {
        _isPaused = !_isPaused;
        _pausePanel?.SetActive(_isPaused);
        Time.timeScale = _isPaused ? 0f : 1f;
    }

    public void Restart()
    {
        Time.timeScale = 1f;
        if (LocalDebug)
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        else
            Photon.Pun.PhotonNetwork.LeaveRoom();
    }
}
