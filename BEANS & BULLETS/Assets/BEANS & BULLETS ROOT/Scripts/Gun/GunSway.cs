using UnityEngine;
using UnityEngine.InputSystem;

public class GunSway : MonoBehaviour
{
    [Header("GoldenEye Sway")]
    [SerializeField] private float swayAmount = 0.003f;
    [SerializeField] private float maxSway = 0.06f;
    [SerializeField] private float smoothSpeed = 3f;
    [SerializeField] private float returnSpeed = 2f;

    // Input
    private Vector2 lookInput;

    // State
    private Vector3 originPos;
    private Vector3 currentSway;

    private void Start()
    {
        originPos = transform.localPosition;
    }

    public void OnLook(InputAction.CallbackContext context)
    {
        lookInput = context.ReadValue<Vector2>();
    }

    private void LateUpdate()
    {
        // Desplazar en dirección OPUESTA al ratón (se queda atrás)
        float targetX = -lookInput.x * swayAmount;
        float targetY = -lookInput.y * swayAmount;

        // Clamp para que no se salga de pantalla
        targetX = Mathf.Clamp(targetX, -maxSway, maxSway);
        targetY = Mathf.Clamp(targetY, -maxSway, maxSway);

        Vector3 targetSway = new Vector3(targetX, targetY, 0f);

        // Movimiento lento hacia el target (inercia pesada)
        currentSway = Vector3.Lerp(
            currentSway,
            targetSway,
            Time.deltaTime * smoothSpeed
        );

        // Cuando no hay input, volver al centro MUY lentamente
        if (lookInput.sqrMagnitude < 0.01f)
        {
            currentSway = Vector3.Lerp(
                currentSway,
                Vector3.zero,
                Time.deltaTime * returnSpeed
            );
        }

        // Aplicar
        transform.localPosition = originPos + currentSway;
    }
}