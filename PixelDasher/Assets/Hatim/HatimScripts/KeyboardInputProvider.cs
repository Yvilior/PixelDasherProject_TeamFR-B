using UnityEngine;

[DefaultExecutionOrder(-100)]
public class KeyboardInputProvider : MonoBehaviour, IInputProvider
{
    [Header("Déplacement")]
    [SerializeField] private KeyCode leftKey     = KeyCode.A;
    [SerializeField] private KeyCode leftAltKey  = KeyCode.LeftArrow;
    [SerializeField] private KeyCode rightKey    = KeyCode.D;
    [SerializeField] private KeyCode rightAltKey = KeyCode.RightArrow;

    [Header("Saut")]
    [SerializeField] private KeyCode jumpKey     = KeyCode.W;
    [SerializeField] private KeyCode jumpAltKey  = KeyCode.Space;
    [SerializeField] private KeyCode jumpAltKey2 = KeyCode.UpArrow;

    [Header("Dash")]
    [SerializeField] private KeyCode dashKey     = KeyCode.E;

    [Header("Gravity Flip")]
    [SerializeField] private KeyCode flipGravityKey = KeyCode.F;

    public float Horizontal { get; private set; }
    public bool JumpDownThisFrame { get; private set; }
    public bool JumpHeld { get; private set; }
    public bool DashDownThisFrame { get; private set; }
    public bool FlipGravityDownThisFrame { get; private set; }

    private void Update()
    {
        float h = 0f;
        if (Input.GetKey(leftKey)  || Input.GetKey(leftAltKey))  h -= 1f;
        if (Input.GetKey(rightKey) || Input.GetKey(rightAltKey)) h += 1f;
        Horizontal = h;

        JumpDownThisFrame = Input.GetKeyDown(jumpKey) || Input.GetKeyDown(jumpAltKey) || Input.GetKeyDown(jumpAltKey2);
        JumpHeld          = Input.GetKey(jumpKey)     || Input.GetKey(jumpAltKey)     || Input.GetKey(jumpAltKey2);
        DashDownThisFrame = Input.GetKeyDown(dashKey);
        FlipGravityDownThisFrame = Input.GetKeyDown(flipGravityKey);
    }
}