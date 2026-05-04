using UnityEngine;

[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(AudioLowPassFilter))]
public class DynamicMusicController : MonoBehaviour
{
    [Header("Low Pass Filter")]
    [Range(1000f, 22000f)] public float maxCutoff = 22000f;
    [Range(200f, 5000f)] public float minCutoff = 800f;
    public float smoothSpeed = 2f;

    private AudioLowPassFilter lowPass;

    void Start()
    {
        lowPass = GetComponent<AudioLowPassFilter>();
        lowPass.cutoffFrequency = maxCutoff;

        // Se suscribe al evento del timer, sin necesidad de buscar referencia
        if (GameTimer.Instance != null)
            GameTimer.Instance.onTimeChanged.AddListener(OnTimeChanged);
    }

    void OnDestroy()
    {
        if (GameTimer.Instance != null)
            GameTimer.Instance.onTimeChanged.RemoveListener(OnTimeChanged);
    }

    void OnTimeChanged(float ratio)
    {
        float targetCutoff = Mathf.Lerp(minCutoff, maxCutoff, ratio);
        StopAllCoroutines();
        StartCoroutine(SmoothCutoff(targetCutoff));
    }

    System.Collections.IEnumerator SmoothCutoff(float target)
    {
        while (Mathf.Abs(lowPass.cutoffFrequency - target) > 1f)
        {
            lowPass.cutoffFrequency = Mathf.Lerp(lowPass.cutoffFrequency, target, Time.deltaTime * smoothSpeed);
            yield return null;
        }
        lowPass.cutoffFrequency = target;
    }
}