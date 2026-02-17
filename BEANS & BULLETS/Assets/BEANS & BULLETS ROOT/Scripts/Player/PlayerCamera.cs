using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCamera : MonoBehaviour
{
    [Header("References")]
    public Transform orientation;
    public PlayerMovement playerMovement; // Para saber si camina

    [Header("Sensitivity")]
    [Range(1f, 100f)]
    public float sensitivity = 50f;
    [Range(0.1f, 2f)]
    public float sensMultiplier = 1f;

    [Header("Clamp")]
    public float topClamp = 90f;
    public float bottomClamp = -90f;

    [Header("Head Bob")]
    public bool enableHeadBob = true;
    public float bobFrequency = 8f;
    public float bobAmplitude = 0.05f;
    private float bobTimer = 0f;
    private float defaultYPos;

    // Input
    private Vector2 lookInput;

    // Private
    private float xRotation;
    private float yRotation;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        defaultYPos = transform.localPosition.y;
    }

    void Update()
    {
        Look();
        if (enableHeadBob) HeadBob();
    }

    #region INPUT EVENT
    public void OnLook(InputAction.CallbackContext context)
    {
        lookInput = context.ReadValue<Vector2>();
    }
    #endregion

    #region CAMERA ROTATION
    private void Look()
    {
        float mouseX = lookInput.x * sensitivity * Time.deltaTime * sensMultiplier;
        float mouseY = lookInput.y * sensitivity * Time.deltaTime * sensMultiplier;

        yRotation += mouseX;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, bottomClamp, topClamp);

        // Rotar camara (arriba/abajo + izq/der)
        transform.localRotation = Quaternion.Euler(xRotation, yRotation, 0);

        // Rotar orientation (solo izq/der, para que 
        // el movimiento sepa hacia donde mira)
        orientation.localRotation = Quaternion.Euler(0, yRotation, 0);
    }
    #endregion

    #region HEAD BOB
    private void HeadBob()
    {
        if (!playerMovement.IsMoving())
        {
            bobTimer = 0;
            Vector3 pos = transform.localPosition;
            pos.y = Mathf.Lerp(pos.y, defaultYPos, Time.deltaTime * 8f);
            transform.localPosition = pos;
            return;
        }

        bobTimer += Time.deltaTime * bobFrequency;
        Vector3 newPos = transform.localPosition;
        newPos.y = defaultYPos + Mathf.Sin(bobTimer) * bobAmplitude;
        transform.localPosition = newPos;
    }
    #endregion
}