using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] private GameObject _startPanel;
    [SerializeField] private GameObject _resultPanel;   // одна панель для победы и поражения
    [SerializeField] private TMP_Text   _resultText;    // текст внутри панели
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
        _resultPanel?.SetActive(false);
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
        _tetrisController?.StartGame();
        if (LocalDebug || !Photon.Pun.PhotonNetwork.IsMasterClient)
            _characterSpawner?.StartGame();
    }

    public void GameOver()
    {
        if (!_isPlaying) return;
        _isPlaying = false;
        ShowResult("Тетрис победил!");
    }

    public void Win()
    {
        if (!_isPlaying) return;
        _isPlaying = false;
        ShowResult("Персонаж победил!");
    }

    private void ShowResult(string message)
    {
        if (_resultText != null) _resultText.text = message;
        _resultPanel?.SetActive(true);
        Time.timeScale = 0f;
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
