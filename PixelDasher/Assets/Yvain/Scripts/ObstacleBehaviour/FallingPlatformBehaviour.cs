using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//Service: Ce script sert a gerer le comportement des plateformes tombantes qui reapparaissent apres un delai.
//Objet: Les plateformes tombantes
//Auteur: Yvain
//Utilisation: Aucune manipulation necessaire
public class FallingPlatformBehaviour : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField] private float delayBeforeFall = 1f;
    [SerializeField] private float hideDuration = 5f;
    [SerializeField] private float maxFallDuration = 15f;
    private Vector3 spawnPosition;
    private Rigidbody2D rb;
    private Collider2D[] platformColliders;
    private SpriteRenderer sprite;
    private bool isFalling = false;
    private void Start()
    {
        spawnPosition = transform.position;
        rb = GetComponent<Rigidbody2D>();
        sprite = GetComponent<SpriteRenderer>();
        platformColliders = GetComponents<Collider2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!isFalling && (other.CompareTag("Player") || other.CompareTag("Interact")))
        {
            StartCoroutine(FallSequence());
        }
    }
    private IEnumerator FallSequence()
    {
        isFalling = true;
        yield return new WaitForSeconds(delayBeforeFall);
        rb.bodyType = RigidbodyType2D.Dynamic;
        float timer = 0f;
        while (rb.linearVelocity.magnitude > 0.05f || timer < 0.2f)
        {
            yield return null;
            timer += Time.deltaTime;
            if (timer >= maxFallDuration)
            {
                break;
            }
        }
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.linearVelocity = Vector2.zero;
        Vector2 scanSize = new Vector2(transform.localScale.x * 0.9f, 0.2f);
        Vector2 scanCenter = (Vector2)transform.position + (Vector2.up * (transform.localScale.y / 2f + 0.1f));
        Collider2D[] hitColliders = Physics2D.OverlapBoxAll(scanCenter, scanSize, transform.eulerAngles.z);
        List<Collider2D> disabledColliders = new List<Collider2D>();
        foreach (Collider2D hitCol in hitColliders)
        {
            if (hitCol != null && hitCol.CompareTag("Interact"))
            {
                bool isPartofPlatform = false;
                foreach (Collider2D pCol in platformColliders)
                {
                    if (hitCol == pCol)
                    {
                        isPartofPlatform = true;
                        break;
                    }
                }
                if (!isPartofPlatform && hitCol.enabled)
                {
                    hitCol.enabled = false;
                    disabledColliders.Add(hitCol);
                }
            }
        }
        if (sprite != null) sprite.enabled = false;
        foreach (Collider2D pCol in platformColliders)
        {
            if (pCol != null) pCol.enabled = false;
        }
        yield return new WaitForSeconds(hideDuration);
        foreach (Collider2D objCol in disabledColliders)
        {
            if (objCol != null)
            {
                objCol.enabled = true;
            }
        }
        transform.position = spawnPosition;
        if (sprite != null) sprite.enabled = true;
        foreach (Collider2D pCol in platformColliders)
        {
            if (pCol != null) pCol.enabled = true;
        }
        isFalling = false;
    }
}