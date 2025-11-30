using UnityEngine;
using TMPro;
using GameInventory;

[RequireComponent(typeof(Collider))]
public class ItemObject : MonoBehaviour
{
    public SO_Item item;
    public int amount = 1;
    public TextMeshProUGUI amountText;

    void Start()
    {
        if (amountText != null) amountText.text = amount.ToString();
    }

    public void SetAmount(int newAmount)
    {
        amount = newAmount;
        if (amountText) amountText.text = amount.ToString();
    }

    // รองรับการเดินชน (Trigger)
    private void OnTriggerEnter(Collider other)
    {
        TryPickUp(other.gameObject);
    }

    // รองรับการตกใส่ (Collision)
    private void OnCollisionEnter(Collision collision)
    {
        TryPickUp(collision.gameObject);
    }

    void TryPickUp(GameObject target)
    {
        // 1. เช็คว่าเป็น Player ไหม? (ต้องตั้ง Tag ให้ถูกนะ!)
        if (target.CompareTag("Player"))
        {
            // 2. พยายามหา Inventory ผ่านสคริปต์ Player.cs (วิธีนี้ชัวร์สุดสำหรับโค้ดคุณ)
            Player playerScript = target.GetComponent<Player>();

            if (playerScript != null && playerScript.inventorySystem != null)
            {
                bool success = playerScript.inventorySystem.AddItem(item, amount);
                if (success)
                {
                    Debug.Log($"เก็บ {item.itemName} สำเร็จ!");
                    Destroy(gameObject);
                }
                return; // จบการทำงาน
            }

            // 3. (สำรอง) เผื่อ Player ไม่มีสคริปต์ Player.cs แต่มี Inventory แปะอยู่ตรงๆ
            Inventory directInv = target.GetComponent<Inventory>();
            if (directInv != null)
            {
                if (directInv.AddItem(item, amount)) Destroy(gameObject);
            }
        }
    }
}