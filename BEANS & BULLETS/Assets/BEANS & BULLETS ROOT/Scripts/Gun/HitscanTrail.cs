using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class HitscanTrail : MonoBehaviour
{
    public static HitscanTrail Instance;

    [Header("Trail Config")]
    [SerializeField] private Material trailMaterial;
    [SerializeField] private float trailDuration = 0.08f;
    [SerializeField] private float trailStartWidth = 0.025f;
    [SerializeField] private float trailEndWidth = 0.008f;
    [SerializeField] private Color trailColor = new Color(1f, 0.9f, 0.5f, 0.8f);

    // Pool de LineRenderers para no crear/destruir constantemente
    private List<LineRenderer> pool = new List<LineRenderer>();

    void Awake()
    {
        Instance = this;
    }

    /// <summary>
    /// Llamar desde ShootVFXController para mostrar un trail.
    /// </summary>
    public void ShowTrail(Vector3 start, Vector3 end)
    {
        StartCoroutine(TrailRoutine(start, end));
    }

    private IEnumerator TrailRoutine(Vector3 start, Vector3 end)
    {
        // Obtener o crear un LineRenderer
        LineRenderer line = GetLineFromPool();

        // Configurar
        line.enabled = true;
        line.useWorldSpace = true;
        line.positionCount = 2;
        line.SetPosition(0, start);
        line.SetPosition(1, end);

        line.startWidth = trailStartWidth;
        line.endWidth = trailEndWidth;

        line.startColor = trailColor;
        line.endColor = new Color(trailColor.r, trailColor.g, trailColor.b, 0.2f);

        // Fade out
        float timer = trailDuration;
        Color startCol = trailColor;

        while (timer > 0f)
        {
            timer -= Time.deltaTime;
            float alpha = timer / trailDuration;

            Color c1 = startCol;
            c1.a = alpha * startCol.a;
            Color c2 = c1;
            c2.a = alpha * 0.2f;

            line.startColor = c1;
            line.endColor = c2;
            line.startWidth = trailStartWidth * alpha;
            line.endWidth = trailEndWidth * alpha;

            yield return null;
        }

        // Devolver al pool
        line.enabled = false;
    }

    private LineRenderer GetLineFromPool()
    {
        // Buscar uno libre
        foreach (var line in pool)
        {
            if (!line.enabled)
                return line;
        }

        // Crear nuevo
        GameObject go = new GameObject("TrailLine");
        go.transform.parent = transform;

        LineRenderer lr = go.AddComponent<LineRenderer>();
        lr.positionCount = 2;
        lr.useWorldSpace = true;
        lr.enabled = false;
        lr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        lr.receiveShadows = false;
        lr.allowOcclusionWhenDynamic = false;

        // Material
        if (trailMaterial != null)
        {
            lr.material = trailMaterial;
        }
        else
        {
            // Crear material básico additive
            lr.material = new Material(Shader.Find("Sprites/Default"));
        }

        pool.Add(lr);
        return lr;
    }
}