using Unity.VisualScripting;
using UnityEngine;
using System.Collections;



public class PlayerMouvement_Omar : MonoBehaviour
{
    [Header("Réglages de mouvement")]
    [SerializeField] private float speed = 5f;
    [SerializeField] private float jumpForce = 7f;
    [SerializeField] private float doubleJumpForce = 6f; // Légèrement moins fort que le premier saut
    [SerializeField] private PhysicsMaterial2D slipperyMaterial;

    [Header("Gravité améliorée")]
    [SerializeField] private float fallMultiplier = 2.5f;   // Chute plus rapide que la montée
    [SerializeField] private float lowJumpMultiplier = 2f;  // Relâcher W = saut plus court
    [SerializeField] private float maxFallSpeed = 20f;      // Vitesse maximale de chute

    [Header("Détection Sol / Murs")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float rayDistance = 0.1f;
    [SerializeField] private float wallRayDistance = 0.1f;

    [Header("Réglages Dash")]
    [SerializeField] private float dashForce = 15f;
    [SerializeField] private float dashDuration = 0.2f;
    [SerializeField] private float dashCooldown = 2f;

    [Header("Réglages Wall Jump")]
    [SerializeField] private Vector2 wallJumpForce = new Vector2(5f, 5f);
    [SerializeField] private float wallJumpDuration = 0.15f;

    [Header("Pouvoirs Déblocables")]
    public bool canDoubleJump; // Cocher dans l'Inspector au Chapitre 1
    public bool canWallJump;   // Cocher dans l'Inspector au Chapitre 3
    public bool canDash;       // Cocher dans l'Inspector au Chapitre 2

    private Rigidbody2D rb;
    private CapsuleCollider2D playerCollider;
    private float horizontalInput;
    private bool isGrounded;
    private bool isTouchingWall;
    private float wallDirection;
    private bool hasDoubleJump;
    private bool isFacingRight = true;
    private bool isDashing;
    private bool canDashAgain = true;
    private bool hasAirDash = true;
    private bool isWallJumping;
    private GameObject lastWall;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        playerCollider = GetComponent<CapsuleCollider2D>();
        if (slipperyMaterial != null)
            playerCollider.sharedMaterial = slipperyMaterial;
    }

    private void Update()
    {
        if (isDashing) return;

        CheckSurroundings();

        // Recharge les capacités quand le joueur touche le sol
        if (isGrounded)
        {
            hasDoubleJump = true;
            hasAirDash = true;
            lastWall = null;
        }
        else if (isTouchingWall && canWallJump)
        {
            hasDoubleJump = true;
            hasAirDash = true;
        }

        horizontalInput = Input.GetAxisRaw("Horizontal");

        if (!isWallJumping)
        {
            if (horizontalInput > 0 && !isFacingRight) Flip();
            else if (horizontalInput < 0 && isFacingRight) Flip();
        }

        // ── Saut ────────────────────────────────────────────────────────
        if (Input.GetKeyDown(KeyCode.W))
        {
            if (isGrounded)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            }
            else if (isTouchingWall && canWallJump)
            {
                StartCoroutine(WallJumpRoutine());
            }
            else if (hasDoubleJump && canDoubleJump && !isWallJumping)
            {
                // Double jump : consomme le saut et applique une force légèrement moins forte
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, doubleJumpForce);
                hasDoubleJump = false;
            }
        }

        // ── Dash ────────────────────────────────────────────────────────
        if (Input.GetKeyDown(KeyCode.Space) && canDash && canDashAgain && !isWallJumping)
        {
            if (isGrounded || hasAirDash)
            {
                if (!isGrounded) hasAirDash = false;
                StartCoroutine(DashRoutine());
            }
        }
    }

    private void FixedUpdate()
    {
        if (isDashing || isWallJumping) return;
        ApplyMovement();
        ApplyBetterGravity();
    }

    private void ApplyMovement()
    {
        rb.linearVelocity = new Vector2(horizontalInput * speed, rb.linearVelocity.y);
    }

    private void ApplyBetterGravity()
    {
        if (rb.linearVelocity.y < 0f)
        {
            // Chute plus rapide que la montée
            rb.linearVelocity += Vector2.up * Physics2D.gravity.y
                * (fallMultiplier - 1f) * Time.fixedDeltaTime;

            // Limite la vitesse max de chute
            if (rb.linearVelocity.y < -maxFallSpeed)
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, -maxFallSpeed);
        }
        else if (rb.linearVelocity.y > 0f && !Input.GetKey(KeyCode.W))
        {
            // Relâcher W pendant la montée = saut plus court
            rb.linearVelocity += Vector2.up * Physics2D.gravity.y
                * (lowJumpMultiplier - 1f) * Time.fixedDeltaTime;
        }
    }

    private IEnumerator DashRoutine()
    {
        canDashAgain = false;
        isDashing = true;
        float originalGravity = rb.gravityScale;
        rb.gravityScale = 0f;
        float dashDirection = isFacingRight ? 1f : -1f;
        rb.linearVelocity = new Vector2(dashDirection * dashForce, 0f);
        yield return new WaitForSeconds(dashDuration);
        rb.gravityScale = originalGravity;
        isDashing = false;
        yield return new WaitForSeconds(dashCooldown);
        canDashAgain = true;
    }

    private IEnumerator WallJumpRoutine()
    {
        isWallJumping = true;
        hasDoubleJump = true;
        hasAirDash = true;
        float jumpDir = -wallDirection;
        if ((jumpDir > 0 && !isFacingRight) || (jumpDir < 0 && isFacingRight))
            Flip();
        rb.linearVelocity = new Vector2(jumpDir * wallJumpForce.x, wallJumpForce.y);
        yield return new WaitForSeconds(wallJumpDuration);
        isWallJumping = false;
    }

    private void CheckSurroundings()
    {
        Vector2 center = playerCollider.bounds.center;
        Vector2 extents = playerCollider.bounds.extents;

        // Détecte le sol par raycast vers le bas
        isGrounded = Physics2D.Raycast(
            center, Vector2.down, extents.y + rayDistance, groundLayer);

        RaycastHit2D hitLeft = Physics2D.Raycast(
            center, Vector2.left, extents.x + wallRayDistance, groundLayer);
        RaycastHit2D hitRight = Physics2D.Raycast(
            center, Vector2.right, extents.x + wallRayDistance, groundLayer);

        RaycastHit2D validHit = default;
        if (hitRight && hitRight.collider.CompareTag("Wall")) validHit = hitRight;
        else if (hitLeft && hitLeft.collider.CompareTag("Wall")) validHit = hitLeft;

        if (validHit.collider != null && validHit.collider.gameObject != lastWall)
        {
            isTouchingWall = true;
            wallDirection = (validHit.normal.x > 0) ? -1f : 1f;
            if (isWallJumping) lastWall = validHit.collider.gameObject;
        }
        else
        {
            isTouchingWall = false;
            wallDirection = 0f;
        }

        if (isGrounded) isTouchingWall = false;
    }

    private void Flip()
    {
        isFacingRight = !isFacingRight;
        Vector3 localScale = transform.localScale;
        localScale.x *= -1f;
        transform.localScale = localScale;
    }
}