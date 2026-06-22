using UnityEngine;

public class WallJump : MonoBehaviour
{
    [Header("Wall Jump Settings")]
    public float wallJumpForce = 12f;        // Force du saut mural
    public float wallSlideSpeed = 2f;       // Vitesse de glissement sur le mur
    public Transform wallCheck;             // Point de detection du mur
    public float wallCheckDistance = 0.5f;   // Distance de detection
    public LayerMask wallLayer;             // Layer des murs

    private Rigidbody2D rb;
    private bool isTouchingWall;            // Touche un mur ?
    private bool isWallSliding;             // Glisse sur le mur ?
    private bool canWallJump = true;        // Peut sauter du mur ?
    
    // References externes
    private PlayerMouvement playerMovement;  // le script de mouvement

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        playerMovement = GetComponent<PlayerMouvement>();
    }

    void Update()
    {
        CheckWall();
        HandleWallSlide();

        // Wall Jump avec la touche W
        if (Input.GetKeyDown(KeyCode.W) && isTouchingWall && canWallJump)
        {
            PerformWallJump();
        }
    }

    void CheckWall()
    {
        // Detecte si le joueur touche un mur
        isTouchingWall = Physics2D.OverlapCircle(wallCheck.position, wallCheckDistance, wallLayer);
    }

    void HandleWallSlide()
    {
        // Glissement sur le mur (si touche un mur et descend)
        if (isTouchingWall && !isGrounded() && rb.linearVelocity.y < 0)
        {
            isWallSliding = true;
            // Ralentit la chute
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, -wallSlideSpeed);
        }
        else
        {
            isWallSliding = false;
        }
    }

    bool isGrounded()
    {
        // Verifie si au sol (utilise ta methode existante ou adapte)
        return GetComponent<PlayerMouvement>().isGrounded;
    }

    void PerformWallJump()
    {
        // Direction opposee au mur
        float jumpDirection = playerMovement.isFacingRight ? -1f : 1f;

        // Applique la force de saut
        rb.linearVelocity = new Vector2(jumpDirection * wallJumpForce * 0.5f, wallJumpForce);

        // Bloque temporairement le mouvement pour eviter de revenir sur le mur
        StartCoroutine(WaitBeforeControl());
    }

    System.Collections.IEnumerator WaitBeforeControl()
    {
        // Desactive le controle pendant 0.2 secondes
        if (playerMovement != null)
            playerMovement.enabled = false;

        yield return new WaitForSeconds(0.2f);

        if (playerMovement != null)
            playerMovement.enabled = true;
    }
}