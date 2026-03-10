using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopSlotUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI descText;
    [SerializeField] private Image typeBadge;
    [SerializeField] private Image highlight;

    [Header("Type Colors")]
    [SerializeField] private Color fireColor = new Color(1f, 0.4f, 0.2f);
    [SerializeField] private Color bulletColor = new Color(0.2f, 0.8f, 1f);
    [SerializeField] private Color modifierColor = new Color(0.4f, 1f, 0.4f);

    [Header("Highlight Colors")]
    [SerializeField] private Color normalColor = new Color(0.1f, 0.1f, 0.2f, 0.8f);
    [SerializeField] private Color hoverColor = new Color(0.2f, 0.2f, 0.4f, 0.9f);
    [SerializeField] private Color disabledColor = new Color(0.05f, 0.05f, 0.1f, 0.5f);

    private int slotIndex;
    private bool interactable = true;

    public void Setup(ShopItem item, int index)
    {
        slotIndex = index;
        interactable = true;

        if (nameText != null)
            nameText.text = item.itemName;

        if (descText != null)
            descText.text = item.description;

        if (iconImage != null)
        {
            iconImage.sprite = item.icon;
            iconImage.color = item.icon != null ? Color.white : new Color(1, 1, 1, 0.1f);
        }

        if (typeBadge != null)
            typeBadge.color = GetTypeColor(item.itemType);

        if (highlight != null)
            highlight.color = normalColor;
    }

    /// <summary>
    /// Llamado cuando el crosshair del player apunta a este slot.
    /// </summary>
    public void OnLookedAt()
    {
        if (!interactable) return;

        if (highlight != null)
            highlight.color = hoverColor;
    }

    /// <summary>
    /// Llamado cuando el crosshair deja de apuntar a este slot.
    /// </summary>
    public void OnLookAway()
    {
        if (!interactable) return;

        if (highlight != null)
            highlight.color = normalColor;
    }

    /// <summary>
    /// Llamado cuando el player hace click/dispara apuntando a este slot.
    /// </summary>
    public void OnSelected()
    {
        if (!interactable) return;

        if (ShopManager.Instance != null)
            ShopManager.Instance.TryPurchase(slotIndex);
    }

    public void SetInteractable(bool value)
    {
        interactable = value;

        if (!value && highlight != null)
            highlight.color = disabledColor;
    }

    private Color GetTypeColor(ShopItemType type)
    {
        switch (type)
        {
            case ShopItemType.FireMode: return fireColor;
            case ShopItemType.BulletType: return bulletColor;
            case ShopItemType.PlayerModifier: return modifierColor;
            default: return Color.white;
        }
    }
}