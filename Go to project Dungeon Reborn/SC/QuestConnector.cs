using UnityEngine;
using GameInventory; // เชื่อมกับ Inventory ของคุณ

// คลาสนี้ทำหน้าที่เป็นล่ามแปลภาษา ระหว่างระบบเพื่อน กับ ระบบของคุณ
public class QuestConnector : MonoBehaviour
{
    public static QuestConnector Instance;
    private Inventory playerInventory;

    void Awake()
    {
        Instance = this;
        playerInventory = FindFirstObjectByType<Inventory>();
    }

    // ฟังก์ชันให้ NPC ของเพื่อนเรียกใช้: "ผู้เล่นมีไอเท็มชื่อนี้ จำนวนเท่านี้ไหม?"
    public bool PlayerHasItem(string itemName, int amount)
    {
        if (playerInventory == null) return false;

        // วนลูปเช็คในกระเป๋า
        int count = 0;
        foreach (var slot in playerInventory.inventorySlots)
        {
            if (slot.item != null && slot.item.itemName == itemName)
            {
                count += slot.stack;
            }
        }
        return count >= amount;
    }

    // ฟังก์ชันให้ NPC ของเพื่อนเรียกใช้: "ยึดของเควสคืนมา"
    public void RemoveItemFromPlayer(string itemName, int amount)
    {
        if (playerInventory == null) return;

        // ต้องค้นหา SO_Item จากชื่อ (เพราะเพื่อนอาจจะส่งมาแค่ชื่อ string)
        // *วิธีที่ดีที่สุดคือคุณควรเอา SO_Item ไปใส่ในสคริปต์เพื่อนโดยตรง*
        // แต่ถ้าแก้ไม่ได้ ให้ใช้วิธีวนลูปหาแล้วลบ

        foreach (var slot in playerInventory.inventorySlots)
        {
            if (slot.item != null && slot.item.itemName == itemName)
            {
                int toRemove = Mathf.Min(amount, slot.stack);
                playerInventory.RemoveItem(slot.item, toRemove);
                amount -= toRemove;
                if (amount <= 0) break;
            }
        }
    }
}