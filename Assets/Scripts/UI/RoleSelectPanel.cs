using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;

/// <summary>
/// Панель выбора роли. Оба игрока выбирают независимо.
/// При конфликте (оба выбрали одинаковое) — рандомное распределение.
/// </summary>
public class RoleSelectPanel : UIPanel, IInRoomCallbacks
{
    public const string PropRole    = "role";    // значения: "tetris" / "character"
    public const string PropReady   = "ready";   // значение: true
    public const string RoomStarted = "started"; // room property — сигнал старта
    public const string RoomRole    = "r_";      // префикс: r_{actorNumber} = роль

    [Header("Buttons")]
    [SerializeField] private Button _tetrisButton;
    [SerializeField] private Button _characterButton;

    private bool _chosen;
    private bool _resolved;

    private void Start()
    {
        if (_tetrisButton != null)
            _tetrisButton.onClick.AddListener(() => ChooseRole("tetris"));
        if (_characterButton != null)
            _characterButton.onClick.AddListener(() => ChooseRole("character"));
    }

    private void OnEnable()  => PhotonNetwork.AddCallbackTarget(this);
    private void OnDisable() => PhotonNetwork.RemoveCallbackTarget(this);

    private void ChooseRole(string role)
    {
        if (_chosen) return;
        _chosen = true;

        SetButtonsInteractable(false);

        Hashtable props = new Hashtable
        {
            { PropRole,  role },
            { PropReady, true }
        };
        PhotonNetwork.LocalPlayer.SetCustomProperties(props);
    }

    // ── Photon callbacks ────────────────────────────────────────────

    public void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
        // Если другой игрок уже выбрал роль — блокируем эту кнопку
        if (!targetPlayer.IsLocal && changedProps.ContainsKey(PropRole))
        {
            string takenRole = (string)changedProps[PropRole];
            if (_tetrisButton != null)
                _tetrisButton.interactable = takenRole != "tetris";
            if (_characterButton != null)
                _characterButton.interactable = takenRole != "character";
        }

        TryResolveRoles();
    }

    private void TryResolveRoles()
    {
        if (_resolved) return;

        Player[] players = PhotonNetwork.PlayerList;
        if (players.Length < 2) return;

        // Ждём, пока оба выбрали
        foreach (Player p in players)
        {
            bool hasReady = p.CustomProperties.ContainsKey(PropReady);
            Debug.Log($"[RoleSelect] TryResolve: actor={p.ActorNumber} hasReady={hasReady}");
            if (!hasReady) return;
        }

        // Только мастер-клиент разрешает конфликт
        if (!PhotonNetwork.IsMasterClient)
        {
            Debug.Log("[RoleSelect] Both ready but not master — waiting");
            return;
        }

        _resolved = true;
        Debug.Log("[RoleSelect] Master resolving roles");

        string role0 = (string)players[0].CustomProperties[PropRole];
        string role1 = (string)players[1].CustomProperties[PropRole];

        if (role0 == role1)
        {
            // Конфликт — рандомное распределение
            bool flip = Random.value > 0.5f;
            role0 = flip ? "tetris" : "character";
            role1 = flip ? "character" : "tetris";

            players[0].SetCustomProperties(new Hashtable { { PropRole, role0 } });
            players[1].SetCustomProperties(new Hashtable { { PropRole, role1 } });
        }

        // Упаковываем роли + сигнал старта в ОДИН SetCustomProperties
        // чтобы OnRoomPropertiesUpdate получил всё сразу
        PhotonNetwork.CurrentRoom.IsOpen = false;
        PhotonNetwork.CurrentRoom.SetCustomProperties(new Hashtable
        {
            { RoomStarted, true },
            { RoomRole + players[0].ActorNumber, role0 },
            { RoomRole + players[1].ActorNumber, role1 }
        });
    }

    private void SetButtonsInteractable(bool state)
    {
        if (_tetrisButton != null) _tetrisButton.interactable = state;
        if (_characterButton != null) _characterButton.interactable = state;
    }

    public override void Show()
    {
        base.Show();
        _chosen = false;
        _resolved = false;
        SetButtonsInteractable(true);
    }

    public void OnPlayerEnteredRoom(Player newPlayer) { }
    public void OnPlayerLeftRoom(Player otherPlayer) { }
    public void OnMasterClientSwitched(Player newMasterClient) { }

    public void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged) { }
}
