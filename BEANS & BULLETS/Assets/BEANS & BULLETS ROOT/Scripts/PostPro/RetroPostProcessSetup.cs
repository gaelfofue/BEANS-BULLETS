// RetroPostProcessSetup.cs
// Adjuntar a un GameObject vacío llamado "PostProcessManager"
// Requiere: URP con Post Processing activado en la URP Asset y en la Camera

using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[RequireComponent(typeof(Volume))]
public class RetroPostProcessSetup : MonoBehaviour
{
    [Header("CONFIGURACIÓN RETRO PS1")]
    [Header("Bloom (Glow Neón)")]
    [SerializeField] private float bloomIntensity = 1.5f;
    [SerializeField] private float bloomThreshold = 0.8f;
    [SerializeField] private Color bloomTint = new Color(0.8f, 1f, 0.9f, 1f);

    [Header("Vignette (Bordes Oscuros CRT)")]
    [SerializeField] private float vignetteIntensity = 0.45f;
    [SerializeField] private float vignetteSmoothness = 0.3f;
    [SerializeField] private Color vignetteColor = Color.black;

    [Header("Film Grain (Ruido Analógico)")]
    [SerializeField] private float grainIntensity = 0.6f;
    [SerializeField] private float grainResponse = 0.8f;

    [Header("Chromatic Aberration (Distorsión CRT)")]
    [SerializeField] private float chromaticIntensity = 0.15f;

    [Header("Color Adjustments (Tono Retro)")]
    [SerializeField] private float contrast = 20f;
    [SerializeField] private float saturation = -15f;
    [SerializeField] private Color colorFilter = new Color(0.9f, 1f, 0.95f, 1f);

    [Header("Lift Gamma Gain (Tinte Frío)")]
    [SerializeField] private Color shadowTint = new Color(0.05f, 0.08f, 0.12f, 0f);
    [SerializeField] private Color midtoneTint = new Color(0f, 0.02f, 0.05f, 0f);

    private Volume volume;

    private void Awake()
    {
        volume = GetComponent<Volume>();

        if (volume.profile == null)
        {
            volume.profile = ScriptableObject.CreateInstance<VolumeProfile>();
        }

        SetupBloom();
        SetupVignette();
        SetupFilmGrain();
        SetupChromaticAberration();
        SetupColorAdjustments();
        SetupLiftGammaGain();
    }

    private void SetupBloom()
    {
        if (!volume.profile.TryGet(out Bloom bloom))
        {
            bloom = volume.profile.Add<Bloom>(true);
        }

        bloom.active = true;
        bloom.intensity.overrideState = true;
        bloom.intensity.value = bloomIntensity;
        bloom.threshold.overrideState = true;
        bloom.threshold.value = bloomThreshold;
        bloom.tint.overrideState = true;
        bloom.tint.value = bloomTint;
        bloom.highQualityFiltering.overrideState = true;
        bloom.highQualityFiltering.value = false; // Intencionalmente bajo
    }

    private void SetupVignette()
    {
        if (!volume.profile.TryGet(out Vignette vignette))
        {
            vignette = volume.profile.Add<Vignette>(true);
        }

        vignette.active = true;
        vignette.intensity.overrideState = true;
        vignette.intensity.value = vignetteIntensity;
        vignette.smoothness.overrideState = true;
        vignette.smoothness.value = vignetteSmoothness;
        vignette.color.overrideState = true;
        vignette.color.value = vignetteColor;
    }

    private void SetupFilmGrain()
    {
        if (!volume.profile.TryGet(out FilmGrain grain))
        {
            grain = volume.profile.Add<FilmGrain>(true);
        }

        grain.active = true;
        grain.type.overrideState = true;
        grain.type.value = FilmGrainLookup.Medium2;
        grain.intensity.overrideState = true;
        grain.intensity.value = grainIntensity;
        grain.response.overrideState = true;
        grain.response.value = grainResponse;
    }

    private void SetupChromaticAberration()
    {
        if (!volume.profile.TryGet(out ChromaticAberration ca))
        {
            ca = volume.profile.Add<ChromaticAberration>(true);
        }

        ca.active = true;
        ca.intensity.overrideState = true;
        ca.intensity.value = chromaticIntensity;
    }

    private void SetupColorAdjustments()
    {
        if (!volume.profile.TryGet(out ColorAdjustments color))
        {
            color = volume.profile.Add<ColorAdjustments>(true);
        }

        color.active = true;
        color.contrast.overrideState = true;
        color.contrast.value = contrast;
        color.saturation.overrideState = true;
        color.saturation.value = saturation;
        color.colorFilter.overrideState = true;
        color.colorFilter.value = colorFilter;
    }

    private void SetupLiftGammaGain()
    {
        if (!volume.profile.TryGet(out LiftGammaGain lgg))
        {
            lgg = volume.profile.Add<LiftGammaGain>(true);
        }

        lgg.active = true;
        lgg.lift.overrideState = true;
        lgg.lift.value = new Vector4(shadowTint.r, shadowTint.g, shadowTint.b, shadowTint.a);
        lgg.gamma.overrideState = true;
        lgg.gamma.value = new Vector4(midtoneTint.r, midtoneTint.g, midtoneTint.b, midtoneTint.a);
    }
}