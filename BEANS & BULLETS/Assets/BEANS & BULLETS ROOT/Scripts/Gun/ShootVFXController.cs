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

    [Header("Hitscan Trail")]
    [SerializeField] private LineRenderer trailLine;
    [SerializeField] private float trailDuration = 0.08f;
    [SerializeField] private float trailWidth = 0.02f;

    [Header("Impact Prefab")]
    [SerializeField] private GameObject impactPrefab;
    [SerializeField] private float impactLifetime = 0.5f;

    void Start()
    {
        // Configurar trail
        if (trailLine != null)
        {
            trailLine.positionCount = 2;
            trailLine.startWidth = trailWidth;
            trailLine.endWidth = trailWidth * 0.5f;
            trailLine.enabled = false;
        }
    }

    /// <summary>
    /// Llamar desde GunSystem después de cada disparo.
    /// </summary>
    public void PlayShootVFX(Vector3 hitPoint, bool didHit, Vector3 hitNormal)
    {
        // 1. Muzzle Flash Star (rotación random)
        if (muzzleFlashStar != null)
        {
            var main = muzzleFlashStar.main;
            main.startRotation = Random.Range(0f, Mathf.PI * 2f);
            muzzleFlashStar.Play();
        }

        // 2. Sparks
        if (muzzleSparks != null)
            muzzleSparks.Play();

        // 3. Shock Ring (con delay)
        if (shockRing != null)
            StartCoroutine(PlayDelayed(shockRing, ringDelay));

        // 4. Trail
        if (trailLine != null)
        {
            Vector3 endPoint = didHit ? hitPoint : muzzlePoint.position + muzzlePoint.forward * 100f;
            StartCoroutine(ShowTrail(muzzlePoint.position, endPoint));
        }

        // 5. Impact VFX
        if (didHit && impactPrefab != null)
        {
            SpawnImpact(hitPoint, hitNormal);
        }
    }

    /// <summary>
    /// Versión para shotgun: múltiples impactos, un solo muzzle.
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
    /// Para cada pellet de shotgun que impacta.
    /// </summary>
    public void PlayTrailAndImpact(Vector3 hitPoint, bool didHit, Vector3 hitNormal)
    {
        if (trailLine != null && didHit)
        {
            // Para shotgun usamos DrawLine ya que solo hay un LineRenderer
            Debug.DrawLine(muzzlePoint.position, hitPoint, Color.yellow, 0.08f);
        }

        if (didHit && impactPrefab != null)
            SpawnImpact(hitPoint, hitNormal);
    }

    private IEnumerator PlayDelayed(ParticleSystem ps, float delay)
    {
        yield return new WaitForSeconds(delay);
        ps.Play();
    }

    private IEnumerator ShowTrail(Vector3 start, Vector3 end)
    {
        trailLine.enabled = true;
        trailLine.SetPosition(0, start);
        trailLine.SetPosition(1, end);

        yield return new WaitForSeconds(trailDuration);

        trailLine.enabled = false;
    }

    private void SpawnImpact(Vector3 position, Vector3 normal)
    {
        Quaternion rotation = Quaternion.LookRotation(normal);
        GameObject impact = Instantiate(impactPrefab, position, rotation);
        Destroy(impact, impactLifetime);
    }
}