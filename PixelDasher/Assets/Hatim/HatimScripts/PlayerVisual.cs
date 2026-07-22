using UnityEngine;

public class PlayerVisual : MonoBehaviour
{
    [SerializeField] private PlayerController controller;

    private void Reset()
    {
        if (controller == null) controller = GetComponentInParent<PlayerController>();
    }

    private void OnEnable()
    {
        if (controller == null) return;
        controller.OnFacingChanged += HandleFacingChanged;
    }

    private void OnDisable()
    {
        if (controller == null) return;
        controller.OnFacingChanged -= HandleFacingChanged;
    }

    private void HandleFacingChanged(bool facingRight)
    {
        Vector3 s = transform.localScale;
        s.x = Mathf.Abs(s.x) * (facingRight ? 1f : -1f);
        transform.localScale = s;
    }
}