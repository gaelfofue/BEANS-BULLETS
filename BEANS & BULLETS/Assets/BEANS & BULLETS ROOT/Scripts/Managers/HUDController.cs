using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HUDController : MonoBehaviour
{
    [Header("Info")]
    [SerializeField] private TextMeshProUGUI roomText;
    [SerializeField] private TextMeshProUGUI scoreText;

    [Header("Ammo")]
    [SerializeField] private TextMeshProUGUI ammoText;

    [Header("Upgrades")]
    [SerializeField] private Image[] upgradeSlots;

    private int killCount = 0;

    private void Update()
    {
        UpdateRoom();
    }

    private void UpdateRoom()
    {
        if (LevelManager.Instance == null) return;

        int rooms = LevelManager.Instance.GetRoomsCompleted();
        roomText.text = "ROOM " + (rooms + 1);
    }

    public void UpdateAmmo(int current, int max)
    {
        if (ammoText != null)
            ammoText.text = current + " / " + max;
    }

    public void RegisterKill()
    {
        killCount++;
        if (scoreText != null)
            scoreText.text = "KILLS: " + killCount;
    }

    public void SetUpgradeSlot(int index, Sprite icon)
    {
        if (index >= 0 && index < upgradeSlots.Length)
        {
            upgradeSlots[index].sprite = icon;
            upgradeSlots[index].color = Color.white;
        }
    }

    public void ClearUpgradeSlots()
    {
        foreach (var slot in upgradeSlots)
        {
            slot.sprite = null;
            slot.color = new Color(1f, 1f, 1f, 0.1f);
        }
    }
}