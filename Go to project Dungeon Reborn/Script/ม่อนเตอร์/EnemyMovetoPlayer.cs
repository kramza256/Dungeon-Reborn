using UnityEngine;
using GameInventory; // เรียกใช้ระบบ Inventory

[System.Serializable]
public class LootItem
{
    public SO_Item item;        // ข้อมูลไอเท็ม
    public int amount = 1;      // จำนวน
    [Range(0, 100)] public float dropChance = 50f; // โอกาสดรอป %
}

public class EnemyMovetoPlayer : Enemy
{
    [Header("Auto Loot Settings")]
    public LootItem[] lootTable; // รายการของที่จะดรอปเข้าตัว

    private void Update()
    {
        if (player == null) return;

        // ... (ส่วนอื่นๆ เหมือนเดิม) ...

        // 4. เช็คระยะ
        if (GetDistanPlayer() < 3.5f)
        {
            // ✅ เพิ่มบรรทัดนี้: สั่งให้ขาหยุดขยับ (เพื่อให้ Animator ยอมเปลี่ยนเป็นท่าโจมตี)
            animator.SetFloat("Speed", 0);

            Attack(player);
        }
        else
        {
            Vector3 direction = (player.transform.position - transform.position).normalized;
            Move(direction);
        }
    }

    // เมื่อตาย
    public override void Die()
    {
        if (currentState == State.death) return;

        // แจกของเข้าตัว Player ทันที
        GiveLootToPlayer();

        base.Die();
    }

    void GiveLootToPlayer()
    {
        // กันเหนียว: ถ้าตายก่อนเจอ Player ให้ลองหา Player อีกที
        if (player == null) player = FindFirstObjectByType<Player>();

        if (player == null || lootTable == null) return;

        foreach (var loot in lootTable)
        {
            // คำนวณโอกาสดรอป (0-100%)
            float randomValue = Random.Range(0f, 100f);

            if (randomValue <= loot.dropChance)
            {
                // ยัดของเข้ากระเป๋า Player
                if (player.inventorySystem != null)
                {
                    bool added = player.inventorySystem.AddItem(loot.item, loot.amount);

                    if (added)
                    {
                        Debug.Log($"Enemy Dropped: {loot.item.itemName} x{loot.amount}");
                        // (Optional) ถ้ามี NotificationManager
                        // NotificationManager.Instance?.ShowNotification(loot.item.itemName, loot.amount);
                    }
                    else
                    {
                        Debug.Log("Inventory Full! Cannot loot item.");
                    }
                }
            }
        }
    }
}