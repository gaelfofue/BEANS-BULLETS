using UnityEngine;
using UnityEngine.InputSystem;

public class GunSystem : MonoBehaviour
{
    [Header("Base Stats")]
    [SerializeField] private GunStats stats;

    [Header("Current Loadout")]
    [SerializeField] private FireMode fireMode;
    [SerializeField] private BulletType bulletType;

    [Header("Layers")]
    [SerializeField] private LayerMask hitMask;

    [Header("References")]
    [SerializeField] private GunRecoil gunRecoil;
    [SerializeField] private ShootVFXController vfx;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip fireSound;
    [SerializeField] private AudioClip reloadSound;
    [SerializeField] private AudioClip emptySound;

    // Estado
    private int currentAmmo;
    private float timeSinceLastShot;
    private bool isReloading;
    private float reloadTimer;

    // Input
    private bool inputDown;
    private bool inputHeld;
    private bool inputConsumed = true;

    // Burst
    private int burstRemaining = 0;
    private float burstTimer = 0f;

    // Cache
    private Camera cam;
    private HUDController hud;
    private CrosshairUI crosshair;

    private void Start()
    {
        currentAmmo = stats.magSize;
        cam = Camera.main;
        hud = FindObjectOfType<HUDController>();
        crosshair = FindObjectOfType<CrosshairUI>();
        UpdateHUD();
    }

    private void Update()
    {
        timeSinceLastShot += Time.deltaTime;

        // DEBUG
        if (inputDown && !inputConsumed)
        {
            Debug.Log($"FIRE INPUT | isReloading:{isReloading} ammo:{currentAmmo} timeSince:{timeSinceLastShot:F2} fireMode:{fireMode != null} bulletType:{bulletType != null} stats:{stats != null}");
        }

        // Recarga
        if (isReloading)
        {
            reloadTimer -= Time.deltaTime;
            if (reloadTimer <= 0f)
                FinishReload();
            return;
        }

        // Burst pendiente
        if (burstRemaining > 0)
        {
            burstTimer -= Time.deltaTime;
            if (burstTimer <= 0f)
            {
                DoSingleShot();
                burstRemaining--;
                if (burstRemaining > 0)
                {
                    FM_Burst burst = fireMode as FM_Burst;
                    burstTimer = burst != null ? burst.burstDelay : 0.08f;
                }
            }
            return;
        }

        // Comprobar si puede disparar
        if (fireMode == null) return;

        bool canFire = fireMode.CanFire(
            inputDown && !inputConsumed,
            inputHeld,
            timeSinceLastShot,
            stats.fireRate
        );

        if (canFire)
        {
            inputConsumed = true;
            TryShoot();
        }
    }

    #region INPUT

    public void OnFire(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            inputDown = true;
            inputHeld = true;
            inputConsumed = false;
        }

