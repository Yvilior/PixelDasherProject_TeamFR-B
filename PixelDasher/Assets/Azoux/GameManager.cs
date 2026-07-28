using UnityEngine;
using TMPro; // Pour gérer le texte TextMeshPro

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI bananaText; // Glisse ton texte UI ici

    private int bananaCount = 0;

    [Header("Respawn")]
    public Vector3 currentRespawnPosition;
    private void Awake()
    {
        // Singleton pour accéder facilement au GameManager depuis les bananes
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        UpdateUI();
    }

    public void AddBananas(int amount)
    {
        bananaCount += amount;
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (bananaText != null)
        {
            bananaText.text = "x" + bananaCount;
        }
    }
    public void UpdateRespawnPoint(Vector3 newPosition)
    {
        currentRespawnPosition = newPosition;
        Debug.Log("Nouveau point de réapparition enregistré : " + newPosition);
    }
}