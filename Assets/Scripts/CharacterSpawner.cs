using UnityEngine;
using Photon.Pun;

public class CharacterSpawner : MonoBehaviour
{
    [SerializeField] private string _characterPrefabName = "Character";

    public void StartGame()
    {
        PhotonNetwork.Instantiate(_characterPrefabName, transform.position, Quaternion.identity);
    }
}
