using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using TMPro;

public class LobbyManager : MonoBehaviourPunCallbacks
{
    [Header("UI")]
    [SerializeField] private GameObject _lobbyPanel;
    [SerializeField] private TMP_InputField _roomCodeInput;
    [SerializeField] private TextMeshProUGUI _statusText;
    [SerializeField] private Button _createButton;
    [SerializeField] private Button _joinButton;

    private void Start()
    {
        Debug.Log("LobbyManager Start");
        _createButton.onClick.AddListener(OnCreateRoomClicked);
        _joinButton.onClick.AddListener(OnJoinRoomClicked);

        _createButton.interactable = false;
        _joinButton.interactable = false;

        _lobbyPanel.SetActive(true);

        if (PhotonNetwork.IsConnectedAndReady)
        {
            _statusText.text = "Подключено. Введи код комнаты.";
            _createButton.interactable = true;
            _joinButton.interactable = true;
            return;
        }

        _statusText.text = "Подключение...";
        Debug.Log("Connecting to Photon...");
        PhotonNetwork.ConnectUsingSettings();
        StartCoroutine(LogNetworkState());
    }

    private System.Collections.IEnumerator LogNetworkState()
    {
        Debug.Log("[Photon State] Coroutine started");
        for (int i = 0; i < 20; i++)
        {
            yield return new WaitForSeconds(2f);
            Debug.Log($"[Photon State] {i*2}s: {PhotonNetwork.NetworkClientState} | IsConnected: {PhotonNetwork.IsConnected} | IsConnectedAndReady: {PhotonNetwork.IsConnectedAndReady}");
        }
    }

    public override void OnConnectedToMaster()
    {
        _statusText.text = "Подключено. Введи код комнаты.";
        _createButton.interactable = true;
        _joinButton.interactable = true;
    }

    public void OnCreateRoomClicked()
    {
        string code = _roomCodeInput.text.ToUpper();
        if (string.IsNullOrEmpty(code)) return;

        RoomOptions options = new RoomOptions { MaxPlayers = 2 };
        PhotonNetwork.CreateRoom(code, options);
        _statusText.text = "Создание комнаты...";
    }

    public void OnJoinRoomClicked()
    {
        string code = _roomCodeInput.text.ToUpper();
        if (string.IsNullOrEmpty(code)) return;

        PhotonNetwork.JoinRoom(code);
        _statusText.text = "Вход в комнату...";
    }

    public override void OnJoinedRoom()
    {
        if (PhotonNetwork.CurrentRoom.PlayerCount == 2)
        {
            StartTheGame();
        }
        else
        {
            _statusText.text = "Ожидание второго игрока...";
        }
    }

    public override void OnLeftRoom()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
    }

    public override void OnDisconnected(Photon.Realtime.DisconnectCause cause)
    {
        Debug.LogError($"Disconnected: {cause}");
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        _statusText.text = "Комната уже существует!";
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        _statusText.text = "Комната не найдена!";
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        if (PhotonNetwork.CurrentRoom.PlayerCount == 2)
            StartTheGame();
    }

    private void StartTheGame()
    {
        _lobbyPanel.SetActive(false);
        GameManager.Instance.StartGame();
    }
}
