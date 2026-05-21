using UnityEngine;
//Service: Ce script sert a gerer le comportement des piques, qui font respawn le player a son point de spawn.
//Objet: Les piques
//Auteur: Yvain
// Utilisation: Aucune manipulation necessaire
public class SpikeBehaviour : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            other.GetComponent<HealthManager>().Respawn();
        }
    }
    // a modifie si syteme de vie implemente

}
