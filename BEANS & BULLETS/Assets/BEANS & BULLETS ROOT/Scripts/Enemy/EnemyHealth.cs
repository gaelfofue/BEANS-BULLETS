using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [Header("Stats")]
    public float maxHealth = 100f;
    private float currentHealth;

    [Header("Feedback")]
    public GameObject deathEffect;

    void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;

        StartCoroutine(DamageFlash());

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        // Partículas de muerte
        if (deathEffect != null)
        {
            Instantiate(deathEffect, transform.position, Quaternion.identity);
        }

        // Dar tiempo al player (futuro)
        // if (GameTimer.Instance != null)
        //     GameTimer.Instance.AddTime();

        // Avisar a la room (futuro)
        // RoomManager room = FindObjectOfType<RoomManager>();
        // if (room != null)
        //     room.EnemyKilled();

        Destroy(gameObject);
    }

    private System.Collections.IEnumerator DamageFlash()
    {
        Renderer renderer = GetComponentInChildren<Renderer>();
        if (renderer == null) yield break;

        Color originalColor = renderer.material.color;
        renderer.material.color = Color.white;
        yield return new WaitForSeconds(0.1f);

        // Por si murió durante el flash
        if (this != null && renderer != null)
            renderer.material.color = originalColor;
    }
}