using UnityEngine;
using GameInventory;

public class CollectableItem : MonoBehaviour
{
    public SO_Item item;      // ไอเทม ScriptableObject
    public int amount = 1;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        var player = other.GetComponent<Player>();
        if (player == null) return;

        // ส่งเข้า Inventory SLOT System ใหม่
        if (player.inventorySystem != null)
        {
            player.inventorySystem.AddItem(item, amount);
        }

        Destroy(gameObject); // เก็บแล้วหาย
    }
}

