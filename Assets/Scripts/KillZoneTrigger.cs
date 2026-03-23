using UnityEngine;

public class KillZoneTrigger : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other) => TryKill(other);

    private void TryKill(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        var character = other.GetComponentInParent<Character>();
        if (character == null) return;

        character.Die();
    }
}
