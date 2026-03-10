using UnityEngine;

public class ShopInteractable : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] private float activateRange = 6f;
    [SerializeField] private ShopUI shopUI;

    private RoomPiece parentRoom;
    private bool shopOpened = false;
    private Transform player;

    void Start()
    {
        // Buscar room
        parentRoom = GetComponentInParent<RoomPiece>();
        if (parentRoom == null)
        {
            RoomPiece[] rooms = FindObjectsByType<RoomPiece>(FindObjectsSortMode.None);
            foreach (var room in rooms)
            {
                if (room.GetPieceType() == RoomPiece.PieceType.Shop)
                {
                    parentRoom = room;
                    break;
                }
            }
        }

        if (ShopManager.Instance != null && parentRoom != null)
            ShopManager.Instance.SetCurrentShopRoom(parentRoom);

        // Buscar ShopUI si no asignada
        if (shopUI == null)
            shopUI = GetComponentInChildren<ShopUI>();

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;
    }

    void Update()
    {
        if (shopOpened) return;
        if (player == null) return;

        float dist = Vector3.Distance(transform.position, player.position);
        if (dist <= activateRange)
            OpenShop();
    }

    void OpenShop()
    {
        shopOpened = true;

        if (ShopManager.Instance == null)
        {
            Debug.LogError("[SHOP] No ShopManager!");
            return;
        }

        ShopItem[] offerings = ShopManager.Instance.GenerateOfferings();

        if (shopUI != null)
            shopUI.Open(offerings);

        Debug.Log("[SHOP] Arcade screen activated!");
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, activateRange);
    }
}