using UnityEngine;

public class EnemyProjectile : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] private float speed = 15f;
    [SerializeField] private float lifetime = 5f;
    [SerializeField] private float damage = 2.5f;

    private Vector3 direction;
    private bool initialized = false;

    /// <summary>
    /// Llamar justo después de Instantiate para darle dirección.
    /// </summary>
    public void Launch(Vector3 dir, float projectileSpeed, float projectileDamage)
    {
        direction = dir.normalized;
        speed = projectileSpeed;
        damage = projectileDamage;
        initialized = true;

        // Auto destruir si no pega nada
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        if (!initialized) return;

        // Mover con transform, NO con Rigidbody
        transform.position += direction * speed * Time.deltaTime;

        // Mirar hacia donde va
        if (direction != Vector3.zero)
            transform.rotation = Quaternion.LookRotation(direction);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy")) return; // No pegarse a sí mismo

        if (other.CompareTag("Player"))
        {
            // Quitar tiempo del timer
            if (GameTimer.Instance != null)
                GameTimer.Instance.RemoveTime(damage);

            Debug.Log($"[PROJECTILE] Hit player! -{damage}s");
        }

        // Destruir al impactar con cualquier cosa
        Destroy(gameObject);
    }
}