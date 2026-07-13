using UnityEngine;

[CreateAssetMenu(menuName = "Player/Stats", fileName = "PlayerStats")]
public class PlayerStats : ScriptableObject
{
    [Header("LAYERS")]
    public LayerMask PlayerLayer;
    public string WallTag = "Wall";

    [Header("INPUT")]
    public bool SnapInput = true;
    [Range(0.01f, 0.99f)] public float HorizontalDeadZone = 0.1f;

    [Header("MOUVEMENT")]
    public float MaxSpeed = 14f;
    public float Acceleration = 120f;
    public float GroundDeceleration = 60f;
    public float AirDeceleration = 30f;
    [Range(-10f, 0f)] public float GroundingForce = -1.5f;
    [Range(0f, 0.5f)] public float GrounderDistance = 0.1f;

    [Header("SAUT")]
    public float JumpPower = 36f;
    public float MaxFallSpeed = 40f;
    public float FallAcceleration = 110f;
    public float JumpEndEarlyGravityModifier = 3f;
    public float CoyoteTime = 0.15f;
    public float JumpBuffer = 0.2f;

    [Header("APEX MODIFIER")]
    public float ApexThreshold = 10f;
    public float ApexBonusSpeed = 4f;
    [Range(0f, 1f)] public float ApexGravityMultiplier = 0.5f;

    [Header("DOUBLE JUMP")]
    public float DoubleJumpPower = 32f;

    [Header("WALL JUMP")]
    [Range(0f, 0.5f)] public float WallDetectionDistance = 0.05f;
    public Vector2 WallJumpForce = new Vector2(20f, 30f);
    public float WallJumpLockTime = 0.15f;

    [Header("WALL SLIDE")]
    public float WallSlideSpeed = 3f;

    [Header("DASH")]
    public float DashSpeed = 25f;
    public float DashDuration = 0.15f;
    public float DashCooldown = 1f;
    [Tooltip("Multiplicateur de vitesse appliqué quand le dash se termine (0 = arrêt net, 1 = garde toute la vitesse)")]
    [Range(0f, 1f)] public float DashEndSpeedMultiplier = 0.5f;

}