using UnityEngine;

public class KillZoneTrigger : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        if (GetComponentInParent<Tetromino>() == null) return;

        other.GetComponent<Character>()?.Die();
    }
}
