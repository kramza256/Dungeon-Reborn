using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameInventory;
using UnityEngine.InputSystem; // เพิ่ม

public class ItemPicker : MonoBehaviour
{
    [Header("Pickup Settings")]
    public float pickupRadius = 2f;
    // public KeyCode pickupKey = KeyCode.E; // อันเก่าใช้ไม่ได้แล้ว

    void Update()
    {
        // แก้เป็นกด E แบบใหม่
        if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            Collider[] hits = Physics.OverlapSphere(transform.position, pickupRadius);
            foreach (var c in hits)
            {
                ItemObject io = c.GetComponent<ItemObject>();
                if (io != null)
                {
                    var inv = GetComponent<Inventory>();
                    if (inv != null)
                    {
                        inv.AddItem(io.item, io.amount);
                        Destroy(io.gameObject);
                    }
                }
            }
        }
    }
    // ... (OnDrawGizmosSelected เหมือนเดิม) ...
}
