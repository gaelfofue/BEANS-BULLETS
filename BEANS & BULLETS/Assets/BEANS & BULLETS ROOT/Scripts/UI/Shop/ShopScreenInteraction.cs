using UnityEngine;
using UnityEngine.InputSystem;

public class ShopScreenInteraction : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] private float interactRange = 5f;
    [SerializeField] private LayerMask shopScreenLayer;

    private Camera cam;
    private ShopSlotUI currentSlot;

    void Start()
    {
        cam = Camera.main;
    }

    void Update()
    {
        CheckLookAt();
    }

    void CheckLookAt()
    {
        Ray ray = new Ray(cam.transform.position, cam.transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, interactRange, shopScreenLayer))
        {
            // ¿Estamos mirando un slot?
            ShopSlotUI slot = hit.collider.GetComponent<ShopSlotUI>();
            if (slot == null)
                slot = hit.collider.GetComponentInParent<ShopSlotUI>();

            if (slot != null)
            {
                // Nuevo slot
                if (slot != currentSlot)
                {
                    // Dejar de mirar el anterior
                    if (currentSlot != null)
                        currentSlot.OnLookAway();

                    currentSlot = slot;
                    currentSlot.OnLookedAt();
                }
            }
            else
            {
                ClearCurrentSlot();
            }
        }
        else
        {
            ClearCurrentSlot();
        }
    }

    void ClearCurrentSlot()
    {
        if (currentSlot != null)
        {
            currentSlot.OnLookAway();
            currentSlot = null;
        }
    }

    /// <summary>
    /// Conectar al Input System: evento de click/disparo en tienda.
    /// </summary>
    public void OnShopSelect(InputAction.CallbackContext context)
    {
        if (!context.performed) return;

        if (currentSlot != null)
        {
            currentSlot.OnSelected();
            Debug.Log($"[SHOP] Selected slot!");
        }
    }
}