using UnityEngine;

public enum ShopItemType
{
    FireMode,
    BulletType,
    PlayerModifier
}

[CreateAssetMenu(menuName = "BeanBullets/Shop/ShopItem")]
public class ShopItem : ScriptableObject
{
    [Header("Display Info")]
    public string itemName;
    [TextArea(2, 4)]
    public string description;
    public Sprite icon;
    public Color rarityColor = Color.white;

    [Header("Type")]
    public ShopItemType itemType;

    [Header("Actual Upgrade (assign ONE)")]
    public FireMode fireMode;
    public BulletType bulletType;
    // public PlayerModifier playerModifier; // Futuro

    [Header("Discovery (futuro)")]
    public bool discovered = true; // Para sistema Balatro futuro
}