using UnityEngine;
using UnityEngine.InputSystem;

public class GunSway : MonoBehaviour
{
    [Header("Sway Settings")]
    public float swayAmount = 0.002f;
    public float maxSway = 0.04f;
    public float smoothSpeed = 4f;
    public float returnSpeed = 6f;

    // Input
    private Vector2 lookInput;

    // State
    private Vector3 originPos;
    private Vector3 currentSway;

    void Start()
    {
        originPos = transform.localPosition;
    }

    public void OnLook(InputAction.CallbackContext context)
    {
        lookInput = context.ReadValue<Vector2>();
    }

    void Update()
    {
        // El arma se mueve en dirección OPUESTA 
        // al movimiento del ratón (se queda atrás)
        float swayX = -lookInput.x * swayAmount;
        float swayY = -lookInput.y * swayAmount;

        swayX = Mathf.Clamp(swayX, -maxSway, maxSway);
        swayY = Mathf.Clamp(swayY, -maxSway, maxSway);

        Vector3 targetSway = new Vector3(swayX, swayY, 0);

        // Suavizar hacia el target
        currentSway = Vector3.Lerp(
            currentSway,
            targetSway,
            Time.deltaTime * smoothSpeed
        );

        // Cuando no hay input, volver al centro
        if (lookInput.magnitude < 0.01f)
        {
            currentSway = Vector3.Lerp(
                currentSway,
                Vector3.zero,
                Time.deltaTime * returnSpeed
            );
        }

        // Aplicar sobre posición original
        transform.localPosition = originPos + currentSway;
    }
}