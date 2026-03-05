using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] private GameObject _startPanel;
    [SerializeField] private GameObject _gameOverPanel;
    [SerializeField] private GameObject _pausePanel;
    [SerializeField] private TetrisController _tetrisController;
    [SerializeField] private CharacterSpawner _characterSpawner;

    private bool _isPaused;
    private bool _isPlaying;

    private void Awake()
    {
        Instance = this;
        _gameOverPanel?.SetActive(false);
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
        if (!Photon.Pun.PhotonNetwork.IsMasterClient)
            _characterSpawner?.StartGame();
    }

    public void GameOver()
    {
        _isPlaying = false;
        _gameOverPanel.SetActive(true);
        Time.timeScale = 0f;
    }

    public void TogglePause()
    {
        _isPaused = !_isPaused;
        _pausePanel.SetActive(_isPaused);
        Time.timeScale = _isPaused ? 0f : 1f;
    }

    public void Restart()
    {
        Time.timeScale = 1f;
        Photon.Pun.PhotonNetwork.LeaveRoom();
    }
}
