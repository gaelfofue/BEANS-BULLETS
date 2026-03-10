using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopUI : MonoBehaviour
{
    [Header("Slots")]
    [SerializeField] private ShopSlotUI[] slots;

    [Header("Screen Feedback")]
    [SerializeField] private Image screenBackground;
    [SerializeField] private Color activeColor = new Color(0.05f, 0.05f, 0.15f);
    [SerializeField] private Color purchasedColor = new Color(0.02f, 0.02f, 0.05f);

    private bool purchased = false;

    void Start()
    {
        // Escuchar compras
        if (ShopManager.Instance != null)
            ShopManager.Instance.OnPurchaseComplete += OnPurchased;

        // Apagar pantalla al inicio
        SetScreenActive(false);
    }

    void OnDestroy()
    {
        if (ShopManager.Instance != null)
            ShopManager.Instance.OnPurchaseComplete -= OnPurchased;
    }

    public void Open(ShopItem[] offerings)
    {
        purchased = false;
        SetScreenActive(true);

        if (screenBackground != null)
            screenBackground.color = activeColor;

        for (int i = 0; i < slots.Length; i++)
        {
            if (i < offerings.Length && offerings[i] != null)
            {
                slots[i].Setup(offerings[i], i);
                slots[i].gameObject.SetActive(true);
            }
            else
            {
                slots[i].gameObject.SetActive(false);
            }
        }
    }

    private void OnPurchased()
    {
        purchased = true;

        // Desactivar todos los slots
        foreach (var slot in slots)
            slot.SetInteractable(false);

        // Cambiar color de pantalla
        if (screenBackground != null)
            screenBackground.color = purchasedColor;
    }

    private void SetScreenActive(bool active)
    {
        foreach (var slot in slots)
            slot.gameObject.SetActive(active);
    }

    public bool HasPurchased()
    {
        return purchased;
    }
}