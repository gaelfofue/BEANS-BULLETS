using UnityEngine;

public class ShopInteractable : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] private float interactRange = 4f;
    [SerializeField] private ShopUI shopUI;

    private RoomPiece parentRoom;
    private bool shopOpened = false;
    private Transform player;

    void Start()
    {
        // Buscar room padre
        parentRoom = GetComponentInParent<RoomPiece>();
        if (parentRoom == null)
        {
            // Buscar en toda la escena
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

        // Registrar room en el ShopManager
        if (ShopManager.Instance != null && parentRoom != null)
            ShopManager.Instance.SetCurrentShopRoom(parentRoom);

        // Buscar player
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;
    }

    void Update()
    {
        if (shopOpened) return;
        if (player == null) return;

        float dist = Vector3.Distance(transform.position, player.position);

        if (dist <= interactRange)
        {
            OpenShop();
        }
    }

    void OpenShop()
    {
        shopOpened = true;

        if (ShopManager.Instance == null)
        {
            Debug.LogError("[SHOP] No ShopManager in scene!");
            return;
        }

        // Generar ofertas
        ShopItem[] offerings = ShopManager.Instance.GenerateOfferings();

        // Abrir UI
        if (shopUI != null)
        {
            shopUI.Open(offerings);
        }
        else
        {
            Debug.LogError("[SHOP] No ShopUI assigned!");
        }

        Debug.Log("[SHOP] Shop opened!");
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, interactRange);
    }
}