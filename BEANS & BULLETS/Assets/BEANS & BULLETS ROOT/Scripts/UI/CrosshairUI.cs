using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class CrosshairUI : MonoBehaviour
{
    [Header("Crosshair")]
    public Image crosshair;
    public float normalSize = 20f;
    public float shootSize = 30f;
    public float shrinkSpeed = 8f;

    [Header("Hitmarker")]
    public Image hitmarker;
    public float hitmarkerDuration = 0.15f;
    private float hitmarkerTimer = 0f;

    // Private
    private RectTransform crosshairRect;

    void Start()
    {
        crosshairRect = crosshair.GetComponent<RectTransform>();
        crosshairRect.sizeDelta = new Vector2(normalSize, normalSize);

        if (hitmarker != null)
            hitmarker.enabled = false;
    }

    void Update()
    {
        // Crosshair vuelve a tamaño normal
        Vector2 current = crosshairRect.sizeDelta;
        Vector2 target = new Vector2(normalSize, normalSize);
        crosshairRect.sizeDelta = Vector2.Lerp(
            current, target, Time.deltaTime * shrinkSpeed
        );

        // Hitmarker timer
        if (hitmarkerTimer > 0)
        {
            hitmarkerTimer -= Time.deltaTime;
            if (hitmarkerTimer <= 0 && hitmarker != null)
            {
                hitmarker.enabled = false;
            }
        }
    }

    public void OnShoot()
    {
        crosshairRect.sizeDelta = new Vector2(shootSize, shootSize);
    }

    public void OnHit()
    {
        // Activar hitmarker
        if (hitmarker != null)
        {
            hitmarker.enabled = true;
            hitmarkerTimer = hitmarkerDuration;
        }

        // Escala la crosshair brevemente
        StartCoroutine(HitPulse());
    }

    IEnumerator HitPulse()
    {
        Vector3 original = transform.localScale;
        transform.localScale = original * 1.3f;
        yield return new WaitForSeconds(0.05f);
        transform.localScale = original;
    }
}