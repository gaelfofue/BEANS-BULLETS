using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] int maxHealth = 100;
    [SerializeField] int health;

    [Header("Feedback")]
    [SerializeField] Material damagedMat;
    [SerializeField] GameObject deathVFX;
    [SerializeField] AudioClip deathSound;
    [SerializeField] float deathVolume = 3f;
    [SerializeField] MeshRenderer enemyRend;
    [SerializeField] float deathShakeIntensity = 0.12f;
    [SerializeField] float deathShakeDuration = 0.1f;

    Material baseMat;
    private RoomPiece myRoom;

    private void Awake()
    {
        health = maxHealth;
        if (enemyRend != null)
            baseMat = enemyRend.material;

        // IMPORTANTE: Desactivar deathVFX al inicio
        if (deathVFX != null)
            deathVFX.SetActive(false);
    }

    public void SetRoom(RoomPiece room)
    {
        myRoom = room;
    }

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

    public void TakeDamage(float damage)
    {
        TakeDamage(Mathf.RoundToInt(damage));
    }

    void Die()
    {
        // Screen shake más fuerte al matar
        if (CameraShake.Instance != null)
            CameraShake.Instance.Shake(deathShakeIntensity, deathShakeDuration);

        // VFX
        if (deathVFX != null)
        {
            deathVFX.transform.parent = null;
            deathVFX.transform.position = transform.position;
            deathVFX.SetActive(true);
            Destroy(deathVFX, 3f);
        }

        if (deathSound != null)
        {
            AudioSource.PlayClipAtPoint(deathSound, transform.position, deathVolume);
        }

        // Timer
        if (GameTimer.Instance != null)
        {
            GameTimer.Instance.AddKillTime();
            Debug.Log("[ENEMY] AddKillTime called");
        }
        else
        {
            Debug.LogError("[ENEMY] GameTimer.Instance is NULL!");
        }

        // Room
        if (myRoom != null)
            myRoom.OnEnemyDied(this);

        // HUD
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