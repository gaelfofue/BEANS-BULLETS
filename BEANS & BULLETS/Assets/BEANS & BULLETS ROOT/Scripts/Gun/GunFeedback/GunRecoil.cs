using UnityEngine;

public class GunRecoil : MonoBehaviour
{
    [Header("Recoil")]
    public float recoilKick = 5f;
    public float recoilSnap = 20f;
    public float recoilReturn = 10f;

    [Header("Reload Spin")]
    public float spinDegrees = 270f;
    public AnimationCurve spinCurve;
    public AnimationCurve positionCurve;

    [Header("Reload Motion")]
    public float spinTiltX = 8f;
    public float spinTiltY = 5f;
    public Vector3 spinPositionOffset = new Vector3(0f, -0.06f, 0.02f);
    public float overshootDegrees = 4f;
    public AnimationCurve overshootCurve;

    // State - Recoil
    private float currentRecoil;
    private float targetRecoil;

    // State - Reload
    private bool isReloading = false;
    private float reloadProgress = 0f;
    private float reloadDuration = 1f;

    // Original
    private Quaternion originRot;
    private Vector3 originPos;

    void Start()
    {
        originRot = transform.localRotation;
        originPos = transform.localPosition;

        // Spin curve: ease in-out con aceleración fuerte al medio
        if (spinCurve == null || spinCurve.keys.Length == 0)
        {
            spinCurve = new AnimationCurve(
                new Keyframe(0f, 0f, 0f, 0f),
                new Keyframe(0.12f, 0.02f, 0.5f, 0.5f),
                new Keyframe(0.55f, 0.85f, 2f, 2f),
                new Keyframe(0.8f, 1f, 0.3f, 0.3f),
                new Keyframe(1f, 1f, 0f, 0f)
            );
        }

        // Position curve: sube ligeramente al inicio, baja en arco, vuelve
        if (positionCurve == null || positionCurve.keys.Length == 0)
        {
            positionCurve = new AnimationCurve(
                new Keyframe(0f, 0f, 0f, 2f),
                new Keyframe(0.1f, 0.3f, 1f, 1f),
                new Keyframe(0.5f, 1f, 0f, 0f),
                new Keyframe(0.85f, 0.2f, -2f, -2f),
                new Keyframe(1f, 0f, -1f, 0f)
            );
        }

        // Overshoot: pequeño rebote al final
        if (overshootCurve == null || overshootCurve.keys.Length == 0)
        {
            overshootCurve = new AnimationCurve(
                new Keyframe(0f, 0f),
                new Keyframe(0.78f, 0f),
                new Keyframe(0.88f, 1f),
                new Keyframe(0.94f, -0.3f),
                new Keyframe(1f, 0f)
            );
        }
    }

    void Update()
    {
        // === RECOIL ===
        targetRecoil = Mathf.Lerp(targetRecoil, 0f, Time.deltaTime * recoilReturn);
        currentRecoil = Mathf.Lerp(currentRecoil, targetRecoil, Time.deltaTime * recoilSnap);

        Quaternion kickRot = Quaternion.AngleAxis(-currentRecoil, Vector3.right);

        // === RELOAD SPIN ===
        if (isReloading)
        {
            reloadProgress += Time.deltaTime / reloadDuration;

            if (reloadProgress >= 1f)
            {
                reloadProgress = 1f;
                isReloading = false;
            }

            float t = reloadProgress;

            // Spin principal en eje Z (roll del arma)
            float spinAmount = spinCurve.Evaluate(t) * spinDegrees;

            // Tilt secundarios para que no sea un giro plano
            float posAmount = positionCurve.Evaluate(t);
            float tiltX = Mathf.Sin(t * Mathf.PI) * spinTiltX;
            float tiltY = Mathf.Sin(t * Mathf.PI * 0.8f) * spinTiltY;

            // Overshoot al final
            float overshoot = overshootCurve.Evaluate(t) * overshootDegrees;

            // Combinar rotaciones
            Quaternion spinRot = Quaternion.Euler(
                spinAmount,
                tiltY,
                tiltX + overshoot
            );

            // Posición en arco
            Vector3 posOffset = spinPositionOffset * posAmount;

            transform.localRotation = originRot * kickRot * spinRot;
            transform.localPosition = originPos + posOffset;
        }
        else
        {
            // Sin recarga, solo recoil
            transform.localRotation = originRot * kickRot;

            // Volver a posición original
            transform.localPosition = Vector3.Lerp(
                transform.localPosition,
                originPos,
                Time.deltaTime * 15f
            );
        }
    }

    public void DoRecoil()
    {
        targetRecoil += recoilKick;
    }

    public void DoReloadSpin(float reloadTime)
    {
        if (isReloading) return;
        isReloading = true;
        reloadProgress = 0f;
        reloadDuration = reloadTime;
    }

    public bool IsReloading()
    {
        return isReloading;
    }
}