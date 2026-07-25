using UnityEngine;

public class BananaCollectible : MonoBehaviour
{
    [SerializeField] private int pointsValue = 1; // Nombre de points rapportés

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // On vérifie si c'est le joueur qui touche la banane
        if (collision.CompareTag("Player"))
        {
            // Ajouter les points au GameManager ou ScoreManager
            if (GameManager.instance != null)
            {
                GameManager.instance.AddBananas(pointsValue);
            }

            // Détruire la banane de la scène
            Destroy(gameObject);
        }
    }
}