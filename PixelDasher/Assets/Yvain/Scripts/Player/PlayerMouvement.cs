using Unity.VisualScripting;
using UnityEngine;
using System.Collections;
//Service: Ce script sert a faire bouger le joueur.
//Objet: Le Player
//Auteur: Yvain
//Utilisation: A ajouter au Player, et a completer pour faire bouger le Player.
public class PlayerMouvement : MonoBehaviour
{
    [Header("Réglages de mouvement")]
    [SerializeField] private float speed = 5f;
    [SerializeField] private float jumpForce = 5f;
    [SerializeField] private PhysicsMaterial2D slipperyMaterial;
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
    public bool canDoubleJump;
    public bool canWallJump;
    public bool canDash;
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
    private GameObject lastWall; // Stocke le dernier mur sauté pour éviter le spam
    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        playerCollider = GetComponent<CapsuleCollider2D>();
        if (slipperyMaterial != null)
        {
            playerCollider.sharedMaterial = slipperyMaterial;
        }
    }
    private void Update()
    {
        if (isDashing) return;
        CheckSurroundings();
        if (isGrounded)
        {
            hasDoubleJump = true;
            hasAirDash = true;
            lastWall = null; // Reset du mur au contact du sol
        }
        else if (isTouchingWall && canWallJump)
        {
            hasDoubleJump = true;
            hasAirDash = true;
        }
        horizontalInput = Input.GetAxisRaw("Horizontal");
        if (!isWallJumping)
        {
            if (horizontalInput > 0 && !isFacingRight)
            {
                Flip();
            }
            else if (horizontalInput < 0 && isFacingRight)
            {
                Flip();
            }
        }
        if (Input.GetKeyDown(KeyCode.W))
        {
            if (isGrounded)
            {
                Jump();
            }
            else if (isTouchingWall && canWallJump)
            {
                StartCoroutine(WallJumpRoutine());
            }
            else if (hasDoubleJump && canDoubleJump && !isWallJumping)
            {
                Jump();
                hasDoubleJump = false;
            }
        }
        if (Input.GetKeyDown(KeyCode.Space) && canDash && canDashAgain && !isWallJumping)
        {
            if (isGrounded || hasAirDash)
            {
                if (!isGrounded)
                {
                    hasAirDash = false;
                }
                StartCoroutine(DashRoutine());
            }
        }
    }
    private void FixedUpdate()
    {
        if (isDashing || isWallJumping) return;
        ApplyMovement();
    }
    private void ApplyMovement()
    {
        rb.linearVelocity = new Vector2(horizontalInput * speed, rb.linearVelocity.y);
    }
    private void Jump()
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
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
        {
            Flip();
        }
        rb.linearVelocity = new Vector2(jumpDir * wallJumpForce.x, wallJumpForce.y);
        yield return new WaitForSeconds(wallJumpDuration);
        isWallJumping = false;
    }
    private void CheckSurroundings()
    {
        Vector2 center = playerCollider.bounds.center;
        Vector2 extents = playerCollider.bounds.extents;
        isGrounded = Physics2D.Raycast(center, Vector2.down, extents.y + rayDistance, groundLayer);
        RaycastHit2D hitLeft = Physics2D.Raycast(center, Vector2.left, extents.x + wallRayDistance, groundLayer);
        RaycastHit2D hitRight = Physics2D.Raycast(center, Vector2.right, extents.x + wallRayDistance, groundLayer);
        RaycastHit2D validHit = default;
        if (hitRight && hitRight.collider.CompareTag("Wall")) validHit = hitRight;
        else if (hitLeft && hitLeft.collider.CompareTag("Wall")) validHit = hitLeft;
        // Permet le saut uniquement si c'est un nouveau mur
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