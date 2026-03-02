using UnityEngine;

[CreateAssetMenu(menuName = "Bean&Bullets/Bullet Types/Vampire")]
public class BT_Vampire : BulletType
{
    public float timePerHit = 1.5f;
    public GameObject hitEffectPrefab;

    private void OnEnable()
    {
        bulletName = "Vampire";
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

            // Bonus de tiempo extra
            if (GameTimer.Instance != null)
            {
                GameTimer.Instance.AddHitTime();
                GameTimer.Instance.AddCustomTime(timePerHit);
            }
        }
    }
}