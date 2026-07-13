public interface IInputProvider
{
    float Horizontal { get; }
    bool JumpDownThisFrame { get; }
    bool JumpHeld { get; }
    bool DashDownThisFrame { get; }
    bool FlipGravityDownThisFrame { get; }
}