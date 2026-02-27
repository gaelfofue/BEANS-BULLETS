using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCamera : MonoBehaviour
{
    [Header("References")]
    public Transform orientation;
    public PlayerMovement playerMovement;

    [Header("Sensitivity")]
    [SerializeField] private float xSensitivity = 0.07f;
    [SerializeField] private float ySensitivity = 0.07f;

    [Header("Clamp")]
    [SerializeField] private float minX = -90f;
    [SerializeField] private float maxX = 90f;

    [Header("Head Bob")]
    public bool enableHeadBob = true;
    public float bobFrequency = 8f;
    public float bobAmplitude = 0.05f;
    private float bobTimer = 0f;
    private float defaultYPos;

    // Input
    private Vector2 lookInput;

    // Rotación acumulada
    private float xRotation = 0f;  // Vertical (arriba/abajo)
    private float yRotation = 0f;  // Horizontal (izq/der)

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        defaultYPos = transform.localPosition.y;

        // Inicializar con la rotación actual
        yRotation = orientation.eulerAngles.y;
    }

    #region INPUT EVENT

    public void OnLook(InputAction.CallbackContext context)
    {
        lookInput = context.ReadValue<Vector2>();
    }

    #endregion

    private void Update()
    {
        Look();
        if (enableHeadBob) HeadBob();
    }

    #region CAMERA ROTATION

    private void Look()
    {
        // Acumular rotación
        yRotation += lookInput.x * xSensitivity;
        xRotation -= lookInput.y * ySensitivity;

        // Clamp vertical
        xRotation = Mathf.Clamp(xRotation, minX, maxX);

        // Cámara: solo rotación vertical (la horizontal viene del CameraHolder)
        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        // Orientation: rotación horizontal para el movimiento
        orientation.localRotation = Quaternion.Euler(0f, yRotation, 0f);
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