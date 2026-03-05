using UnityEngine;
using Photon.Pun;

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

    private void OnEnable()
    {
        _spawner.OnSpawned += OnSpawned;
    }

    private void OnDisable()
    {
        _spawner.OnSpawned -= OnSpawned;
    }

    public void StartGame()
    {
        if (PhotonNetwork.IsMasterClient)
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
        if (!PhotonNetwork.IsMasterClient) return;
        if (_current == null) return;

        HandleInput();

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

        photonView.RPC(nameof(RpcLock), RpcTarget.All, xs, ys, typeInt);
    }

    [PunRPC]
    private void RpcLock(int[] xs, int[] ys, int typeInt)
    {
        var positions = new Vector2Int[xs.Length];
        for (int i = 0; i < xs.Length; i++)
            positions[i] = new Vector2Int(xs[i], ys[i]);

        if (PhotonNetwork.IsMasterClient)
        {
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
