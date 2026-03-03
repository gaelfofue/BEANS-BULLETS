using UnityEngine;

[CreateAssetMenu(menuName = "Bean&Bullets/Bullet Types/Standard")]
public class BT_Standard : BulletType
{
    [Header("Effects")]
    public GameObject hitEffectPrefab;

    private void OnEnable()
    {
        bulletName = "Standard";
    }

    public override void OnHit(RaycastHit hit, float damage, Vector3 shootDirection)
    {
        if (hitEffectPrefab != null)
        {
            GameObject effect = Instantiate(hitEffectPrefab, hit.point, Quaternion.LookRotation(hit.normal));
            Destroy(effect, 2f);
        }

        EnemyHealth enemy = hit.collider.GetComponentInParent<EnemyHealth>();
        if (enemy != null)
        {
            float finalDamage = damage;
            if (hit.collider.CompareTag("Headshot"))
                finalDamage *= 2f;

            enemy.TakeDamage(finalDamage);
        }
    }
}