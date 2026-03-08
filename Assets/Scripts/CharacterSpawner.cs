using UnityEngine;
using Photon.Pun;

public class CharacterSpawner : MonoBehaviour
{
    [SerializeField] private string _characterPrefabName = "Character";

    public void StartGame()
    {
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
