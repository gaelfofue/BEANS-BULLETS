using UnityEngine;

public class EnemyProjectile : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] private float speed = 15f;
    [SerializeField] private float lifetime = 5f;
    [SerializeField] private float damage = 2.5f;

    private Vector3 direction;
    private bool initialized = false;

    public void Launch(Vector3 dir, float projectileSpeed, float projectileDamage)
    {
        direction = dir.normalized;
        speed = projectileSpeed;
        damage = projectileDamage;
        initialized = true;

        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        if (!initialized) return;

        transform.position += direction * speed * Time.deltaTime;

        if (direction != Vector3.zero)
            transform.rotation = Quaternion.LookRotation(direction);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy")) return;

        if (other.CompareTag("Player"))
        {
            if (GameTimer.Instance != null)
                GameTimer.Instance.RemoveTime(damage);
        }

        Destroy(gameObject);
    }
}