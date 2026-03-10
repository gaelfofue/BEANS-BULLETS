using UnityEngine;
using System.Collections.Generic;

public class ShopManager : MonoBehaviour
{
    public static ShopManager Instance;

    [Header("Item Pool")]
    [SerializeField] private ShopItem[] allItems;

    [Header("Config")]
    [SerializeField] private int itemsToShow = 3;

    // Estado
    private ShopItem[] currentOfferings;
    private bool hasPurchased = false;
    private RoomPiece currentShopRoom;

    // Evento para que la UI sepa que hubo compra
    public System.Action OnPurchaseComplete;

    void Awake()
    {
        Instance = this;
    }

    /// <summary>
    /// Llamado cuando el player interactúa con el arcade.
    /// Genera las ofertas aleatorias.
    /// </summary>
    public ShopItem[] GenerateOfferings()
    {
        hasPurchased = false;
        currentOfferings = new ShopItem[itemsToShow];

        // Pool temporal para no repetir
        List<ShopItem> pool = new List<ShopItem>(allItems);

        for (int i = 0; i < itemsToShow; i++)
        {
            if (pool.Count == 0) break;

            int index = Random.Range(0, pool.Count);
            currentOfferings[i] = pool[index];
            pool.RemoveAt(index);
        }

        Debug.Log($"[SHOP] Generated {currentOfferings.Length} offerings");
        return currentOfferings;
    }

    /// <summary>
    /// Llamado cuando el player hace click en una mejora.
    /// </summary>
    public bool TryPurchase(int slotIndex)
    {
        if (hasPurchased) return false;
        if (slotIndex < 0 || slotIndex >= currentOfferings.Length) return false;
        if (currentOfferings[slotIndex] == null) return false;

        ShopItem item = currentOfferings[slotIndex];
        hasPurchased = true;

        // Aplicar la mejora
        ApplyItem(item);

        Debug.Log($"[SHOP] Purchased: {item.itemName} ({item.itemType})");

        // Notificar
        OnPurchaseComplete?.Invoke();

        // Desbloquear puerta
        if (currentShopRoom != null)
            currentShopRoom.OnShopInteractionComplete();

        return true;
    }

    private void ApplyItem(ShopItem item)
    {
        // Buscar el GunSystem del player
        GunSystem gun = FindFirstObjectByType<GunSystem>();
        if (gun == null)
        {
            Debug.LogError("[SHOP] No GunSystem found!");
            return;
        }

        switch (item.itemType)
        {
            case ShopItemType.FireMode:
                if (item.fireMode != null)
                    gun.SetFireMode(item.fireMode);
                break;

            case ShopItemType.BulletType:
                if (item.bulletType != null)
                    gun.SetBulletType(item.bulletType);
                break;

            case ShopItemType.PlayerModifier:
                // Futuro: gun.AddModifier(item.playerModifier);
                Debug.Log("[SHOP] PlayerModifier not implemented yet");
                break;
        }
    }

    public void SetCurrentShopRoom(RoomPiece room)
    {
        currentShopRoom = room;
    }

    public bool HasPurchased()
    {
        return hasPurchased;
    }
}