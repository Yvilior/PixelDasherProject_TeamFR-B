using UnityEngine;

[RequireComponent(typeof(Animator))]
public class PlayerAnimator : MonoBehaviour
{
    [SerializeField] private PlayerController controller;
    private Animator animator;

    // Bools
    private static readonly int IsGroundedHash    = Animator.StringToHash("IsGrounded");
    private static readonly int IsMovingHash      = Animator.StringToHash("IsMoving");
    private static readonly int IsFallingHash     = Animator.StringToHash("IsFalling");
    private static readonly int IsWallSlidingHash = Animator.StringToHash("IsWallSliding");
    private static readonly int IsDashingHash     = Animator.StringToHash("IsDashing");

    // Triggers
    private static readonly int JumpTrigger       = Animator.StringToHash("Jump");
    private static readonly int DoubleJumpTrigger = Animator.StringToHash("DoubleJump");
    private static readonly int WallJumpTrigger   = Animator.StringToHash("WallJump");
    private static readonly int DashTrigger       = Animator.StringToHash("Dash");

    private void Reset()
    {
        if (controller == null) controller = GetComponentInParent<PlayerController>();
    }

    private void Awake()
    {
        animator = GetComponent<Animator>();
        if (controller == null) controller = GetComponentInParent<PlayerController>();
    }

    private void OnEnable()
    {
        if (controller == null) return;
        controller.OnJumped       += HandleJump;
        controller.OnDoubleJumped += HandleDoubleJump;
        controller.OnWallJumped   += HandleWallJump;
        controller.OnDashStarted  += HandleDash;
    }

    private void OnDisable()
    {
        if (controller == null) return;
        controller.OnJumped       -= HandleJump;
        controller.OnDoubleJumped -= HandleDoubleJump;
        controller.OnWallJumped   -= HandleWallJump;
        controller.OnDashStarted  -= HandleDash;
    }

    private void Update()
    {
        if (controller == null) return;

        animator.SetBool(IsGroundedHash,    controller.IsGrounded);
        animator.SetBool(IsMovingHash,      Mathf.Abs(controller.HorizontalInput) > 0.01f);
        animator.SetBool(IsWallSlidingHash, controller.IsWallSliding);
        animator.SetBool(IsDashingHash,     controller.IsDashing);
    }

    private void HandleJump()       => animator.SetTrigger(JumpTrigger);
    private void HandleDoubleJump() => animator.SetTrigger(DoubleJumpTrigger);
    private void HandleWallJump()   => animator.SetTrigger(WallJumpTrigger);
    private void HandleDash()       => animator.SetTrigger(DashTrigger);
}