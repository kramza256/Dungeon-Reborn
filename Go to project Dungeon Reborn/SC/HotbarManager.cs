using GameInventory;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem; // เพิ่ม

public class HotbarManager : MonoBehaviour
{
    public static HotbarManager Instance;
    // ... (ตัวแปรเดิม) ...
    public Image swordSlotIcon;
    public Image axeSlotIcon;
    public Transform handPosition;

    private GameObject currentItem;
    private SO_Item swordItem;
    private SO_Item axeItem;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Update()
    {
        if (Keyboard.current == null) return;

        // แก้เป็นระบบใหม่
        if (Keyboard.current.digit1Key.wasPressedThisFrame && swordItem != null)
        {
            Equip(swordItem);
        }
        else if (Keyboard.current.digit2Key.wasPressedThisFrame && axeItem != null)
        {
            Equip(axeItem);
        }
    }

    // ... (ส่วน AssignToHotbar และ Equip เหมือนเดิม ก๊อปของเก่ามาใส่ได้เลย) ...
    public void AssignToHotbar(SO_Item item) { /* ... */ }
    public void Equip(SO_Item item) { /* ... */ }
}
