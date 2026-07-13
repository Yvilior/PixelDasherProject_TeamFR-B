using UnityEngine;

public class DropZone : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            other.GetComponent<HealthManager>().Respawn();
        }
    }
}