using UnityEngine;
using Photon.Pun;

public class BombNPCSpawner : MonoBehaviour
{
    [SerializeField] private BombNPC  _npcPrefab;
    [SerializeField] private Transform[] _spawnPoints;   // точки спауна (слева/справа)
    [SerializeField] private float    _spawnInterval = 15f;
    [SerializeField] private int      _maxAlive      = 3;

    private float _timer;
    private int   _aliveCount;

    private void Update()
    {
        _timer += Time.deltaTime;
        if (_timer >= _spawnInterval)
        {
            _timer = 0f;
            TrySpawn();
        }
    }

    private void TrySpawn()
    {
        if (!PhotonNetwork.IsMasterClient && !GameManager.LocalDebug) return;
        if (_aliveCount >= _maxAlive) return;
        if (_npcPrefab == null || _spawnPoints == null || _spawnPoints.Length == 0) return;

        Transform spawnPoint = _spawnPoints[Random.Range(0, _spawnPoints.Length)];
        BombNPC npc;
        if (GameManager.LocalDebug)
            npc = Instantiate(_npcPrefab, spawnPoint.position, Quaternion.identity);
        else
            npc = PhotonNetwork.Instantiate("BombNpc", spawnPoint.position, Quaternion.identity)
                               .GetComponent<BombNPC>();

        _aliveCount++;
        npc.OnDied += () => _aliveCount--;
    }

    // Вызвать из GameManager или кнопки для немедленного спауна (тест)
    public void SpawnNow() => TrySpawn();
}
