using UnityEngine;

[CreateAssetMenu(menuName = "Bean&Bullets/Bullet Types/Explosive")]
public class BT_Explosive : BulletType
{
    public float explosionRadius = 5f;
    public float explosionDamageMultiplier = 0.5f;
    public GameObject explosionEffectPrefab;

    private void OnEnable()
    {
        bulletName = "Explosive";
    }

    public override void OnHit(RaycastHit hit, float damage, Vector3 shootDirection)
    {
        // Efecto
        if (explosionEffectPrefab != null)
        {
            GameObject effect = Instantiate(explosionEffectPrefab, hit.point, Quaternion.identity);
            Destroy(effect, 3f);
        }

        // Daño directo
        EnemyHealth directEnemy = hit.collider.GetComponentInParent<EnemyHealth>();
        if (directEnemy != null)
        {
            directEnemy.TakeDamage(damage);
            if (GameTimer.Instance != null)
                GameTimer.Instance.AddHitTime();
        }

        // Daño de área
        Collider[] nearby = Physics.OverlapSphere(hit.point, explosionRadius);
        foreach (Collider col in nearby)
        {
            EnemyHealth aoeEnemy = col.GetComponentInParent<EnemyHealth>();
            if (aoeEnemy != null && aoeEnemy != directEnemy)
            {
                float distance = Vector3.Distance(hit.point, col.transform.position);
                float falloff = 1f - (distance / explosionRadius);
                float aoeDamage = damage * explosionDamageMultiplier * falloff;
                aoeEnemy.TakeDamage(aoeDamage);
            }
        }
    }
}