        if (context.canceled)
        {
            inputDown = false;
            inputHeld = false;
        }
    }

    public void OnReload(InputAction.CallbackContext context)
    {
        if (context.performed)
            TryReload();
    }

    #endregion

    #region SHOOT

    private void TryShoot()
    {
        if (isReloading) return;

        if (currentAmmo <= 0)
        {
            PlaySound(emptySound);
            TryReload();
            return;
        }

        int rayCount = fireMode.GetRayCount();
        float spread = fireMode.GetSpreadAngle();

        if (rayCount > 1 && spread > 0f)
        {
            DoShotgunBlast(rayCount, spread);
        }
        else if (rayCount > 1 && fireMode is FM_Burst)
        {
            DoSingleShot();
            burstRemaining = rayCount - 1;
            FM_Burst burst = fireMode as FM_Burst;
            burstTimer = burst.burstDelay;
        }
        else
        {
            DoSingleShot();
        }
    }

    private void DoShotgunBlast(int pellets, float spread)
    {
        currentAmmo--;
        timeSinceLastShot = 0f;

        PlaySound(fireSound);

        if (crosshair != null)
            crosshair.OnShoot();

        if (gunRecoil != null)
            gunRecoil.DoRecoil();

        // Muzzle VFX una sola vez
        if (vfx != null)
            vfx.PlayMuzzleOnly();

        bool hitAnyEnemy = false;

        for (int i = 0; i < pellets; i++)
        {
            Vector3 direction = cam.transform.forward;
            direction += cam.transform.right * Random.Range(-spread, spread);
            direction += cam.transform.up * Random.Range(-spread, spread);
            direction.Normalize();

            Ray ray = new Ray(cam.transform.position, direction);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, stats.range, hitMask))
            {
                // Trail + Impact por cada pellet
                if (vfx != null)
                    vfx.PlayTrailAndImpact(hit.point, true, hit.normal);

                if (bulletType != null)
                {
                    float finalDamage = stats.damage * bulletType.GetDamageMultiplier();
                    finalDamage /= pellets * 0.5f;
                    bulletType.OnHit(hit, finalDamage, direction);
                }

                EnemyHealth enemy = hit.collider.GetComponentInParent<EnemyHealth>();
                if (enemy != null)
                    hitAnyEnemy = true;
            }
        }

        if (hitAnyEnemy && crosshair != null)
            crosshair.OnHit();

        UpdateHUD();

        if (currentAmmo <= 0)
            TryReload();
    }

    private void DoSingleShot()
    {
        if (currentAmmo <= 0) return;

        currentAmmo--;
        timeSinceLastShot = 0f;

        // Audio
        PlaySound(fireSound);

        // Crosshair
        if (crosshair != null)
            crosshair.OnShoot();

        // Recoil
        if (gunRecoil != null)
            gunRecoil.DoRecoil();

        // Dirección
        Vector3 direction = cam.transform.forward;
        float spread = fireMode.GetSpreadAngle();
        if (spread > 0f)
        {
            direction += cam.transform.right * Random.Range(-spread, spread);
            direction += cam.transform.up * Random.Range(-spread, spread);
            direction.Normalize();
        }

        // Raycast
        Ray ray = new Ray(cam.transform.position, direction);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, stats.range, hitMask))
        {
            // VFX con impacto
            if (vfx != null)
                vfx.PlayShootVFX(hit.point, true, hit.normal);

            if (bulletType != null)
            {
                float finalDamage = stats.damage * bulletType.GetDamageMultiplier();
                bulletType.OnHit(hit, finalDamage, direction);
            }

            EnemyHealth enemy = hit.collider.GetComponentInParent<EnemyHealth>();
            if (enemy != null && crosshair != null)
                crosshair.OnHit();
        }
        else
        {
            // VFX sin impacto
            if (vfx != null)
                vfx.PlayShootVFX(Vector3.zero, false, Vector3.zero);
        }

        UpdateHUD();

        if (currentAmmo <= 0)
            TryReload();
    }

    #endregion

    #region RELOAD

    private void TryReload()
    {
        if (isReloading) return;
        if (currentAmmo >= stats.magSize) return;

        isReloading = true;
        reloadTimer = stats.reloadTime;

        PlaySound(reloadSound);

        if (gunRecoil != null)
            gunRecoil.DoReloadSpin(stats.reloadTime);
    }

    private void FinishReload()
    {
        isReloading = false;
        currentAmmo = stats.magSize;
        timeSinceLastShot = stats.fireRate; // PERMITE DISPARAR INMEDIATAMENTE
        UpdateHUD();
    }

    #endregion

    #region LOADOUT

    public void SetFireMode(FireMode newMode)
    {
        fireMode = newMode;
        burstRemaining = 0;

        hud = FindObjectOfType<HUDController>();
        if (hud != null && newMode != null)
            hud.SetUpgrade(0, newMode.icon);
    }

    public void SetBulletType(BulletType newType)
    {
        bulletType = newType;

        hud = FindObjectOfType<HUDController>();
        if (hud != null && newType != null)
            hud.SetUpgrade(1, newType.icon);
    }

    public FireMode GetFireMode() { return fireMode; }
    public BulletType GetBulletType() { return bulletType; }

    #endregion

    #region HELPERS

    private void UpdateHUD()
    {
        if (hud != null)
            hud.UpdateAmmo(currentAmmo, stats.magSize);
    }

    private void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
            audioSource.PlayOneShot(clip);
    }

    #endregion
}