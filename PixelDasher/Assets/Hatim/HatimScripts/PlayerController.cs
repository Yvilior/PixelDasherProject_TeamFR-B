using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(CapsuleCollider2D))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] private PlayerStats stats;

    [Header("Debug / Gizmos")]
    [SerializeField] private bool drawJumpArc = true;
    [SerializeField] private Color jumpArcColor = Color.cyan;
    [SerializeField, Range(50, 500)] private int jumpArcSteps = 200;
    [SerializeField] private bool stopArcOnCollision = true;

    private Rigidbody2D rb;
    private CapsuleCollider2D col;
    private PlayerAbilities abilities;
    private IInputProvider inputProvider;

    // ===== EVENTS =====
    public event Action OnJumped;
    public event Action OnDoubleJumped;
    public event Action OnWallJumped;
    public event Action OnLanded;
    public event Action OnDashStarted;
    public event Action OnDashEnded;
    public event Action OnWallSlideStarted;
    public event Action OnWallSlideEnded;
    public event Action<bool> OnFacingChanged;
    public event Action<bool> OnGravityFlipped;

    // ===== PROPERTIES PUBLIQUES =====
    public bool IsGrounded     => grounded;
    public bool IsDashing      => isDashing;
    public bool IsWallSliding  => isWallSlidingNow;
    public bool IsRising       => !grounded && LocalVy > 0f;
    public bool IsFalling      => !grounded && LocalVy < 0f;
    public bool FacingRight    => isFacingRight;
    public bool GravityFlipped => gravitySign < 0;
    public Vector2 Velocity    => frameVelocity;
    public float HorizontalInput => input.Move.x;

    private struct FrameInput
    {
        public bool JumpDown;
        public bool JumpHeld;
        public bool DashDown;
        public bool FlipGravityDown;
        public Vector2 Move;
    }
    private FrameInput input;

    private Vector2 frameVelocity;
    private float time;
    private bool cachedQueryStartInColliders;

    // Gravity direction (+1 = normal, -1 = flipped)
    private int gravitySign = 1;
    private float LocalVy => frameVelocity.y * gravitySign;

    // Collisions
    private bool grounded;
    private bool touchingWallLeft, touchingWallRight;   // pour le wall jump (avec anti-spam)
    private bool wallContactLeft, wallContactRight;     // pour le wall slide (contact physique pur)
    private float timeLeftGrounded = float.MinValue;
    private int lastWallJumpSide;

    // Jump state
    private bool jumpToConsume;
    private bool bufferedJumpUsable;
    private bool coyoteUsable;
    private bool endedJumpEarly;
    private float timeJumpPressed = float.MinValue;
    private bool HasBufferedJump => bufferedJumpUsable && time < timeJumpPressed + stats.JumpBuffer;
    private bool CanUseCoyote => coyoteUsable && !grounded && time < timeLeftGrounded + stats.CoyoteTime;

    private bool doubleJumpUsable;

    private float wallJumpLockUntil = float.MinValue;
    private bool IsWallJumpLocked => time < wallJumpLockUntil;
    private float lastWallJumpDir;

    // Dash
    private bool dashToConsume;
    private bool isDashing;
    private float dashEndTime;
    private float groundDashCooldownUntil; // cooldown du dash au sol
    private bool airDashUsable = true;     // une seule charge en l'air

    // Gravity flip input
    private bool flipGravityToConsume;

    // Facing
    private bool isFacingRight = true;

    // Wall slide tracking
    private bool isWallSlidingNow;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<CapsuleCollider2D>();
        abilities = GetComponent<PlayerAbilities>();
        inputProvider = GetComponent<IInputProvider>();

        if (inputProvider == null)
            Debug.LogError($"{nameof(PlayerController)} : aucun IInputProvider trouvé sur le GameObject !", this);

        cachedQueryStartInColliders = Physics2D.queriesStartInColliders;
        rb.gravityScale = 0f;

#if UNITY_EDITOR
        abilities.UnlockAll();
