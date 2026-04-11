using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;

public class GameManager : MonoBehaviour, IInRoomCallbacks
{
    public static GameManager Instance { get; private set; }

    [SerializeField] private TetrisController _tetrisController;
    [SerializeField] private CharacterSpawner _characterSpawner;
    [SerializeField] private TetrisAI         _tetrisAI;
    [SerializeField] private SoloBoardSetup   _soloBoardSetup;

    [Header("Gameplay Objects")]
    [SerializeField] private GameObject _grid;
    [SerializeField] private GameObject _exit;

    [Header("Debug")]
    [SerializeField] private bool _localDebugMode;
    public static bool LocalDebug { get; private set; }
    public static bool SoloMode   { get; private set; }

    public static bool IsTetrisPlayer()
    {
        if (SoloMode)  return false; // в Solo игрок всегда персонаж
        if (LocalDebug) return true;
        int actorNum = Photon.Pun.PhotonNetwork.LocalPlayer.ActorNumber;
        string roleKey = RoleSelectPanel.RoomRole + actorNum;
        Photon.Pun.PhotonNetwork.CurrentRoom.CustomProperties
            .TryGetValue(roleKey, out object roleObj);
        return (roleObj as string) == "tetris";
    }

    private bool _isPaused;
    private bool _isPlaying;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        LocalDebug = _localDebugMode;
        Time.timeScale = 1f;
    }

    private void OnEnable()  => PhotonNetwork.AddCallbackTarget(this);
    private void OnDisable() => PhotonNetwork.RemoveCallbackTarget(this);

    private void Update()
    {
        if (_isPlaying && Input.GetKeyDown(KeyCode.Escape))
            TogglePause();
    }

    // ── IInRoomCallbacks ─────────────────────────────────────
    // GameManager сам слушает сигнал старта — не зависит от активной панели

    public void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
    {
        if (propertiesThatChanged.ContainsKey(RoleSelectPanel.RoomStarted))
        {
            Debug.Log("[GameManager] OnRoomPropertiesUpdate: started signal received");
            StartGame();
        }
    }

    public void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps) { }
    public void OnPlayerEnteredRoom(Player newPlayer) { }
    public void OnPlayerLeftRoom(Player otherPlayer) { }
    public void OnMasterClientSwitched(Player newMasterClient) { }

    // ── Game Flow ─────────────────────────────────────────────

    public void StartSolo()
    {
        if (_isPlaying) return;
        SoloMode   = true;
        _isPlaying = true;
        Time.timeScale = 1f;
        _tetrisController?.ResetBoard();
        _characterSpawner?.Reset();
        _grid?.SetActive(true);
        _exit?.SetActive(true);
        _soloBoardSetup?.gameObject.SetActive(true);
        _tetrisAI?.gameObject.SetActive(true);
        UIManager.Instance?.ShowGameHud();
        GameHUD.Instance?.StartTimer();
        _soloBoardSetup?.Setup();   // сначала заполняем поле
        _characterSpawner?.StartGame();
        _tetrisController?.StartGame();
        _tetrisAI?.StartAI();
    }

    public void StartGame()
    {
        if (_isPlaying) return;
        _isPlaying = true;
        Time.timeScale = 1f;
        _tetrisController?.ResetBoard();
        _characterSpawner?.Reset();
        _grid?.SetActive(true);
        _exit?.SetActive(true);
        UIManager.Instance?.ShowGameHud();
        GameHUD.Instance?.StartTimer();

        if (LocalDebug)
        {
            _tetrisController?.StartGame();
            _characterSpawner?.StartGame();
            return;
        }

        int actorNum = PhotonNetwork.LocalPlayer.ActorNumber;
        string roleKey = RoleSelectPanel.RoomRole + actorNum;
        PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue(roleKey, out object roleObj);
        string role = roleObj as string;

        Debug.Log($"[GameManager] StartGame: actor={actorNum} role={role}");

        if (role == "tetris")
            _tetrisController?.StartGame();
        else if (role == "character")
        {
            _characterSpawner?.StartGame();
            _tetrisController?.InitForViewing();
        }
        else
        {
            bool isMaster = PhotonNetwork.IsMasterClient;
            Debug.LogWarning($"[GameManager] Role not found, fallback isMaster={isMaster}");
            if (isMaster) _tetrisController?.StartGame();
            else _characterSpawner?.StartGame();
        }
    }

    public void GameOver()
    {
        if (!_isPlaying) return;
        _isPlaying = false;
        Time.timeScale = 0f;
        GameHUD.Instance?.StopTimer();
        UIManager.Instance?.ShowEndGameBlocs();
    }

    public void Win()
    {
        if (!_isPlaying) return;
        _isPlaying = false;
        Time.timeScale = 0f;
        GameHUD.Instance?.StopTimer();
        UIManager.Instance?.ShowEndGameCharacter();
    }

    public void TogglePause()
    {
        _isPaused = !_isPaused;
        if (_isPaused)
            UIManager.Instance?.ShowSettings();
        else
            UIManager.Instance?.ShowGameHud();
        Time.timeScale = _isPaused ? 0f : 1f;
    }

    public void Restart()
    {
        bool wasSolo = SoloMode;
        SoloMode = false;
        Time.timeScale = 1f;
        if (LocalDebug || wasSolo)
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        else
            PhotonNetwork.LeaveRoom();
    }
}
