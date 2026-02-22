// RetroLightingSetup.cs
// Adjuntar a un GO vacío. Ejecutar desde Context Menu para configurar la escena.

using UnityEngine;

public class RetroLightingSetup : MonoBehaviour
{
    [Header("ILUMINACIÓN RETRO")]
    [SerializeField] private Color ambientColor = new Color(0.03f, 0.05f, 0.08f);

    [Header("Paleta de Luces (Neón)")]
    [SerializeField]
    private Color[] lightPalette = new Color[]
    {
        new Color(0.2f, 1f, 0.6f),    // Verde terminal
        new Color(1f, 0.3f, 0.2f),     // Rojo alarma
        new Color(0.3f, 0.5f, 1f),     // Azul frío
        new Color(1f, 0.8f, 0.2f),     // Amarillo advertencia
        new Color(0.8f, 0.2f, 1f),     // Púrpura
    };

    [ContextMenu("Apply Retro Ambient")]
    public void ApplyAmbient()
    {
        // Eliminar luz direccional
        Light[] allLights = FindObjectsOfType<Light>();
        foreach (Light l in allLights)
        {
            if (l.type == LightType.Directional)
            {
                l.gameObject.SetActive(false);
                Debug.Log($"Desactivada luz direccional: {l.name}");
            }
        }

        // Configurar ambiente
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = ambientColor;
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.Exponential;
        RenderSettings.fogColor = new Color(0.02f, 0.02f, 0.05f);
        RenderSettings.fogDensity = 0.04f;

        Debug.Log("Iluminación retro aplicada. Skybox = negro, fog = oscuro.");
    }

    [ContextMenu("Spawn Test Lights")]
    public void SpawnTestLights()
    {
        GameObject parent = new GameObject("RETRO LIGHTS");

        for (int i = 0; i < lightPalette.Length; i++)
        {
            GameObject lightGO = new GameObject($"RetroLight_{i}");
            lightGO.transform.parent = parent.transform;
            lightGO.transform.position = new Vector3(i * 5f, 3f, 0f);

            Light light = lightGO.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = lightPalette[i];
            light.intensity = 3f;
            light.range = 12f;
            light.shadows = LightShadows.Hard; // Sombras duras = retro

            Debug.Log($"Luz creada: {lightGO.name} - Color: {lightPalette[i]}");
        }
    }
}