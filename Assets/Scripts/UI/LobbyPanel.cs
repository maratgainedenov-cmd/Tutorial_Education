using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using ExitGames.Client.Photon;

/// <summary>
/// Лобби: создание комнаты и список доступных комнат.
/// </summary>
public class LobbyPanel : UIPanel, ILobbyCallbacks, IMatchmakingCallbacks, IConnectionCallbacks
{
    [Header("Create Room")]
    [SerializeField] private TMP_InputField _roomNameInput;
    [SerializeField] private Button _createButton;

    [Header("Room List")]
    [SerializeField] private Transform _roomListContent;
    [SerializeField] private RoomRow _roomRowPrefab;

    [Header("Navigation")]
    [SerializeField] private Button _backButton;
    [SerializeField] private TextMeshProUGUI _statusText;

    private readonly List<RoomRow> _rows = new();

    private void Start()
    {
        _createButton.onClick.AddListener(OnCreateClicked);
        _backButton.onClick.AddListener(OnBackClicked);
    }

    public override void Show()
    {
        base.Show();
        SetStatus("Подключение...");
        _createButton.interactable = false;

        if (PhotonNetwork.IsConnectedAndReady
            && PhotonNetwork.Server == ServerConnection.MasterServer)
        {
            // Уже на Master Server — сразу заходим в лобби
            SetStatus("Выберите или создайте комнату.");
            _createButton.interactable = true;
            PhotonNetwork.JoinLobby();
        }
        else if (!PhotonNetwork.IsConnected)
        {
            // Первый запуск — подключаемся
            PhotonNetwork.ConnectUsingSettings();
        }
        // Иначе: клиент ещё на Game Server после LeaveRoom —
        // ждём OnConnectedToMaster который придёт автоматически
    }

    public override void Hide()
    {
        base.Hide();
        if (PhotonNetwork.InLobby)
            PhotonNetwork.LeaveLobby();
    }

    // ── Photon callbacks ──────────────────────────────────────

    public void OnJoinedLobby()
    {
        SetStatus("Выберите или создайте комнату.");
        _createButton.interactable = true;
    }

    public void OnLeftLobby() { }

    public void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        ClearRows();
        foreach (RoomInfo info in roomList)
        {
            if (info.RemovedFromList || !info.IsOpen || !info.IsVisible) continue;
            RoomRow row = Instantiate(_roomRowPrefab, _roomListContent);
            row.Setup(info.Name, info.PlayerCount, info.MaxPlayers);
            _rows.Add(row);
        }
    }

    public void OnLobbyStatisticsUpdate(List<TypedLobbyInfo> lobbyStatistics) { }

    // ── IConnectionCallbacks ──────────────────────────────────

    public void OnConnectedToMaster()
    {
        PhotonNetwork.JoinLobby();
    }

    public void OnConnected() { }
    public void OnDisconnected(DisconnectCause cause) { SetStatus($"Отключён: {cause}"); }
    public void OnRegionListReceived(RegionHandler regionHandler) { }
    public void OnCustomAuthenticationResponse(Dictionary<string, object> data) { }
    public void OnCustomAuthenticationFailed(string debugMessage) { }

    // ── IMatchmakingCallbacks ─────────────────────────────────

    public void OnJoinedRoom()
    {
        UIManager.Instance?.ShowWaiting(true);
    }

    public void OnCreatedRoom() { }
    public void OnCreateRoomFailed(short returnCode, string message)
    {
        SetStatus($"Ошибка: {message}");
        _createButton.interactable = true;
    }
    public void OnJoinRoomFailed(short returnCode, string message)
    {
        SetStatus($"Ошибка: {message}");
    }
    public void OnJoinRandomFailed(short returnCode, string message) { }
    public void OnLeftRoom() { }
    public void OnFriendListUpdate(List<Photon.Realtime.FriendInfo> friendList) { }

    // ── Кнопки ───────────────────────────────────────────────

    private void OnCreateClicked()
    {
        string name = _roomNameInput.text.Trim();
        if (string.IsNullOrEmpty(name)) return;

        RoomOptions options = new RoomOptions { MaxPlayers = 2 };
        PhotonNetwork.CreateRoom(name, options);
        SetStatus("Создание комнаты...");
        _createButton.interactable = false;
    }

    private void OnBackClicked()
    {
        UIManager.Instance.ShowMainMenu();
    }

    // ── Утилиты ──────────────────────────────────────────────

    private void ClearRows()
    {
        foreach (RoomRow row in _rows)
            if (row != null) Destroy(row.gameObject);
        _rows.Clear();
    }

    private void SetStatus(string text)
    {
        if (_statusText != null)
            _statusText.text = text;
    }

    private void OnEnable()  => PhotonNetwork.AddCallbackTarget(this);
    private void OnDisable() => PhotonNetwork.RemoveCallbackTarget(this);
}
