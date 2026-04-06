using UnityEngine;
using DG.Tweening;

/// <summary>
/// Выход для персонажа-платформера.
/// Находится в сетке — тетрис-игрок может заблокировать его тетромино.
/// Когда клетка свободна и персонаж касается триггера → победа.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class Exit : MonoBehaviour
{
    [Header("Grid")]
    [SerializeField] private Board      _board;
    [SerializeField] private Vector2Int _gridPos = new Vector2Int(0, 2); // клетка выхода

    [Header("Visual")]
    [SerializeField] private SpriteRenderer _openVisual;    // спрайт когда открыт
    [SerializeField] private SpriteRenderer _blockedVisual; // спрайт когда заблокирован
    [SerializeField] private Transform _cellReference;      // любой блок из сцены для референса размера

    private static readonly Color ColOpen    = new Color(0.2f, 1f,  0.3f, 0.85f);
    private static readonly Color ColBlocked = new Color(1f,  0.2f, 0.2f, 0.5f);

    private Tween _pulseTween;
    private float _activeTime;
    private const float ActivationDelay = 1f; // секунда после старта игры

    public void SetGridPos(Vector2Int pos)
    {
        _gridPos = pos;
        if (_board != null)
            transform.position = _board.transform.TransformPoint(new Vector3(pos.x, pos.y, 0f));
    }

    private void Awake()
    {
        if (_board == null) _board = FindObjectOfType<Board>();
        GetComponent<Collider2D>().isTrigger = true;
    }

    private void Start()
    {
        // Ставим выход точно на клетку сетки (как блоки на доске — целые координаты)
        if (_board != null)
        {
            transform.position = _board.transform.TransformPoint(
                new Vector3(_gridPos.x, _gridPos.y, 0f));
        }

        // Масштабируем визуалы до ровно 1 клетки
        FitToOneCell(_openVisual);
        FitToOneCell(_blockedVisual);

        // Запускаем пульсацию
        if (_openVisual != null)
        {
            _openVisual.color = ColOpen;
            _pulseTween = _openVisual.DOFade(0.4f, 0.6f)
                                     .SetLoops(-1, LoopType.Yoyo)
                                     .SetEase(Ease.InOutSine);
        }
    }

    private void FitToOneCell(SpriteRenderer sr)
    {
        if (sr == null) return;
        if (_cellReference != null)
        {
            // Берём размер реальной клетки с доски
            sr.transform.localScale = _cellReference.lossyScale;
        }
        else if (sr.sprite != null)
        {
            Vector2 spriteSize = sr.sprite.rect.size / sr.sprite.pixelsPerUnit;
            if (spriteSize.x > 0f && spriteSize.y > 0f)
                sr.transform.localScale = new Vector3(1f / spriteSize.x, 1f / spriteSize.y, 1f);
        }
    }

    private void OnEnable()
    {
        _activeTime = 0f;
    }

    private void Update()
    {
        _activeTime += Time.deltaTime;
        if (_board == null) return;

        bool blocked = _board.IsOccupied(_gridPos);

        if (_openVisual    != null) _openVisual.gameObject.SetActive(!blocked);
        if (_blockedVisual != null) _blockedVisual.gameObject.SetActive(blocked);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_activeTime < ActivationDelay) return;
        if (!other.CompareTag("Player")) return;

        // Выход заблокирован тетромино — не работает
        if (_board != null && _board.IsOccupied(_gridPos)) return;

        var character = other.GetComponent<Character>()
                     ?? other.GetComponentInParent<Character>();
        if (character == null) return;

        GameManager.Instance?.Win();
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        // Показываем клетку выхода в редакторе
        var b = _board != null ? _board : FindObjectOfType<Board>();
        if (b == null) return;

        Vector3 worldPos = b.transform.TransformPoint(
            new Vector3(_gridPos.x, _gridPos.y, 0f));

        Gizmos.color = new Color(0.2f, 1f, 0.2f, 0.25f);
        Gizmos.DrawCube(worldPos + new Vector3(0.5f, 0.5f, 0f), Vector3.one);

        Gizmos.color = new Color(0.2f, 1f, 0.2f, 0.9f);
        Gizmos.DrawWireCube(worldPos + new Vector3(0.5f, 0.5f, 0f), Vector3.one);

        UnityEditor.Handles.color = Color.green;
        UnityEditor.Handles.Label(worldPos + new Vector3(0.5f, 1.2f, 0f), "EXIT");
    }
#endif
}
