using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform player;

    [Header("Décalage")]
    [SerializeField] private float offsetX = 3f;
    [SerializeField] private float offsetY = 1.5f;

    [Header("Suivi")]
    [SerializeField] private float followSpeed = 4f;
    [SerializeField] private bool followVertical = false;

    private void LateUpdate()
    {
        if (player == null)
            return;

        float targetY = followVertical
            ? player.position.y + offsetY
            : transform.position.y;

        Vector3 targetPosition = new Vector3(
            player.position.x + offsetX,
            targetY,
            -10f
        );

        transform.position = Vector3.Lerp(
            transform.position,
            targetPosition,
            followSpeed * Time.deltaTime
        );
    }
}