#endif
    }

    private void Start()
    {
        OnFacingChanged?.Invoke(isFacingRight);
        OnGravityFlipped?.Invoke(GravityFlipped);
    }

    private void Update()
    {
        time += Time.deltaTime;
        GatherInput();
    }

    private void GatherInput()
    {
        input = new FrameInput
        {
            JumpDown         = inputProvider.JumpDownThisFrame,
            JumpHeld         = inputProvider.JumpHeld,
            DashDown         = inputProvider.DashDownThisFrame,
            FlipGravityDown  = inputProvider.FlipGravityDownThisFrame,
            Move             = new Vector2(inputProvider.Horizontal, 0f)
        };

        if (stats.SnapInput)
            input.Move.x = Mathf.Abs(input.Move.x) < stats.HorizontalDeadZone ? 0f : Mathf.Sign(input.Move.x);

        if (input.JumpDown) { jumpToConsume = true; timeJumpPressed = time; }
        if (input.DashDown) dashToConsume = true;
        if (input.FlipGravityDown) flipGravityToConsume = true;
    }

    private void FixedUpdate()
    {
        CheckCollisions();
        HandleGravityFlip();
        HandleDash();
        if (isDashing) { ApplyMovement(); return; }
        HandleJump();
        HandleDirection();
        HandleGravity();
        HandleFacing();
        UpdateWallSlideState();
        ApplyMovement();
    }

    // ===== COLLISIONS =====
    private void CheckCollisions()
    {
        Physics2D.queriesStartInColliders = false;

        int mask = ~stats.PlayerLayer;
        Vector2 size = col.bounds.size;

        Vector2 downDir = Vector2.down * gravitySign;
        Vector2 upDir   = -downDir;

        bool groundHit  = Physics2D.CapsuleCast(col.bounds.center, size, col.direction, 0f, downDir, stats.GrounderDistance, mask);
        bool ceilingHit = Physics2D.CapsuleCast(col.bounds.center, size, col.direction, 0f, upDir,   stats.GrounderDistance, mask);

        if (ceilingHit && LocalVy > 0f) frameVelocity.y = 0f;

        if (!grounded && groundHit)
        {
            grounded = true;
            coyoteUsable = true;
            bufferedJumpUsable = true;
            doubleJumpUsable = true;
            airDashUsable = true;
            endedJumpEarly = false;
            lastWallJumpSide = 0;
            OnLanded?.Invoke();
        }
        else if (grounded && !groundHit)
        {
            grounded = false;
            timeLeftGrounded = time;
        }

        float halfWidth = col.bounds.extents.x;
        var hitR = Physics2D.Raycast(col.bounds.center, Vector2.right, halfWidth + stats.WallDetectionDistance, mask);
        var hitL = Physics2D.Raycast(col.bounds.center, Vector2.left,  halfWidth + stats.WallDetectionDistance, mask);
        bool isWallR = hitR.collider != null && hitR.collider.CompareTag(stats.WallTag);
        bool isWallL = hitL.collider != null && hitL.collider.CompareTag(stats.WallTag);

        if (!grounded)
       {
        wallContactRight = isWallR;
        wallContactLeft  = isWallL;

         touchingWallRight = isWallR && lastWallJumpSide != 1;
         touchingWallLeft  = isWallL && lastWallJumpSide != -1;

        // Re-arm seulement sur un mur "frais" (pas celui qu'on vient de wall-jumper)
         if ((touchingWallRight || touchingWallLeft) && abilities.Has(AbilityType.WallJump))
        {
         airDashUsable = true; 
        }

        }
        else
        {
            wallContactRight = false;
            wallContactLeft = false;
            touchingWallRight = false;
            touchingWallLeft = false;
        }

        Physics2D.queriesStartInColliders = cachedQueryStartInColliders;
    }

    // ===== GRAVITY FLIP =====
    private void HandleGravityFlip()
    {
        if (!flipGravityToConsume) return;
        flipGravityToConsume = false;

        if (!abilities.Has(AbilityType.GravityFlip)) return;

        gravitySign = -gravitySign;

        grounded = false;
        coyoteUsable = false;
        bufferedJumpUsable = false;
        endedJumpEarly = false;
        timeLeftGrounded = float.MinValue;
        lastWallJumpSide = 0;

        OnGravityFlipped?.Invoke(GravityFlipped);
    }

    // ===== SAUT =====
    private void HandleJump()
    {
        if (!endedJumpEarly && !grounded && !input.JumpHeld && LocalVy > 0f)
            endedJumpEarly = true;

        if (!jumpToConsume && !HasBufferedJump) return;

        if (grounded || CanUseCoyote)
        {
            ExecuteJump(stats.JumpPower);
            OnJumped?.Invoke();
        }
        else if ((touchingWallLeft || touchingWallRight) && abilities.Has(AbilityType.WallJump))
        {
            ExecuteWallJump();
            OnWallJumped?.Invoke();
        }
        else if (doubleJumpUsable && abilities.Has(AbilityType.DoubleJump))
        {
            ExecuteDoubleJump();
            OnDoubleJumped?.Invoke();
        }

        jumpToConsume = false;
    }

    private void ExecuteJump(float power)
    {
        endedJumpEarly = false;
        timeJumpPressed = float.MinValue;
        bufferedJumpUsable = false;
        coyoteUsable = false;
        frameVelocity.y = power * gravitySign;
    }

    private void ExecuteDoubleJump()
    {
        doubleJumpUsable = false;
        ExecuteJump(stats.DoubleJumpPower);
    }

    private void ExecuteWallJump()
    {
        endedJumpEarly = false;
        timeJumpPressed = float.MinValue;
        bufferedJumpUsable = false;

        int wallSide = touchingWallRight ? 1 : -1;
        float awayDir = -wallSide;

        bool pressingAway = Mathf.Abs(input.Move.x) > 0.01f &&
                            Mathf.Sign(input.Move.x) == awayDir;

        float xVel;
        if (pressingAway)
        {
            xVel = awayDir * stats.WallJumpForce.x;
            wallJumpLockUntil = time + stats.WallJumpLockTime;
            lastWallJumpDir = awayDir;
        }
        else
        {
            xVel = 0f;
            wallJumpLockUntil = time;
            lastWallJumpDir = 0f;
        }

        // Anti-spam : consomme la paroi quel que soit le type de saut
        lastWallJumpSide = wallSide;

        frameVelocity = new Vector2(xVel, stats.WallJumpForce.y * gravitySign);
        doubleJumpUsable = false;
        airDashUsable = true;
    }

    // ===== HORIZONTAL =====
    private void HandleDirection()
    {
        if (IsWallJumpLocked)
        {
            frameVelocity.x = Mathf.MoveTowards(frameVelocity.x, lastWallJumpDir * stats.MaxSpeed,
                                                stats.AirDeceleration * Time.fixedDeltaTime);
            return;
        }

        float apex = grounded ? 0f : Mathf.InverseLerp(stats.ApexThreshold, 0f, Mathf.Abs(frameVelocity.y));
        float targetSpeed = stats.MaxSpeed + apex * stats.ApexBonusSpeed;

        if (input.Move.x == 0f)
        {
            float decel = grounded ? stats.GroundDeceleration : stats.AirDeceleration;
            frameVelocity.x = Mathf.MoveTowards(frameVelocity.x, 0f, decel * Time.fixedDeltaTime);
        }
        else
        {
            frameVelocity.x = Mathf.MoveTowards(frameVelocity.x, input.Move.x * targetSpeed,
                                                stats.Acceleration * Time.fixedDeltaTime);
        }
    }

    // ===== GRAVITÉ =====
    private void HandleGravity()
    {
        if (grounded && LocalVy <= 0f)
        {
            frameVelocity.y = stats.GroundingForce * gravitySign;
            return;
        }

        float gravity = stats.FallAcceleration;

        if (endedJumpEarly && LocalVy > 0f)
            gravity *= stats.JumpEndEarlyGravityModifier;

        float apex = Mathf.InverseLerp(stats.ApexThreshold, 0f, Mathf.Abs(frameVelocity.y));
        gravity = Mathf.Lerp(gravity, gravity * stats.ApexGravityMultiplier, apex);

        float targetVy = -stats.MaxFallSpeed * gravitySign;
        frameVelocity.y = Mathf.MoveTowards(frameVelocity.y, targetVy, gravity * Time.fixedDeltaTime);

        if (IsCurrentlyWallSliding())
        {
            if (gravitySign > 0)
                frameVelocity.y = Mathf.Max(frameVelocity.y, -stats.WallSlideSpeed);
            else
                frameVelocity.y = Mathf.Min(frameVelocity.y, stats.WallSlideSpeed);
        }
    }

    private bool IsCurrentlyWallSliding()
    {
        if (grounded) return false;
        if (!wallContactLeft && !wallContactRight) return false;
        if (!abilities.Has(AbilityType.WallSlide)) return false;
        if (LocalVy > 0f) return false;
        return (wallContactLeft && input.Move.x < 0f) || (wallContactRight && input.Move.x > 0f);
    }

    private void UpdateWallSlideState()
    {
        bool now = IsCurrentlyWallSliding();
        if (now && !isWallSlidingNow) OnWallSlideStarted?.Invoke();
        else if (!now && isWallSlidingNow) OnWallSlideEnded?.Invoke();
        isWallSlidingNow = now;
    }

    // ===== DASH =====
    private void HandleDash()
    {
        if (isDashing)
        {
            if (time >= dashEndTime)
            {
                isDashing = false;
                frameVelocity.x *= stats.DashEndSpeedMultiplier;
                OnDashEnded?.Invoke();
            }
            return;
        }

        if (!dashToConsume || !abilities.Has(AbilityType.Dash)) return;

        // Vérif de disponibilité selon le contexte
        if (grounded)
        {
            if (time < groundDashCooldownUntil) { dashToConsume = false; return; }
        }
        else
        {
            if (!airDashUsable) { dashToConsume = false; return; }
        }

        isDashing = true;
        float dir = isFacingRight ? 1f : -1f;
        frameVelocity = new Vector2(dir * stats.DashSpeed, 0f);
        dashEndTime = time + stats.DashDuration;

        if (grounded)
            groundDashCooldownUntil = time + stats.DashDuration + stats.DashCooldown;
        else
            airDashUsable = false;

        dashToConsume = false;
        OnDashStarted?.Invoke();
    }

    // ===== FACING =====
    private void HandleFacing()
   {
    if (IsWallJumpLocked) return;

    // Pendant le wall slide, on force le facing vers le mur
    if (isWallSlidingNow)
    {
        if (wallContactRight) SetFacing(true);
        else if (wallContactLeft) SetFacing(false);
        return;
    }
    if (input.Move.x > 0f) SetFacing(true);
    else if (input.Move.x < 0f) SetFacing(false);
    }

    private void SetFacing(bool facingRight)
    {
        if (isFacingRight == facingRight) return;
        isFacingRight = facingRight;
        OnFacingChanged?.Invoke(isFacingRight);
    }

    private void ApplyMovement() => rb.linearVelocity = frameVelocity;

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (stats == null) Debug.LogWarning("PlayerController : assigne un PlayerStats", this);
    }

    private void OnDrawGizmosSelected()
    {
        if (stats == null || !drawJumpArc) return;
        Vector2 startPos = transform.position;
        DrawSimulatedJumpArc(startPos, +1f);
        DrawSimulatedJumpArc(startPos, -1f);
    }

    private void DrawSimulatedJumpArc(Vector2 startPos, float horizontalDir)
    {
        int simGS = Application.isPlaying ? gravitySign : 1;
        Vector2 velocity = new Vector2(horizontalDir * stats.MaxSpeed, stats.JumpPower * simGS);
        Vector2 position = startPos;
        Vector2 prev = position;
        float dt = Time.fixedDeltaTime;

        Gizmos.color = jumpArcColor;

        for (int i = 0; i < jumpArcSteps; i++)
        {
            float gravity = stats.FallAcceleration;
            float apex = Mathf.InverseLerp(stats.ApexThreshold, 0f, Mathf.Abs(velocity.y));
            gravity = Mathf.Lerp(gravity, gravity * stats.ApexGravityMultiplier, apex);
            float targetVy = -stats.MaxFallSpeed * simGS;
            velocity.y = Mathf.MoveTowards(velocity.y, targetVy, gravity * dt);

            float targetSpeedX = stats.MaxSpeed + apex * stats.ApexBonusSpeed;
            velocity.x = horizontalDir * targetSpeedX;

            position += velocity * dt;

            if (stopArcOnCollision)
            {
                Vector2 segment = position - prev;
                RaycastHit2D hit = Physics2D.Raycast(prev, segment.normalized, segment.magnitude, ~stats.PlayerLayer);
                if (hit.collider != null)
                {
                    Gizmos.DrawLine(prev, hit.point);
                    Gizmos.DrawWireSphere(hit.point, 0.15f);
                    return;
                }
            }

            Gizmos.DrawLine(prev, position);
            prev = position;

            if (i > 5 && (position.y - startPos.y) * simGS < -0.1f) return;
        }
    }
#endif
}