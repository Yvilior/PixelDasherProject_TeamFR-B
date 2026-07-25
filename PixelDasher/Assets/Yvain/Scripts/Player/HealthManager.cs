using System.Collections;
using UnityEngine;
using UnityEngine.UI;

// Service: Ce script sert à gérer le système de vie, de dégâts, de respawn et l'affichage des cœurs UI.
// Objet: Le Player
// Auteur: modification AZIZ pour la scene 
// Utilisation: ca sert a eliminer un coeur quand le player est endommage par une banane rouge 

public class HealthManager : MonoBehaviour
{
    [Header("Point de Respawn")]
    public GameObject spawnPoint;

    [Header("Réglages Santé")]
    [SerializeField] private int maxHealth = 3;
    private int currentHealth;

    [Header("Interface UI (Cœurs)")]
    [Tooltip("Glisse ici les 3 Images de cœurs se trouvant sous ton Canvas")]
    [SerializeField] private Image[] heartImages;

    [Header("Période d'Invulnérabilité (Clignotement)")]
    [SerializeField] private float invincibilityDuration = 1.2f;
    [SerializeField] private float flashInterval = 0.15f;

    private SpriteRenderer spriteRenderer;
    private bool isInvulnerable = false;

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        currentHealth = maxHealth;
        UpdateHeartsUI();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Détection si la banane rouge / le pic a le tag Spike
        if (other.CompareTag("Spike") && !isInvulnerable)
        {
            TakeDamage(1);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Détection si la banane rouge / le pic a le tag Spike
        if (collision.gameObject.CompareTag("Spike") && !isInvulnerable)
        {
            TakeDamage(1);
        }
    }
    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        // Retire le cœur sur l'écran
        UpdateHeartsUI();

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            StartCoroutine(InvulnerabilityRoutine());
        }
    }

    private void UpdateHeartsUI()
    {
        if (heartImages == null || heartImages.Length == 0) return;

        // Active ou masque les cœurs du Canvas selon la santé actuelle
        for (int i = 0; i < heartImages.Length; i++)
        {
            if (i < currentHealth)
            {
                heartImages[i].enabled = true; // Cœur visible
            }
            else
            {
                heartImages[i].enabled = false; // Cœur enlevé
            }
        }
    }

    private IEnumerator InvulnerabilityRoutine()
    {
        isInvulnerable = true;

        if (spriteRenderer != null)
        {
            float timer = 0f;
            while (timer < invincibilityDuration)
            {
                // Clignotement de transparence du joueur
                Color color = spriteRenderer.color;
                color.a = (color.a == 1f) ? 0.3f : 1f;
                spriteRenderer.color = color;

                yield return new WaitForSeconds(flashInterval);
                timer += flashInterval;
            }

            // Remet l'opacité normale
            Color finalColor = spriteRenderer.color;
            finalColor.a = 1f;
            spriteRenderer.color = finalColor;
        }
        else
        {
            yield return new WaitForSeconds(invincibilityDuration);
        }

        isInvulnerable = false;
    }

    private void Die()
    {
        Debug.Log("Le joueur n'a plus de cœurs ! Réapparition au SpawnPoint...");

        // Réinitialise les cœurs et la santé
        currentHealth = maxHealth;
        UpdateHeartsUI();

        // Replacer au point de respawn
        Respawn();
    }

    public void Respawn()
    {
        if (spawnPoint != null)
        {
            gameObject.transform.position = spawnPoint.transform.position;
        }
        else
        {
            Debug.LogWarning("Attention : Aucun SpawnPoint n'est assigné dans HealthManager !");
        }
    }
}