using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        HealthManager healthManager = other.GetComponent<HealthManager>();

        if (healthManager == null)
        {
            Debug.LogError("Le Player ne possède pas de HealthManager.");
            return;
        }

        healthManager.spawnPoint = gameObject;
        Debug.Log("Checkpoint activé : " + gameObject.name);
    }
}