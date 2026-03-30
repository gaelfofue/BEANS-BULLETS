using UnityEngine;
using System.Collections;

public class ShootVFXController : MonoBehaviour
{
    [Header("Muzzle Point")]
    [SerializeField] private Transform muzzlePoint;

    [Header("Muzzle Flash Star")]
    [SerializeField] private ParticleSystem muzzleFlashStar;

    [Header("Shock Ring")]
    [SerializeField] private ParticleSystem shockRing;
    [SerializeField] private float ringDelay = 0.02f;

    [Header("Sparks")]
    [SerializeField] private ParticleSystem muzzleSparks;

    [Header("Impact Prefab")]
    [SerializeField] private GameObject impactPrefab;
    [SerializeField] private float impactLifetime = 0.5f;

    /// <summary>
    /// Disparo normal: muzzle + trail + impacto.
    /// </summary>
    public void PlayShootVFX(Vector3 hitPoint, bool didHit, Vector3 hitNormal)
    {
        // Muzzle flash
        if (muzzleFlashStar != null)
        {
            var main = muzzleFlashStar.main;
            main.startRotation = Random.Range(0f, Mathf.PI * 2f);
            muzzleFlashStar.Play();
        }

        // Sparks
        if (muzzleSparks != null)
            muzzleSparks.Play();

        // Shock ring
        if (shockRing != null)
            StartCoroutine(PlayDelayed(shockRing, ringDelay));

        // Trail (sistema independiente)
        if (HitscanTrail.Instance != null)
        {
            Vector3 endPoint = didHit ? hitPoint : muzzlePoint.position + muzzlePoint.forward * 100f;
            HitscanTrail.Instance.ShowTrail(muzzlePoint.position, endPoint);
        }

        // Impact
        if (didHit && impactPrefab != null)
            SpawnImpact(hitPoint, hitNormal);
    }

    /// <summary>
    /// Shotgun: solo muzzle una vez.
    /// </summary>
    public void PlayMuzzleOnly()
    {
        if (muzzleFlashStar != null)
        {
            var main = muzzleFlashStar.main;
            main.startRotation = Random.Range(0f, Mathf.PI * 2f);
            muzzleFlashStar.Play();
        }

        if (muzzleSparks != null)
            muzzleSparks.Play();

        if (shockRing != null)
            StartCoroutine(PlayDelayed(shockRing, ringDelay));
    }

    /// <summary>
    /// Shotgun: trail + impacto por cada pellet.
    /// </summary>
    public void PlayTrailAndImpact(Vector3 hitPoint, bool didHit, Vector3 hitNormal)
    {
        if (didHit && HitscanTrail.Instance != null)
            HitscanTrail.Instance.ShowTrail(muzzlePoint.position, hitPoint);

        if (didHit && impactPrefab != null)
            SpawnImpact(hitPoint, hitNormal);
    }

    private IEnumerator PlayDelayed(ParticleSystem ps, float delay)
    {
        yield return new WaitForSeconds(delay);
        ps.Play();
    }

    private void SpawnImpact(Vector3 position, Vector3 normal)
    {
        Quaternion rotation = Quaternion.LookRotation(normal);
        GameObject impact = Instantiate(impactPrefab, position, rotation);
        Destroy(impact, impactLifetime);
    }
}