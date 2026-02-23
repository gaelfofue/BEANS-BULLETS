using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [Header("Stats")]
    public float maxHealth = 100f;
    private float currentHealth;

    [Header("Feedback")]
    public GameObject deathEffect;

    // Referencia a la sala (asignada por Room.cs al spawnear)
    private Room myRoom;

    void Start()
    {
        currentHealth = maxHealth;
    }

    public void SetRoom(Room room)
    {
        myRoom = room;
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
        if (deathEffect != null)
        {
            Instantiate(deathEffect, transform.position, Quaternion.identity);
        }

        // Dar tiempo por kill
        if (GameTimer.Instance != null)
        {
            GameTimer.Instance.AddKillTime();
        }

        // Notificar a la sala
        if (myRoom != null)
        {
            myRoom.OnEnemyDied(this);
        }

        Destroy(gameObject);
    }

    private System.Collections.IEnumerator DamageFlash()
    {
        Renderer renderer = GetComponentInChildren<Renderer>();
        if (renderer == null) yield break;

        Color originalColor = renderer.material.color;
        renderer.material.color = Color.white;
        yield return new WaitForSeconds(0.1f);

        if (this != null && renderer != null)
            renderer.material.color = originalColor;
    }
}