using UnityEngine;
using UnityEngine.InputSystem;

public class GunSystem : MonoBehaviour
{
    [Header("References")]
    public Transform cam;
    public Transform attackPoint;

    [Header("Base Stats")]
    public float damage = 25f;
    public float range = 100f;
    public float fireRate = 0.5f;
    public float spread = 0f;
    public int bulletsPerShot = 1;

    [Header("Fire Mode")]
    public FireMode fireMode = FireMode.SemiAuto;

    [Header("Magazine (ignored in Charge mode)")]
    public int magazineSize = 6;
    public float reloadTime = 1f;
    private int bulletsLeft;
    private bool reloading = false;

    [Header("Charge Settings (only Charge mode)")]
    public float chargeTime = 1.5f;
    public float chargeMultiplier = 4f;
    public float minChargeToFire = 0.2f;
    private float currentCharge = 0f;
    private bool isCharging = false;

    [Header("Graphics")]
    public GameObject muzzleFlash;
    public GameObject hitEffect;
    public GameObject enemyHitEffect;

    [Header("Feel")]
    public GunRecoil gunRecoil;

    [Header("UI")]
    public CrosshairUI crosshairUI;

    // Input
    private bool holdingShoot;
    private bool readyToShoot = true;

    public enum FireMode
    {
        SemiAuto,
        Auto,
        Charge
    }

    void Awake()
    {
        bulletsLeft = magazineSize;
    }

    void Update()
    {
        if (reloading) return;

        if (fireMode == FireMode.Auto && holdingShoot && readyToShoot)
        {
            if (bulletsLeft > 0)
                Shoot(damage);
            else
                TryReload();
        }

        if (isCharging)
        {
            currentCharge += Time.deltaTime / chargeTime;
            currentCharge = Mathf.Clamp01(currentCharge);
        }
    }

    #region INPUT

    public void OnShoot(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            holdingShoot = true;
            OnShootPressed();
        }
        if (context.canceled)
        {
            holdingShoot = false;
            OnShootReleased();
        }
    }

    public void OnReload(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            TryReload();
        }
    }

    #endregion

    #region SHOOT LOGIC

    private void OnShootPressed()
    {
        if (reloading) return;

        switch (fireMode)
        {
            case FireMode.SemiAuto:
                if (readyToShoot && bulletsLeft > 0)
                    Shoot(damage);
                else if (bulletsLeft <= 0)
                    TryReload();
                break;

            case FireMode.Charge:
                if (readyToShoot)
                    StartCharge();
                break;
        }
    }

    private void OnShootReleased()
    {
        if (fireMode == FireMode.Charge && isCharging)
        {
            ReleaseCharge();
        }
    }

    #endregion

    #region DISPARO

    private void Shoot(float finalDamage)
    {
        readyToShoot = false;

        for (int i = 0; i < bulletsPerShot; i++)
        {
            ShootRay(finalDamage);
        }

        if (muzzleFlash != null)
        {
            GameObject flash = Instantiate(
                muzzleFlash,
                attackPoint.position,
                attackPoint.rotation
            );
            Destroy(flash, 0.1f);
        }

        // Recoil
        if (gunRecoil != null)
            gunRecoil.DoRecoil();

        // Crosshair feedback
        if (crosshairUI != null)
            crosshairUI.OnShoot();

        if (fireMode != FireMode.Charge)
        {
            bulletsLeft--;
        }

        Invoke(nameof(ResetShot), fireRate);
    }

    #endregion

    #region CHARGE

    private void StartCharge()
    {
        isCharging = true;
        currentCharge = 0f;
    }

    private void ReleaseCharge()
    {
        isCharging = false;

        if (currentCharge < minChargeToFire)
        {
            currentCharge = 0f;
            return;
        }

        float chargeDamage = damage * (1 + (currentCharge * (chargeMultiplier - 1)));
        Shoot(chargeDamage);
        currentCharge = 0f;
    }

    #endregion

    #region RECARGA

    private void TryReload()
    {
        if (fireMode == FireMode.Charge) return;
        if (reloading) return;
        if (bulletsLeft >= magazineSize) return;

        reloading = true;
        isCharging = false;
        currentCharge = 0f;

        // Spin
        if (gunRecoil != null)
            gunRecoil.DoReloadSpin(reloadTime);

        Invoke(nameof(ReloadFinished), reloadTime);
    }

    private void ReloadFinished()
    {
        bulletsLeft = magazineSize;
        reloading = false;
    }

    #endregion

    #region RAYCAST

    private void ShootRay(float finalDamage)
    {
        Vector3 direction = cam.forward;
        if (spread > 0)
        {
            float x = Random.Range(-spread, spread);
            float y = Random.Range(-spread, spread);
            direction += new Vector3(x, y, 0);
        }

        Ray ray = new Ray(cam.position, direction);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, range))
        {
            EnemyHealth enemy = hit.collider.GetComponent<EnemyHealth>();

            if (enemy != null)
            {
                enemy.TakeDamage(finalDamage);

                // Hitmarker
                if (crosshairUI != null)
                    crosshairUI.OnHit();

                if (enemyHitEffect != null)
                {
                    GameObject effect = Instantiate(
                        enemyHitEffect,
                        hit.point,
                        Quaternion.LookRotation(hit.normal)
                    );
                    Destroy(effect, 1f);
                }
            }
            else
            {
                if (hitEffect != null)
                {
                    GameObject effect = Instantiate(
                        hitEffect,
                        hit.point,
                        Quaternion.LookRotation(hit.normal)
                    );
                    Destroy(effect, 2f);
                }
            }
        }
    }

    private void ResetShot()
    {
        readyToShoot = true;
    }

    #endregion

    #region GETTERS PARA HUD

    public int GetBulletsLeft() { return bulletsLeft; }
    public int GetMagazineSize() { return magazineSize; }
    public bool IsReloading() { return reloading; }
    public float GetChargePercent() { return currentCharge; }
    public bool IsCharging() { return isCharging; }
    public FireMode GetFireMode() { return fireMode; }

    #endregion

    #region MUTACIONES

    public void MutateToShotgun()
    {
        fireMode = FireMode.SemiAuto;
        damage = 15f;
        spread = 0.1f;
        bulletsPerShot = 5;
        fireRate = 0.8f;
        magazineSize = 2;
        reloadTime = 1.5f;
        ForceReload();
    }

    public void MutateToSniper()
    {
        fireMode = FireMode.Charge;
        damage = 30f;
        spread = 0f;
        bulletsPerShot = 1;
        fireRate = 0.3f;
        chargeTime = 1.5f;
        chargeMultiplier = 4f;
    }

    public void ResetToRevolver()
    {
        fireMode = FireMode.SemiAuto;
        damage = 25f;
        spread = 0f;
        bulletsPerShot = 1;
        fireRate = 0.5f;
        magazineSize = 6;
        reloadTime = 1f;
        ForceReload();
    }

    private void ForceReload()
    {
        bulletsLeft = magazineSize;
        reloading = false;
    }

    #endregion
}