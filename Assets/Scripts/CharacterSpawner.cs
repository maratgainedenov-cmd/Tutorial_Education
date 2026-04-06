using UnityEngine;
using Photon.Pun;

public class CharacterSpawner : MonoBehaviour
{
    [SerializeField] private string    _characterPrefabName = "Character";
    [SerializeField] private Transform _soloSpawnPoint;

    private bool _spawned;

    public void StartGame()
    {
        if (_spawned) return;
        _spawned = true;

        Vector3 spawnPos = (GameManager.SoloMode && _soloSpawnPoint != null)
            ? _soloSpawnPoint.position
            : transform.position;

        if (GameManager.LocalDebug || GameManager.SoloMode)
        {
            var prefab = Resources.Load<GameObject>(_characterPrefabName);
            Instantiate(prefab, spawnPos, Quaternion.identity);
        }
        else
        {
            PhotonNetwork.Instantiate(_characterPrefabName, spawnPos, Quaternion.identity);
        }
    }
}
