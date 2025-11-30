using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using GameInventory;

public class EquipmentSlotUI : MonoBehaviour, IPointerClickHandler
{
    [Header("References")]
    public Player player;       // ลากตัว Player มาใส่
    public Image iconImage;     // รูปไอคอนในช่อง
    public Image background;    // พื้นหลัง (เผื่ออยากเปลี่ยนสีตอนใส่ของ)

    [Header("Current State")]
    public SO_Item currentItem;

    private void Start()
    {
        // ถ้าลืมลาก Player มาใส่ ให้หาเอง
        if (player == null) player = FindFirstObjectByType<Player>();
        UpdateSlotUI();
    }

    // ฟังก์ชันอัปเดตหน้าตา UI
    public void UpdateSlotUI()
    {
        if (currentItem != null)
        {
            iconImage.sprite = currentItem.icon;
            iconImage.enabled = true;
            iconImage.color = Color.white;
        }
        else
        {
            iconImage.sprite = null;
            iconImage.enabled = false;
            iconImage.color = Color.clear;
        }
    }

    // จัดการการคลิก (ใส่/ถอด อาวุธ)
    public void OnPointerClick(PointerEventData eventData)
    {
        if (MouseItemData.Instance == null) return;
        MouseItemData mouseItem = MouseItemData.Instance;

        // 1. ถ้าเมาส์ถือของอยู่ -> พยายามใส่ของลงช่อง
        if (mouseItem.assignedItem != null)
        {
            // เช็คว่าเป็นอาวุธหรือไม่ (Sword หรือ Axe)
            if (IsWeapon(mouseItem.assignedItem))
            {
                // สลับของ: เอาของใหม่ใส่ช่อง, เอาของเก่า(ถ้ามี)กลับไปติดเมาส์
                SO_Item previousItem = currentItem;

                SetEquipment(mouseItem.assignedItem); // สวมใส่

                // คืนของเก่าเข้าเมาส์ หรือ เคลียร์เมาส์
                if (previousItem != null)
                {
                    mouseItem.UpdateMouseItem(previousItem, 1);
                }
                else
                {
                    mouseItem.ClearSlot();
                }
            }
            else
            {
                Debug.Log("Item is not a weapon!"); // แจ้งเตือนถ้าไม่ใช่ดาบ/ขวาน
            }
        }
        // 2. ถ้าเมาส์ว่าง -> ถอดของออกจากช่อง
        else if (currentItem != null)
        {
            mouseItem.UpdateMouseItem(currentItem, 1); // เอาของติดเมาส์ไป
            Unequip(); // ถอดออก
        }
    }

    // ฟังก์ชันเช็คประเภทไอเท็ม
    private bool IsWeapon(SO_Item item)
    {
        return item.itemType == SO_Item.ItemType.Sword ||
               item.itemType == SO_Item.ItemType.Axe;
    }

    // สั่งสวมใส่
    public void SetEquipment(SO_Item newItem)
    {
        currentItem = newItem;
        UpdateSlotUI();

        // สั่งให้ตัวละครถืออาวุธ
        if (player != null) player.EquipWeapon(newItem);
    }

    // สั่งถอด
    public void Unequip()
    {
        currentItem = null;
        UpdateSlotUI();

        // สั่งให้ตัวละครเลิกถือ
        if (player != null) player.UnequipWeapon();
    }
}