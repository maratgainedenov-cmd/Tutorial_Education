using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;

/// <summary>
/// Одна строка в списке комнат лобби.
/// Повесить на префаб RoomRow.
/// </summary>
public class RoomRow : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _roomNameText;
    [SerializeField] private TextMeshProUGUI _countText;
    [SerializeField] private Button _enterButton;

    private string _roomName;

    public void Setup(string roomName, int playerCount, int maxPlayers)
    {
        _roomName = roomName;
        if (_roomNameText != null) _roomNameText.text = roomName;
        if (_countText != null)    _countText.text    = $"{playerCount} / {maxPlayers}";

        bool isFull = playerCount >= maxPlayers;
        if (_enterButton != null)
        {
            _enterButton.interactable = !isFull;
            _enterButton.onClick.AddListener(OnEnterClicked);
        }
    }

    private void OnEnterClicked()
    {
        PhotonNetwork.JoinRoom(_roomName);
    }
}
