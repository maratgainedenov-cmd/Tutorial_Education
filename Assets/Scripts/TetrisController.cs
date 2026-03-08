using UnityEngine;
using Photon.Pun;
using DG.Tweening;

public class TetrisController : MonoBehaviourPun, IPunObservable
{
    [SerializeField] private TetrominoSpawner _spawner;
    [SerializeField] private Board _board;
    [SerializeField] private float _fallInterval = 1f;
    [SerializeField] private Block _blockPrefab;

    private Tetromino _current;
    private float _timer;

    // Mirror blocks shown on Player 2's screen
    private Block[] _mirrorBlocks;

    // Ghost piece (тень куда упадёт фигура)
    private Block[] _ghostBlocks;
    private SpriteRenderer[][] _ghostRenderers;

    private void OnEnable()
    {
        _spawner.OnSpawned += OnSpawned;
    }

    private void OnDisable()
    {
        _spawner.OnSpawned -= OnSpawned;
        DestroyGhostBlocks();
    }

    public void StartGame()
    {
        if (PhotonNetwork.IsMasterClient || GameManager.LocalDebug)
        {
            _spawner.SpawnNext();
        }
        else
        {
            CreateMirrorBlocks();
        }
    }

    private void CreateMirrorBlocks()
    {
        if (_blockPrefab == null)
        {
            Debug.LogError("TetrisController: Block Prefab не назначен!");
            return;
        }
        _mirrorBlocks = new Block[4];
        for (int i = 0; i < 4; i++)
        {
            _mirrorBlocks[i] = Instantiate(_blockPrefab, _board.transform);
            _mirrorBlocks[i].gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        if (!PhotonNetwork.IsMasterClient && !GameManager.LocalDebug) return;
        if (_current == null) return;

        HandleInput();
        UpdateGhost();

        _timer += Time.deltaTime;
        if (_timer >= _fallInterval)
        {
            _timer = 0f;
            if (!TryMove(Vector2Int.down))
                LockCurrent();
        }
    }

    private void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.A))
            TryMove(Vector2Int.left);

        if (Input.GetKeyDown(KeyCode.D))
            TryMove(Vector2Int.right);

        if (Input.GetKeyDown(KeyCode.S))
        {
            if (TryMove(Vector2Int.down))
                _timer = 0f;
        }

        if (Input.GetKeyDown(KeyCode.W))
            TryRotate();
    }

    private bool TryMove(Vector2Int direction)
    {
        _current.Move(direction);

        if (_board.IsValidPositions(_current.GetPositions()))
            return true;

        _current.Move(-direction);
        return false;
    }

    private void TryRotate()
    {
        _current.Rotate();

        if (_board.IsValidPositions(_current.GetPositions()))
            return;

        _current.RotateBack();
    }

    private void LockCurrent()
    {
        var positions = _current.GetPositions();
        int[] xs = new int[positions.Length];
        int[] ys = new int[positions.Length];
        for (int i = 0; i < positions.Length; i++)
        {
            xs[i] = positions[i].x;
            ys[i] = positions[i].y;
        }
        int typeInt = (int)_current.Type;

        if (GameManager.LocalDebug)
            RpcLock(xs, ys, typeInt);
        else
            photonView.RPC(nameof(RpcLock), RpcTarget.All, xs, ys, typeInt);
    }

    [PunRPC]
    private void RpcLock(int[] xs, int[] ys, int typeInt)
    {
        var positions = new Vector2Int[xs.Length];
        for (int i = 0; i < xs.Length; i++)
            positions[i] = new Vector2Int(xs[i], ys[i]);

        if (PhotonNetwork.IsMasterClient || GameManager.LocalDebug)
        {
            SetGhostActive(false);
            _board.Lock(_current.GetPositions(), _current.GetBlocks());
            Destroy(_current.gameObject);
            _current = null;
            _spawner.SpawnNext();
        }
        else
        {
            if (_mirrorBlocks != null)
                foreach (var b in _mirrorBlocks)
                    b.gameObject.SetActive(false);

            _board.LockRemote(positions, (TetrominoType)typeInt);
        }
    }

    private void OnSpawned(Tetromino tetromino)
    {
        _current = tetromino;
        _timer = 0f;
        EnsureGhostBlocks();
    }

    // ── Ghost piece ──────────────────────────────────────────────────────

    private void EnsureGhostBlocks()
    {
        if (_ghostBlocks != null && _ghostBlocks[0] != null) return;
        if (_blockPrefab == null) return;

        DestroyGhostBlocks();

        _ghostBlocks    = new Block[4];
        _ghostRenderers = new SpriteRenderer[4][];

        for (int i = 0; i < 4; i++)
        {
            _ghostBlocks[i] = Instantiate(_blockPrefab, _board.transform);
            _ghostBlocks[i].gameObject.SetActive(false);

            foreach (var col in _ghostBlocks[i].GetComponentsInChildren<Collider2D>())
                col.enabled = false;

            // Кешируем ВСЕ SpriteRenderer в блоке — неважно на каком уровне иерархии
            _ghostRenderers[i] = _ghostBlocks[i].GetComponentsInChildren<SpriteRenderer>();
            foreach (var sr in _ghostRenderers[i]) sr.sortingOrder = 5;
        }

    }

    private void DestroyGhostBlocks()
    {
        if (_ghostBlocks == null) return;
        foreach (var b in _ghostBlocks)
            if (b != null) Destroy(b.gameObject);
        _ghostBlocks    = null;
        _ghostRenderers = null as SpriteRenderer[][];

    }

    private void UpdateGhost()
    {
        if (_ghostBlocks == null) { EnsureGhostBlocks(); return; }

        var positions = _current.GetPositions();

        // Опускаем вниз пока можно
        int drop = 0;
        while (_board.IsValidPositions(Shifted(positions, -drop - 1)))
            drop++;

        // Если тень совпадает с фигурой — прячем
        if (drop == 0) { SetGhostActive(false); return; }

        var ghostPos = Shifted(positions, -drop);
        Color baseColor = Tetromino.GetColor(_current.Type);

        // Тень: тот же оттенок, но темнее и полупрозрачная
        Color ghostColor = new Color(baseColor.r * 0.75f, baseColor.g * 0.75f, baseColor.b * 0.75f, 0.6f);

        for (int i = 0; i < _ghostBlocks.Length; i++)
        {
            _ghostBlocks[i].gameObject.SetActive(true);
            _ghostBlocks[i].transform.localPosition =
                new Vector3(ghostPos[i].x, ghostPos[i].y, 0.1f);
            if (_ghostRenderers[i] != null)
                foreach (var sr in _ghostRenderers[i]) sr.color = ghostColor;
        }
    }

    private static Vector2Int[] Shifted(Vector2Int[] positions, int dy)
    {
        var result = new Vector2Int[positions.Length];
        for (int i = 0; i < positions.Length; i++)
            result[i] = positions[i] + new Vector2Int(0, dy);
        return result;
    }

    private void SetGhostActive(bool active)
    {
        if (_ghostBlocks == null) return;
        foreach (var b in _ghostBlocks)
            if (b != null) b.gameObject.SetActive(active);
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            bool hasPiece = _current != null;
            stream.SendNext(hasPiece);
            if (hasPiece)
            {
                stream.SendNext((int)_current.Type);
                var pos = _current.GetPositions();
                for (int i = 0; i < 4; i++)
                {
                    stream.SendNext(pos[i].x);
                    stream.SendNext(pos[i].y);
                }
            }
        }
        else
        {
            bool hasPiece = (bool)stream.ReceiveNext();
            if (hasPiece)
            {
                int typeInt = (int)stream.ReceiveNext();
                var positions = new Vector2Int[4];
                for (int i = 0; i < 4; i++)
                {
                    int x = (int)stream.ReceiveNext();
                    int y = (int)stream.ReceiveNext();
                    positions[i] = new Vector2Int(x, y);
                }
                UpdateMirrorPiece(typeInt, positions);
            }
        }
    }

    private void UpdateMirrorPiece(int typeInt, Vector2Int[] positions)
    {
        if (_mirrorBlocks == null) return;
        Color color = Tetromino.GetColor((TetrominoType)typeInt);
        for (int i = 0; i < _mirrorBlocks.Length && i < positions.Length; i++)
        {
            _mirrorBlocks[i].gameObject.SetActive(true);
            _mirrorBlocks[i].transform.localPosition = new Vector3(positions[i].x, positions[i].y, 0f);
            _mirrorBlocks[i].SetColor(color);
        }
    }
}
