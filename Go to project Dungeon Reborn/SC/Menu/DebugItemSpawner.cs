using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameInventory;
using UnityEngine.InputSystem; // ✅ เพิ่มบรรทัดนี้

[System.Serializable]
public class ItemSpawnEntry
{
    public SO_Item item;
    public int amount;
}

public class DebugItemSpawner : MonoBehaviour
{
    public Inventory playerInventory;
    public List<ItemSpawnEntry> itemsToGive;

    void Update()
    {
        // ✅ แก้: เช็คปุ่ม F1 แบบระบบใหม่
        if (Keyboard.current != null && Keyboard.current.f1Key.wasPressedThisFrame)
        {
            foreach (var entry in itemsToGive)
            {
                if (entry.item != null && entry.amount > 0)
                {
                    playerInventory.AddItem(entry.item, entry.amount);
                    Debug.Log($"Gave {entry.amount} x {entry.item.itemName}");
                }
            }
        }
    }
}
