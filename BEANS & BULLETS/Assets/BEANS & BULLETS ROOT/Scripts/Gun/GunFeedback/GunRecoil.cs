using UnityEngine;

public class GunRecoil : MonoBehaviour
{
    [Header("Recoil")]
    public float recoilKick = 5f;
    public float recoilSnap = 20f;
    public float recoilReturn = 10f;

    [Header("Reload Spin")]
    public float spinSpeedMultiplier = 2f;  // 1 = una vuelta en el tiempo de recarga
                                            // 2 = el doble de rápido
                                            // 3 = el triple, etc.
    private bool isSpinning = false;
    private float spinProgress = 0f;
    private float spinSpeed = 900f;

    // State
    private float currentRecoil;
    private float targetRecoil;

    // Original
    private Quaternion originRot;

    void Start()
    {
        originRot = transform.localRotation;
    }

    void Update()
    {
        targetRecoil = Mathf.Lerp(
            targetRecoil,
            0f,
            Time.deltaTime * recoilReturn
        );

        currentRecoil = Mathf.Lerp(
            currentRecoil,
            targetRecoil,
            Time.deltaTime * recoilSnap
        );

        if (isSpinning)
        {
            spinProgress += Time.deltaTime * spinSpeed;

            if (spinProgress >= 900f)
            {
                isSpinning = false;
                spinProgress = 0f;
            }
        }

        Quaternion kickRot = Quaternion.AngleAxis(-currentRecoil, Vector3.right);

        if (isSpinning)
        {
            Quaternion spinRot = Quaternion.AngleAxis(spinProgress, Vector3.left);
            transform.localRotation = originRot * kickRot * spinRot;
        }
        else
        {
            transform.localRotation = originRot * kickRot;
        }
    }

    public void DoRecoil()
    {
        targetRecoil += recoilKick;
    }

    public void DoReloadSpin(float reloadTime)
    {
        if (isSpinning) return;
        isSpinning = true;
        spinProgress = 0f;
        spinSpeed = (900f / reloadTime) * spinSpeedMultiplier;
    }
}