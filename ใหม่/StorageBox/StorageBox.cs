using UnityEngine;
using UnityEngine.Events;
using GameInventory;

// สร้างคลาสย่อยสำหรับระบุของและจำนวน
[System.Serializable]
public class BoxReward
{
    public SO_Item item;
    public int amount = 1;
}

public class StorageBox : MonoBehaviour, IInteractable
{
    [Header("🎁 รายการของรางวัล (ใส่กี่ชิ้นก็ได้)")]
    public BoxReward[] itemsToGive; // ✅ เปลี่ยนเป็น Array ให้ใส่ได้หลายช่อง

    [Header("⚙️ การตั้งค่า")]
    public bool destroyAfterLoot = false;

    [Header("✨ เอฟเฟกต์")]
    public UnityEvent onLooted;

    private bool isLooted = false;
    public bool isInteractable { get; set; } = true;

    public void Interact(Player player)
    {
        // ถ้าเก็บไปแล้ว หรือ ลืมใส่ของ -> จบข่าว
        if (isLooted || itemsToGive == null || itemsToGive.Length == 0) return;

        if (player.inventorySystem != null)
        {
            bool atLeastOneSuccess = false;

            // 🔄 วนลูปแจกของทีละชิ้น
            foreach (var reward in itemsToGive)
            {
                if (reward.item == null) continue;

                // ยัดเข้ากระเป๋า
                bool success = player.inventorySystem.AddItem(reward.item, reward.amount);

                if (success)
                {
                    atLeastOneSuccess = true;
                    Debug.Log($"เก็บสำเร็จ: {reward.item.itemName} x{reward.amount}");

                    // เด้งแจ้งเตือนทีละชิ้น
                    if (NotificationManager.Instance != null)
                    {
                        NotificationManager.Instance.ShowNotification(reward.item.itemName, reward.amount, reward.item.icon);
                    }
                }
                else
                {
                    Debug.Log($"กระเป๋าเต็ม! อดได้ {reward.item.itemName}");
                }
            }

            // ถ้าเก็บของได้สำเร็จอย่างน้อย 1 ชิ้น ถือว่าเปิดกล่องแล้ว
            if (atLeastOneSuccess)
            {
                isLooted = true;
                isInteractable = false;
                onLooted?.Invoke();

                if (destroyAfterLoot)
                {
                    Destroy(gameObject, 0.1f);
                }
            }
        }
    }
}