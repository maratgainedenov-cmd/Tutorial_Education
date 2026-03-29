using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;

/// <summary>
/// Панель ожидания соперника. Показывается сразу после входа в комнату.
/// Как только заходит второй игрок — переключает на RoleSelectPanel.
/// </summary>
public class WaitingPanel : UIPanel, IInRoomCallbacks
{
    [Header("Room Info")]
    [SerializeField] private TMP_Text _roomNameText;
    [SerializeField] private TMP_Text _statusText;

    [Header("Player Slots")]
    [SerializeField] private TMP_Text _player1Label;
    [SerializeField] private TMP_Text _player2Label;

    [Header("Navigation")]
    [SerializeField] private Button _leaveButton;

    private void Start()
    {
        _leaveButton.onClick.AddListener(OnLeaveClicked);
    }

    private void OnEnable()  => PhotonNetwork.AddCallbackTarget(this);
    private void OnDisable() => PhotonNetwork.RemoveCallbackTarget(this);

    public override void Show()
    {
        base.Show();
        Refresh();
    }

    private void Refresh()
    {
        if (!PhotonNetwork.InRoom) return;

        if (_roomNameText != null)
            _roomNameText.text = PhotonNetwork.CurrentRoom.Name;

        UpdateStatus();
    }

    private void UpdateStatus()
    {
        int count = PhotonNetwork.CurrentRoom.PlayerCount;
        Debug.Log($"[WaitingPanel] UpdateStatus: count={count}");

        if (_statusText != null)
            _statusText.text = count >= 2 ? "Соперник найден!" : "Ожидание соперника...";

        if (_player2Label != null)
            _player2Label.text = count >= 2 ? "Игрок 2" : "Ожидание...";

        if (count >= 2)
        {
            Debug.Log("[WaitingPanel] 2 players — switching to RoleSelect");
            UIManager.Instance?.ShowRoleSelect();
        }
    }

    private void OnLeaveClicked()
    {
        PhotonNetwork.LeaveRoom();
        UIManager.Instance?.ShowLobby();
    }

    // ── IInRoomCallbacks ──────────────────────────────────────

    public void OnPlayerEnteredRoom(Player newPlayer) => UpdateStatus();

    public void OnPlayerLeftRoom(Player otherPlayer) { }
    public void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged) { }
    public void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps) { }
    public void OnMasterClientSwitched(Player newMasterClient) { }
}
