using UnityEngine;

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
        CheckClick(); // 🆕
    }

    void CheckLookAt()
    {
        Ray ray = new Ray(cam.transform.position, cam.transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, interactRange, shopScreenLayer))
        {
            ShopSlotUI slot = hit.collider.GetComponent<ShopSlotUI>();
            if (slot == null)
                slot = hit.collider.GetComponentInParent<ShopSlotUI>();

            if (slot != null)
            {
                if (slot != currentSlot)
                {
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

    // 🆕 DETECCIÓN DIRECTA DE CLICKS
    void CheckClick()
    {
        // Detectar click izquierdo O disparo (Fire1)
        if (Input.GetMouseButtonDown(0) || Input.GetButtonDown("Fire1"))
        {
            if (currentSlot != null)
            {
                currentSlot.OnSelected();
                Debug.Log($"[SHOP] Selected slot: {currentSlot.name}");
            }
        }
    }
}