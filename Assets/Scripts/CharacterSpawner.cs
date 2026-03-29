using UnityEngine;
using Photon.Pun;

public class CharacterSpawner : MonoBehaviour
{
    [SerializeField] private string _characterPrefabName = "Character";

    private bool _spawned;

    public void StartGame()
    {
        if (_spawned) return;
        _spawned = true;
        if (GameManager.LocalDebug)
        {
            var prefab = Resources.Load<GameObject>(_characterPrefabName);
            Instantiate(prefab, transform.position, Quaternion.identity);
        }
        else
        {
            PhotonNetwork.Instantiate(_characterPrefabName, transform.position, Quaternion.identity);
        }
    }
}
