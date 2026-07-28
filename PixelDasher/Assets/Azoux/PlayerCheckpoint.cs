using UnityEngine;

public class PlayerCheckpoint : MonoBehaviour
{
    private bool isActivated = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Vérifie si c'est le joueur qui touche le checkpoint
        if (other.CompareTag("Player") && !isActivated)
        {
            isActivated = true;

            // Met à jour la position de réapparition dans le GameManager
            if (GameManager.instance != null)
            {
                GameManager.instance.UpdateRespawnPoint(transform.position);
                Debug.Log("Nouveau Checkpoint activé à : " + transform.position);
            }
        }
    }
}