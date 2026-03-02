using UnityEngine;

[CreateAssetMenu(menuName = "Bean&Bullets/Bullet Types/Ricochet")]
public class BT_Ricochet : BulletType
{
    public int maxBounces = 3;
    public float bounceRange = 30f;
    public float damageFalloff = 0.7f;
    public GameObject hitEffectPrefab;
    public LayerMask hitMask;

    private void OnEnable()
    {
        bulletName = "Ricochet";
    }

    public override void OnHit(RaycastHit hit, float damage, Vector3 shootDirection)
    {
        if (hitEffectPrefab != null)
        {
            GameObject effect = Instantiate(hitEffectPrefab, hit.point, Quaternion.LookRotation(hit.normal));
            Destroy(effect, 2f);
        }

        // Daño al primer target
        EnemyHealth enemy = hit.collider.GetComponentInParent<EnemyHealth>();
        if (enemy != null)
        {
            enemy.TakeDamage(damage);
            if (GameTimer.Instance != null)
                GameTimer.Instance.AddHitTime();
        }

        // Rebotes
        Vector3 currentPoint = hit.point;
        Vector3 currentDir = Vector3.Reflect(shootDirection, hit.normal);
        float currentDamage = damage;

        for (int i = 0; i < maxBounces; i++)
        {
            currentDamage *= damageFalloff;

            RaycastHit bounceHit;
            if (Physics.Raycast(currentPoint + currentDir * 0.1f, currentDir, out bounceHit, bounceRange, hitMask))
            {
                if (hitEffectPrefab != null)
                {
                    GameObject effect = Instantiate(hitEffectPrefab, bounceHit.point, Quaternion.LookRotation(bounceHit.normal));
                    Destroy(effect, 2f);
                }

                EnemyHealth bounceEnemy = bounceHit.collider.GetComponentInParent<EnemyHealth>();
                if (bounceEnemy != null)
                {
                    bounceEnemy.TakeDamage(currentDamage);
                    if (GameTimer.Instance != null)
                        GameTimer.Instance.AddHitTime();
                }

                currentPoint = bounceHit.point;
                currentDir = Vector3.Reflect(currentDir, bounceHit.normal);
            }
            else
            {
                break;
            }
        }
    }
}