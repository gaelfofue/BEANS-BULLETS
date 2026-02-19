using UnityEngine;

public class GunRecoil : MonoBehaviour
{
    [Header("Recoil")]
    public float recoilUp = 0.05f;      // Ahora es posición, no rotación
    public float recoilBack = 0.02f;    // Retroceso hacia atrás
    public float recoilSnap = 20f;
    public float recoilReturn = 10f;

    [Header("Reload Spin")]
    private bool isSpinning = false;
    private float spinProgress = 0f;
    private float spinSpeed = 360f;

    // State
    private Vector3 currentPosRecoil;
    private Vector3 targetPosRecoil;

    // Guardar transform original
    private Vector3 originPos;
    private Quaternion originRot;

    void Start()
    {
        originPos = transform.localPosition;
        originRot = transform.localRotation;
    }

    void Update()
    {
        // Posición recoil vuelve a 0
        targetPosRecoil = Vector3.Lerp(
            targetPosRecoil,
            Vector3.zero,
            Time.deltaTime * recoilReturn
        );

        currentPosRecoil = Vector3.Lerp(
            currentPosRecoil,
            targetPosRecoil,
            Time.deltaTime * recoilSnap
        );

        // Spin reload
        if (isSpinning)
        {
            spinProgress += Time.deltaTime * spinSpeed;

            if (spinProgress >= 360f)
            {
                isSpinning = false;
                spinProgress = 0f;
                transform.localRotation = originRot;
                transform.localPosition = originPos + currentPosRecoil;
                return;
            }

            // Rotar sobre el eje FORWARD del arma (su propio eje)
            transform.localRotation = originRot * Quaternion.AngleAxis(spinProgress, Vector3.forward);
            transform.localPosition = originPos + currentPosRecoil;
        }
        else
        {
            // Sin spin, solo aplicar recoil de posición
            transform.localPosition = originPos + currentPosRecoil;
            transform.localRotation = originRot;
        }
    }

    public void DoRecoil()
    {
        // Recoil como MOVIMIENTO: sube y retrocede
        targetPosRecoil += new Vector3(0, recoilUp, -recoilBack);
    }

    public void DoReloadSpin(float reloadTime)
    {
        if (isSpinning) return;

        isSpinning = true;
        spinProgress = 0f;
        spinSpeed = 360f / reloadTime;
    }
}