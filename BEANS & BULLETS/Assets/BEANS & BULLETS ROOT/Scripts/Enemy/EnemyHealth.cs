using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] int maxHealth = 100;
    [SerializeField] int health;

    [Header("Feedback")]
    [SerializeField] Material damagedMat;
    [SerializeField] GameObject deathVFX;
    [SerializeField] MeshRenderer enemyRend;
    Material baseMat;

    private RoomPiece myRoom;

    private void Awake()
    {
        health = maxHealth;
        if (enemyRend != null)
            baseMat = enemyRend.material;
    }

    public void SetRoom(RoomPiece room)
    {
        myRoom = room;
    }

    // Para el GunSystem del profesor (int)
    public void TakeDamage(int damage)
    {
        health -= damage;

        if (enemyRend != null && damagedMat != null)
        {
            enemyRend.material = damagedMat;
            Invoke(nameof(ResetEnemyMaterial), 0.1f);
        }

        if (health <= 0)
        {
            health = 0;
            Die();
        }
    }

    // Para los BulletTypes de Bean & Bullets (float)
    public void TakeDamage(float damage)
    {
        TakeDamage(Mathf.RoundToInt(damage));
    }

    void Die()
    {
        if (deathVFX != null)
        {
            deathVFX.transform.parent = null;
            deathVFX.transform.position = transform.position;
            deathVFX.SetActive(true);
            Destroy(deathVFX, 3f);
        }

        if (GameTimer.Instance != null)
            GameTimer.Instance.AddKillTime();

        if (myRoom != null)
            myRoom.OnEnemyDied(this);

        // FindFirstObjectByType en vez de FindObjectOfType (Unity 6)
        HUDController hud = FindFirstObjectByType<HUDController>();
        if (hud != null)
            hud.RegisterKill();

        Destroy(gameObject);
    }

    void ResetEnemyMaterial()
    {
        if (enemyRend != null && baseMat != null)
            enemyRend.material = baseMat;
    }
}