using UnityEngine;
using Photon.Pun;
using DG.Tweening;

public class TetrisController : MonoBehaviourPun, IPunObservable
{
    [SerializeField] private TetrominoSpawner _spawner;
    [SerializeField] private Board _board;
    [SerializeField] private float _fallSpeed = 5f; // клеток в секунду
    [SerializeField] private Block _blockPrefab;
    [SerializeField] private BombNPC _bombNpcPrefab;

    private Tetromino _current;
    private float _fallAccum;
    private float _fallSpeedOverride = -1f; // -1 = не активно

    // Mirror blocks shown on Player 2's screen
    private Block[] _mirrorBlocks;

    // Ghost piece
    private Block[] _ghostBlocks;
    private SpriteRenderer[][] _ghostRenderers;

    private void Awake()
    {
        if (_board   == null) Debug.LogError("[TetrisController] Board не назначен в Inspector!", this);
        if (_spawner == null) Debug.LogError("[TetrisController] TetrominoSpawner не назначен в Inspector!", this);

        if (photonView != null && !photonView.ObservedComponents.Contains(this))
            photonView.ObservedComponents.Add(this);
    }

    private void OnEnable()  { _spawner.OnSpawned += OnSpawned; _spawner.OnNextChanged += SyncNextType; }
    private void OnDisable() { _spawner.OnSpawned -= OnSpawned; _spawner.OnNextChanged -= SyncNextType; DestroyGhostBlocks(); }

    public void ResetBoard()
    {
        _board?.ClearBoard();
        DestroyGhostBlocks();
        if (_current != null)
        {
            Destroy(_current.gameObject);
            _current = null;
        }
        _fallAccum = 0f;
    }

    public void StartGame()
    {
        if (!GameManager.LocalDebug && !GameManager.SoloMode)
            photonView.RPC(nameof(RpcSyncNext), RpcTarget.Others, (int)_spawner.NextType);
    }

    public void InitForViewing()
    {
        if (!GameManager.LocalDebug)
            CreateMirrorBlocks();
    }

    private void SyncNextType(TetrominoType type)
    {
        if (GameManager.LocalDebug || !GameManager.IsTetrisPlayer()) return;
        photonView.RPC(nameof(RpcSyncNext), RpcTarget.Others, (int)type);
    }

    [PunRPC]
    private void RpcSyncNext(int typeInt) => _spawner.SetNextType((TetrominoType)typeInt);

    // Вызывается из превью для BombNPC — спаунит живого NPC в колонке
    public void SpawnBombNpcAt(int gridX)
    {
        if (!GameManager.IsTetrisPlayer()) return;

        int spawnY = _spawner != null ? _spawner.SpawnPosition.y : _board.Height - 2;
        Vector3 worldPos = _board.transform.TransformPoint(new Vector3(gridX, spawnY, 0f));

        if (GameManager.LocalDebug)
            Instantiate(_bombNpcPrefab, worldPos, Quaternion.identity);
        else
            PhotonNetwork.Instantiate("BombNpc", worldPos, Quaternion.identity);

        _spawner.ConsumeNext();
    }

    /// <summary>Вызывается из TetrisAI в Solo режиме.</summary>
    public void SpawnAtColumnAI(int gridX, float fallSpeedMultiplier = 1f)
    {
        if (!GameManager.SoloMode) return;
        if (_current != null) return;
        _spawner.SpawnNext();
        if (_current == null) return;

        var pivot = _current.GetPivot();
        _current.SetPivot(new Vector2Int(gridX, pivot.y));

        // Сдвигаем пивот так, чтобы все блоки оказались внутри сетки
        var positions = _current.GetPositions();
        int minX = int.MaxValue, maxX = int.MinValue;
        foreach (var p in positions) { minX = Mathf.Min(minX, p.x); maxX = Mathf.Max(maxX, p.x); }

        int adjustedX = gridX;
        if (minX < 0)              adjustedX -= minX;                    // выходит влево — двигаем вправо
        else if (maxX >= _board.Width) adjustedX -= (maxX - _board.Width + 1); // выходит вправо — двигаем влево

        _current.SetPivot(new Vector2Int(adjustedX, pivot.y));
        _fallAccum = 0f;
        _fallSpeedOverride = fallSpeedMultiplier != 1f ? _fallSpeed * fallSpeedMultiplier : -1f;
    }

    // Вызывается из NextPiecePreview когда бросили на сетку
    public void SpawnAtColumn(int gridX, Vector2Int[] rotatedOffsets = null)
    {
        if (!GameManager.IsTetrisPlayer()) return;
        if (_current != null) return;

        _spawner.SpawnNext();

        if (_current == null) return;

        // Применяем поворот из превью
        if (rotatedOffsets != null)
            _current.SetOffsets(rotatedOffsets);

        var pivot = _current.GetPivot();
        _current.SetPivot(new Vector2Int(gridX, pivot.y));
        _fallAccum = 0f;
    }

