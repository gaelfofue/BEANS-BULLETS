using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopUI : MonoBehaviour
{
    [Header("Panel")]
    [SerializeField] private GameObject shopPanel;

    [Header("Slot 1")]
    [SerializeField] private Button slot1Button;
    [SerializeField] private Image slot1Icon;
    [SerializeField] private TextMeshProUGUI slot1Name;
    [SerializeField] private TextMeshProUGUI slot1Desc;
    [SerializeField] private Image slot1TypeBadge;

    [Header("Slot 2")]
    [SerializeField] private Button slot2Button;
    [SerializeField] private Image slot2Icon;
    [SerializeField] private TextMeshProUGUI slot2Name;
    [SerializeField] private TextMeshProUGUI slot2Desc;
    [SerializeField] private Image slot2TypeBadge;

    [Header("Slot 3")]
    [SerializeField] private Button slot3Button;
    [SerializeField] private Image slot3Icon;
    [SerializeField] private TextMeshProUGUI slot3Name;
    [SerializeField] private TextMeshProUGUI slot3Desc;
    [SerializeField] private Image slot3TypeBadge;

    [Header("Type Colors")]
    [SerializeField] private Color fireColor = new Color(1f, 0.4f, 0.2f);
    [SerializeField] private Color bulletColor = new Color(0.2f, 0.8f, 1f);
    [SerializeField] private Color modifierColor = new Color(0.4f, 1f, 0.4f);

    // Cache
    private Button[] slotButtons;
    private Image[] slotIcons;
    private TextMeshProUGUI[] slotNames;
    private TextMeshProUGUI[] slotDescs;
    private Image[] slotBadges;

    void Awake()
    {
        slotButtons = new Button[] { slot1Button, slot2Button, slot3Button };
        slotIcons = new Image[] { slot1Icon, slot2Icon, slot3Icon };
        slotNames = new TextMeshProUGUI[] { slot1Name, slot2Name, slot3Name };
        slotDescs = new TextMeshProUGUI[] { slot1Desc, slot2Desc, slot3Desc };
        slotBadges = new Image[] { slot1TypeBadge, slot2TypeBadge, slot3TypeBadge };

        // Conectar botones
        slot1Button.onClick.AddListener(() => OnSlotClicked(0));
        slot2Button.onClick.AddListener(() => OnSlotClicked(1));
        slot3Button.onClick.AddListener(() => OnSlotClicked(2));

        // Ocultar al inicio
        shopPanel.SetActive(false);
    }

    void Start()
    {
        // Escuchar evento de compra
        if (ShopManager.Instance != null)
            ShopManager.Instance.OnPurchaseComplete += OnPurchaseComplete;
    }

    void OnDestroy()
    {
        if (ShopManager.Instance != null)
            ShopManager.Instance.OnPurchaseComplete -= OnPurchaseComplete;
    }

    /// <summary>
    /// Abre la tienda y muestra las mejoras.
    /// </summary>
    public void Open(ShopItem[] offerings)
    {
        shopPanel.SetActive(true);

        // Mostrar cursor para hacer click
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        for (int i = 0; i < slotButtons.Length; i++)
        {
            if (i < offerings.Length && offerings[i] != null)
            {
                ShopItem item = offerings[i];

                slotButtons[i].gameObject.SetActive(true);
                slotButtons[i].interactable = true;

                // Icono
                if (slotIcons[i] != null)
                {
                    slotIcons[i].sprite = item.icon;
                    slotIcons[i].color = item.icon != null ? Color.white : new Color(1, 1, 1, 0.2f);
                }

                // Nombre
                if (slotNames[i] != null)
                    slotNames[i].text = item.itemName;

                // Descripción
                if (slotDescs[i] != null)
                    slotDescs[i].text = item.description;

                // Color del tipo
                if (slotBadges[i] != null)
                    slotBadges[i].color = GetTypeColor(item.itemType);
            }
            else
            {
                slotButtons[i].gameObject.SetActive(false);
            }
        }
    }

    public void Close()
    {
        shopPanel.SetActive(false);

        // Volver a lockear cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void OnSlotClicked(int index)
    {
        if (ShopManager.Instance == null) return;

        bool success = ShopManager.Instance.TryPurchase(index);

        if (success)
        {
            // Feedback visual: desactivar todos los botones
            foreach (var btn in slotButtons)
                btn.interactable = false;
        }
    }

    private void OnPurchaseComplete()
    {
        // Cerrar la UI después de un momento
        Invoke(nameof(Close), 0.5f);
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