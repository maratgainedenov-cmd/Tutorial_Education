using UnityEngine;
using Photon.Pun;

public class CharacterSpawner : MonoBehaviour
{
    [SerializeField] private string    _characterPrefabName = "Character";
    [SerializeField] private Transform _soloSpawnPoint;

    private bool      _spawned;
    private GameObject _spawnedCharacter;

    public void Reset()
    {
        if (_spawnedCharacter != null)
        {
            if (GameManager.SoloMode || GameManager.LocalDebug)
                Destroy(_spawnedCharacter);
            else
                PhotonNetwork.Destroy(_spawnedCharacter);
            _spawnedCharacter = null;
        }
        _spawned = false;
    }

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
            _spawnedCharacter = Instantiate(prefab, spawnPos, Quaternion.identity);
        }
        else
        {
            _spawnedCharacter = PhotonNetwork.Instantiate(_characterPrefabName, spawnPos, Quaternion.identity);
        }
    }
}
