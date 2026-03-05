using UnityEngine;

public class Block : MonoBehaviour
{
    [SerializeField] private SpriteRenderer _renderer;
    [SerializeField] private KillZoneTrigger _killZone;

    public void SetColor(Color color)
    {
        _renderer.color = color;
    }

    private void OnTransformParentChanged()
    {
        if (_killZone == null) return;
        _killZone.enabled = GetComponentInParent<Tetromino>() != null;
    }
}
