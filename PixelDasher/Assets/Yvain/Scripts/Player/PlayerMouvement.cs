using Unity.VisualScripting;
using UnityEngine;
using System.Collections;
//Service: Ce script sert a faire bouger le joueur.
//Objet: Le Player
//Auteur: Yvain, Hatim
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

    [Header("Feel du saut")]
    [SerializeField] private float coyoteTime = 0.15f;
    [SerializeField] private float jumpBufferTime = 0.2f;
    [SerializeField, Range(0f, 1f)] private float jumpCutMultiplier = 0.4f;
    [SerializeField] private float maxFallSpeed = 40f;

    [Header("Apex modifier")]
    [SerializeField] private float apexThreshold = 2.5f;
    [SerializeField] private float apexBonusSpeed = 2f;
    [SerializeField, Range(0f, 1f)] private float apexGravityMultiplier = 0.4f;

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
    private GameObject lastWall;

    private float coyoteCounter;
    private float jumpBufferCounter;
    private float baseGravityScale;
    private float apexPoint;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        playerCollider = GetComponent<CapsuleCollider2D>();
        baseGravityScale = rb.gravityScale;
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
            lastWall = null;
            coyoteCounter = coyoteTime;
        }
        else
        {
            coyoteCounter -= Time.deltaTime;

            if (isTouchingWall && canWallJump)
            {
                hasDoubleJump = true;
                hasAirDash = true;
            }
        }

        horizontalInput = Input.GetAxisRaw("Horizontal");

        if (!isWallJumping)
        {
            if (horizontalInput > 0 && !isFacingRight) Flip();
            else if (horizontalInput < 0 && isFacingRight) Flip();
        }

        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.Space))
        {
            jumpBufferCounter = jumpBufferTime;
        }
        else
        {
            jumpBufferCounter -= Time.deltaTime;
        }

        if (jumpBufferCounter > 0f)
        {
            if (coyoteCounter > 0f)
            {
                Jump();
                jumpBufferCounter = 0f;
                coyoteCounter = 0f;
            }
            else if (isTouchingWall && canWallJump)
            {
                StartCoroutine(WallJumpRoutine());
                jumpBufferCounter = 0f;
            }
            else if (hasDoubleJump && canDoubleJump && !isWallJumping)
            {
                Jump();
                hasDoubleJump = false;
                jumpBufferCounter = 0f;
            }
        }

        if ((Input.GetKeyUp(KeyCode.W) || Input.GetKeyUp(KeyCode.Space)) && rb.linearVelocity.y > 0f && !isWallJumping)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * jumpCutMultiplier);
        }

        if (Input.GetKeyDown(KeyCode.E) && canDash && canDashAgain && !isWallJumping)
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
        UpdateApexPoint();
        ApplyMovement();
        ClampFallSpeed();
    }

    private void UpdateApexPoint()
    {
        if (isGrounded)
        {
            apexPoint = 0f;
            rb.gravityScale = baseGravityScale;
            return;
        }
        apexPoint = Mathf.InverseLerp(apexThreshold, 0f, Mathf.Abs(rb.linearVelocity.y));
        rb.gravityScale = Mathf.Lerp(baseGravityScale, baseGravityScale * apexGravityMultiplier, apexPoint);
    }

    private void ApplyMovement()
    {
        float currentSpeed = speed + apexPoint * apexBonusSpeed;
        rb.linearVelocity = new Vector2(horizontalInput * currentSpeed, rb.linearVelocity.y);
    }

    private void ClampFallSpeed()
    {
        if (rb.linearVelocity.y < -maxFallSpeed)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, -maxFallSpeed);
        }
    }

    private void Jump()
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
    }

    private IEnumerator DashRoutine()
    {
        canDashAgain = false;
        isDashing = true;
        rb.gravityScale = 0f;
        float dashDirection = isFacingRight ? 1f : -1f;
        rb.linearVelocity = new Vector2(dashDirection * dashForce, 0f);
        yield return new WaitForSeconds(dashDuration);
        rb.gravityScale = baseGravityScale;
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