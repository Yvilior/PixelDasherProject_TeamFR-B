using UnityEngine;
//Service: Ce script sert a gerer le systeme de vie, de degat et de respawn.
//Objet: Le Player
//Auteur: Yvain
//Utilisation: Inserez Un empty "SpawnPoint" et referez le ici.
public class HealthManager : MonoBehaviour
{
    public GameObject spawnPoint;
    public void Respawn()
    {
        gameObject.transform.position = spawnPoint.transform.position;
    }

    // a l'avenir, la gestion de la sante du player
}