    private void CreateMirrorBlocks()
    {
        if (_blockPrefab == null) { Debug.LogError("TetrisController: Block Prefab не назначен!"); return; }
        _mirrorBlocks = new Block[4];
        for (int i = 0; i < 4; i++)
        {
            _mirrorBlocks[i] = Instantiate(_blockPrefab, _board.transform);
            _mirrorBlocks[i].gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        if (!GameManager.IsTetrisPlayer() && !GameManager.SoloMode) return;
        if (_current == null) return;

        UpdateGhost();

        float activeFallSpeed = _fallSpeedOverride > 0f ? _fallSpeedOverride : _fallSpeed;
        _fallAccum += activeFallSpeed * Time.deltaTime;
        _fallAccum = Mathf.Min(_fallAccum, 2f); // не более 2 клеток за кадр
        while (_fallAccum >= 1f)
        {
            _fallAccum -= 1f;
            if (!TryMove(Vector2Int.down))
            {
                LockCurrent();
                return;
            }
            BroadcastPiecePositions();
        }

        // Плавное визуальное падение — показываем дробную позицию между клетками
        _current.UpdateVisualPositions(-_fallAccum);
    }

    private bool TryMove(Vector2Int direction)
    {
        _current.Move(direction);
        if (_board.IsValidPositions(_current.GetPositions())) return true;
        _current.Move(-direction);
        return false;
    }

    private void LockCurrent()
    {
        _fallSpeedOverride = -1f;
        var positions = _current.GetPositions();
        int[] xs = new int[positions.Length];
        int[] ys = new int[positions.Length];
        for (int i = 0; i < positions.Length; i++) { xs[i] = positions[i].x; ys[i] = positions[i].y; }
        int typeInt = (int)_current.Type;

        if (GameManager.LocalDebug)
        {
            RpcLock(xs, ys, typeInt);
        }
        else
        {
            // Отправляем только остальным, мастер выполняет напрямую
            photonView.RPC(nameof(RpcLock), RpcTarget.Others, xs, ys, typeInt);
            SetGhostActive(false);
            _board.Lock(_current.GetPositions(), _current.GetBlocks());
            Destroy(_current.gameObject);
            _current = null;
        }
    }

    [PunRPC]
    private void RpcLock(int[] xs, int[] ys, int typeInt)
    {
        var positions = new Vector2Int[xs.Length];
        for (int i = 0; i < xs.Length; i++)
            positions[i] = new Vector2Int(xs[i], ys[i]);

        if (GameManager.LocalDebug)
        {
            SetGhostActive(false);
            _board.Lock(_current.GetPositions(), _current.GetBlocks());
            Destroy(_current.gameObject);
            _current = null;
        }
        else
        {
            if (_mirrorBlocks != null)
                foreach (var b in _mirrorBlocks) b.gameObject.SetActive(false);
            _board.LockRemote(positions, (TetrominoType)typeInt);
        }
    }

    private void OnSpawned(Tetromino tetromino)
    {
        _current = tetromino;
        _fallAccum = 0f;
        EnsureGhostBlocks();
        BroadcastPiecePositions();
    }

    private void BroadcastPiecePositions()
    {
        if (GameManager.LocalDebug || GameManager.SoloMode || _current == null) return;
        var pos = _current.GetPositions();
        var xs = new int[4]; var ys = new int[4];
        for (int i = 0; i < 4; i++) { xs[i] = pos[i].x; ys[i] = pos[i].y; }
        photonView.RPC(nameof(RpcUpdatePiece), RpcTarget.Others, (int)_current.Type, xs, ys);
    }

    [PunRPC]
    private void RpcUpdatePiece(int typeInt, int[] xs, int[] ys)
    {
        if (_mirrorBlocks == null) return;
        Color color = Tetromino.GetColor((TetrominoType)typeInt);
        for (int i = 0; i < _mirrorBlocks.Length && i < xs.Length; i++)
        {
            _mirrorBlocks[i].gameObject.SetActive(true);
            _mirrorBlocks[i].transform.localPosition = new Vector3(xs[i], ys[i], 0f);
            _mirrorBlocks[i].SetColor(color);
        }
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
            foreach (var col in _ghostBlocks[i].GetComponentsInChildren<Collider2D>()) col.enabled = false;
            _ghostRenderers[i] = _ghostBlocks[i].GetComponentsInChildren<SpriteRenderer>();
            foreach (var sr in _ghostRenderers[i]) sr.sortingOrder = 5;
        }
    }

    private void DestroyGhostBlocks()
    {
        if (_ghostBlocks == null) return;
        foreach (var b in _ghostBlocks) if (b != null) Destroy(b.gameObject);
        _ghostBlocks    = null;
        _ghostRenderers = null as SpriteRenderer[][];
    }

    private void UpdateGhost()
    {
        if (_ghostBlocks == null) { EnsureGhostBlocks(); return; }

        var positions = _current.GetPositions();
        int drop = 0;
        while (_board.IsValidPositions(Shifted(positions, -drop - 1))) drop++;

        if (drop == 0) { SetGhostActive(false); return; }

        var ghostPos   = Shifted(positions, -drop);
        Color baseColor  = Tetromino.GetColor(_current.Type);
        Color ghostColor = new Color(baseColor.r * 0.75f, baseColor.g * 0.75f, baseColor.b * 0.75f, 0.6f);

        for (int i = 0; i < _ghostBlocks.Length; i++)
        {
            _ghostBlocks[i].gameObject.SetActive(true);
            _ghostBlocks[i].transform.localPosition = new Vector3(ghostPos[i].x, ghostPos[i].y, 0.1f);
            if (_ghostRenderers[i] != null)
                foreach (var sr in _ghostRenderers[i]) sr.color = ghostColor;
        }
    }

    private readonly Vector2Int[] _shiftBuffer = new Vector2Int[4];

    private Vector2Int[] Shifted(Vector2Int[] positions, int dy)
    {
        for (int i = 0; i < positions.Length && i < _shiftBuffer.Length; i++)
            _shiftBuffer[i] = positions[i] + new Vector2Int(0, dy);
        return _shiftBuffer;
    }

    private void SetGhostActive(bool active)
    {
        if (_ghostBlocks == null) return;
        foreach (var b in _ghostBlocks) if (b != null) b.gameObject.SetActive(active);
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
                for (int i = 0; i < 4; i++) { stream.SendNext(pos[i].x); stream.SendNext(pos[i].y); }
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
