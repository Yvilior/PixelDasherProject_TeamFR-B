using UnityEngine;
//Service: Ce script sert a porter des objets, MECANIQUE NUMERO 4
//Objet: Le Player
//Auteur: Yvain
//Utilisation: Inserer le rigibody du player dans le script, et s'assurer que les objets interactifs ont le tag "Interact"
public class PlayerInteraction : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField] private float interactionRadius = 1.5f; 
    [SerializeField] private Vector2 holdOffset = new Vector2(0f, 1.5f); 
    [Header("Physics (Jet)")]
    [SerializeField] private Rigidbody2D playerRigidbody; 
    private Transform carriedObject = null;
    private Collider2D carriedCollider = null;
    private Rigidbody2D carriedRigidbody = null;
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (carriedObject != null)
            {
                DropObject();
            }
            else
            {
                TryPickUpObject();
            }
        }
        if (carriedObject != null)
        {
            UpdateObjectPosition();
        }
    }
    private void TryPickUpObject()
    {
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, interactionRadius);
        foreach (var col in hitColliders)
        {
            if (col.CompareTag("Interact") && col.gameObject != this.gameObject)
            {
                carriedObject = col.transform;
                carriedCollider = col.GetComponent<Collider2D>();
                carriedRigidbody = col.GetComponent<Rigidbody2D>();
                if (carriedCollider != null) carriedCollider.enabled = false;
                if (carriedRigidbody != null)
                {
                    carriedRigidbody.simulated = false;
                }
                break;
            }
        }
    }

    private void UpdateObjectPosition()
    {
        carriedObject.position = (Vector2)transform.position + holdOffset;
    }

    private void DropObject()
    {
        if (carriedCollider != null) carriedCollider.enabled = true;
        if (carriedRigidbody != null)
        {
            carriedRigidbody.simulated = true; 
            if (playerRigidbody != null)
            {
                carriedRigidbody.linearVelocity = playerRigidbody.linearVelocity;
            }
        }
        carriedObject = null;
        carriedCollider = null;
        carriedRigidbody = null;
    }
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRadius);
    }
}