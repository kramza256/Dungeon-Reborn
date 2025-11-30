using UnityEngine;
using GameInventory; // เพิ่ม Namespace ของคุณ

public class Item : MonoBehaviour
{
    [Header("Inventory Data (New System)")]
    public SO_Item itemData; // ลาก SO_Item ของคุณมาใส่ที่นี่ใน Inspector
    public int amount = 1;

    // ... (ตัวแปรเดิมของเพื่อน เก็บไว้ได้) ...

    // ฟังก์ชันเก็บของ (เมื่อเดินชน หรือกดเก็บ)
    public virtual void OnCollect(Player player)
    {
        // 1. เช็คว่ามีข้อมูล Item Data ไหม?
        if (itemData != null)
        {
            // 2. ส่งเข้า Inventory ของคุณผ่าน Player
            player.AddItem(itemData, amount);

            Debug.Log($"Collected: {itemData.itemName} x{amount}");

            // 3. ทำลายของในฉาก
            Destroy(gameObject);
        }
        else
        {
            // ถ้าไม่มี itemData (เป็นของระบบเก่า) ก็ให้ทำงานแบบเดิม
            // ... (โค้ดเก่าของเพื่อน ใส่ตรงนี้ถ้าต้องการ) ...

            Debug.LogWarning("Collected item but no SO_Item assigned!");
            Destroy(gameObject);
        }
    }

    // เพิ่ม OnTriggerEnter เผื่อเดินชนแล้วเก็บเลย
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Player player = other.GetComponent<Player>();
            if (player != null)
            {
                OnCollect(player);
            }
        }
    }
}