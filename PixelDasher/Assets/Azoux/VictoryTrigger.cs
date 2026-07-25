using UnityEngine;
using UnityEngine.SceneManagement;
// code ajoute par aziz , ca permet a afficher un panel de victoire a l'arriver du player au point final de la scene
public class VictoryTrigger : MonoBehaviour
{
    [Header("Détection")]
    [Tooltip("Le Tag attribué à votre personnage (par défaut 'Player')")]
    [SerializeField] private string playerTag = "Player";

    [Header("Récompense & UI")]
    [Tooltip("Glissez ici votre Canvas / Panneau 'Tu as gagné'")]
    [SerializeField] private GameObject victoryPanel;

    [Header("Changement de Scène (Optionnel)")]
    [Tooltip("Nom de la scène suivante si vous souhaitez changer de niveau")]
    [SerializeField] private string nextSceneName;

    private bool isVictoryTriggered = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Empêche le déclenchement multiple
        if (isVictoryTriggered) return;

        // Vérifie si l'objet qui entre dans la zone est bien le joueur
        if (collision.CompareTag(playerTag))
        {
            isVictoryTriggered = true;
            OnVictory();
        }
    }

    private void OnVictory()
    {
        Debug.Log("Victoire ! Le joueur a franchi le point d'arrivée.");

        // Option 1 : Afficher le panneau de victoire en jeu
        if (victoryPanel != null)
        {
            victoryPanel.SetActive(true);
            Time.timeScale = 0f; // Mettre le jeu en pause
        }
        


        // ce code sera utilise plus tard


        //  Option 2 : Charger directement la scène de victoire ou le niveau suivant
       //   else if (!string.IsNullOrEmpty(nextSceneName))
        //  {
        //   SceneManager.LoadScene(nextSceneName);
       //   }
    }
}