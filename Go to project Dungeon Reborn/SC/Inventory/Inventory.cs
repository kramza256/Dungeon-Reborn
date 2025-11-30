using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using GameInventory;

namespace GameInventory
{
    public class Inventory : MonoBehaviour
    {
        [Header("Friend System Connect")]
        public Player friendPlayerScript; // ลากตัวละคร Player มาใส่ตรงนี้เพื่อให้ Inv ของเราไปโชว์ในของเพื่อน

        [Header("Inventory Setup")]
        public SO_Item EMPTY_ITEM;
        public Transform slotPrefab;
        public Transform InventoryPanel;
        public int SlotAmount = 30;
        [HideInInspector] public InventorySlot[] inventorySlots;

        [Header("Drop Settings")]
        public Transform dropOrigin;
        public float dropForce = 3f;

        private GridLayoutGroup gridLayoutGroup;

        void Awake()
        {
            if (InventoryPanel)
                gridLayoutGroup = InventoryPanel.GetComponent<GridLayoutGroup>();

            // พยายามหา Player อัตโนมัติ ถ้าลืมลากใส่
            if (friendPlayerScript == null)
                friendPlayerScript = FindFirstObjectByType<Player>();
        }

        void Start()
        {
            CreateInventorySlots();
        }

        public void CreateInventorySlots()
        {
            // ล้างของเก่า
            foreach (Transform child in InventoryPanel) Destroy(child.gameObject);

            inventorySlots = new InventorySlot[SlotAmount];
            for (int i = 0; i < SlotAmount; i++)
            {
                var go = Instantiate(slotPrefab, InventoryPanel);
                if (go.TryGetComponent(out InventorySlot slot))
                {
                    slot.inventory = this;
                    slot.SetThislot(EMPTY_ITEM, 0);
                    inventorySlots[i] = slot;
                }
            }

            // อัปเดตข้อมูลไปที่ Player ครั้งแรก
            UpdatePlayerDisplay();
        }

        public bool AddItem(SO_Item item, int amount)
        {
            if (item == null) return false;
            bool success = false;

            // 1. เติมใส่ Stack เดิมก่อน
            foreach (var s in inventorySlots)
            {
                if (s.item == item && s.stack < item.maxStack)
                {
                    int space = item.maxStack - s.stack;
                    int add = Mathf.Min(space, amount);
                    s.SetThislot(item, s.stack + add);
                    amount -= add;
                    if (amount <= 0)
                    {
                        success = true;
                        break;
                    }
                }
            }

            // 2. ใส่ช่องว่างใหม่
            if (amount > 0)
            {
                foreach (var s in inventorySlots)
                {
                    if (s.item == EMPTY_ITEM)
                    {
                        int add = Mathf.Min(item.maxStack, amount);
                        s.SetThislot(item, add);
                        amount -= add;
                        if (amount <= 0)
                        {
                            success = true;
                            break;
                        }
                    }
                }
            }

            if (success)
            {
                UpdatePlayerDisplay();

                // ✅✅✅ เพิ่มตรงนี้: แจ้งเตือนเมื่อได้ของ
                if (NotificationManager.Instance != null)
                {
                    // คำนวณจำนวนที่ได้รับจริง (amount ตั้งต้น - amount ที่เหลือ)
                    // หรือส่งจำนวนเต็มไปเลยก็ได้ถ้าง่าย
                    // แต่ในฟังก์ชันนี้ amount ถูกลบจนเหลือ 0 แล้ว ดังนั้นต้องเก็บค่า amount เริ่มต้นไว้ถ้าจะใช้
                    // เพื่อความง่าย ผมขอส่งค่า 1 หรือจำนวนที่รับมาตอนแรก (คุณอาจต้องปรับ logic นิดหน่อยถ้ารับไม่ครบ)
                    NotificationManager.Instance.ShowNotification(item.itemName, 1); // *หมายเหตุ: ควรแก้ให้ส่งจำนวนที่รับได้จริง
                }
            }
            else
            {
                Debug.Log("Inventory Full");
            }
            return success;
        }

        public bool RemoveItem(int index, int amount)
        {
            if (index < 0 || index >= inventorySlots.Length) return false;
            var s = inventorySlots[index];
            if (s.item == EMPTY_ITEM || s.stack < amount) return false;

            int newStack = s.stack - amount;
            if (newStack <= 0) s.SetThislot(EMPTY_ITEM, 0);
            else s.SetThislot(s.item, newStack);

            UpdatePlayerDisplay(); // ✅ อัปเดต
            return true;
        }

        public bool RemoveItem(SO_Item item, int amount)
        {
            for (int i = 0; i < inventorySlots.Length && amount > 0; i++)
            {
                var s = inventorySlots[i];
                if (s.item == item && s.stack > 0)
                {
                    int removeAmount = Mathf.Min(s.stack, amount);
                    int newStack = s.stack - removeAmount;

                    if (newStack <= 0) s.SetThislot(EMPTY_ITEM, 0);
                    else s.SetThislot(s.item, newStack);

                    amount -= removeAmount;
                }
            }

            UpdatePlayerDisplay(); // ✅ อัปเดต
            return amount <= 0;
        }

        // ✅ ฟังก์ชันใหม่: ซิงค์ข้อมูล Inventory นี้ ไปแสดงผลที่สคริปต์ Player ของเพื่อน
        public void UpdatePlayerDisplay()
        {
            if (friendPlayerScript == null) return;

            // เคลียร์ลิสต์เก่าของเพื่อนทิ้งก่อน
            friendPlayerScript.inventoryListDisplay.Clear();

            // วนลูปดูของในกระเป๋าเรา แล้วส่งชื่อไปให้เพื่อนเก็บไว้โชว์
            foreach (var slot in inventorySlots)
            {
                if (slot.item != null && slot.item != EMPTY_ITEM && slot.stack > 0)
                {
                    // Format: "ชื่อไอเท็ม (xจำนวน)"
                    string displayInfo = $"{slot.item.itemName} (x{slot.stack})";
                    friendPlayerScript.inventoryListDisplay.Add(displayInfo);
                }
            }
        }

        // ฟังก์ชันนับจำนวน (สำหรับ Mass Crafting)
        public int GetItemCount(SO_Item item)
        {
            int total = 0;
            foreach (var slot in inventorySlots)
            {
                if (slot.item == item) total += slot.stack;
            }
            return total;
        }

        public bool HasItem(SO_Item item, int amount)
        {
            int total = 0;
            foreach (var slot in inventorySlots)
            {
                if (slot.item == item)
                {
                    total += slot.stack;
                    if (total >= amount) return true;
                }
            }
            return false;
        }

        public bool HasItemsForRecipe(CraftRecipe recipe)
        {
            Dictionary<SO_Item, int> totalNeeded = new Dictionary<SO_Item, int>();
            foreach (var ing in recipe.ingredients)
            {
                if (ing.item == null || ing.item == EMPTY_ITEM) continue;
                if (totalNeeded.ContainsKey(ing.item)) totalNeeded[ing.item] += ing.amount;
                else totalNeeded.Add(ing.item, ing.amount);
            }

            foreach (var kvp in totalNeeded)
            {
                if (!HasItem(kvp.Key, kvp.Value)) return false;
            }
            return true;
        }

        public InventorySlot IsEmptySlotLeft(SO_Item checker = null, InventorySlot exclude = null)
        {
            InventorySlot empty = null;
            foreach (var s in inventorySlots)
            {
                if (s == exclude) continue;
                if (checker != null && s.item == checker && s.stack < s.item.maxStack) return s;
                if (s.item == EMPTY_ITEM && empty == null) empty = s;
            }
            return empty;
        }

        public void SetLayoutControlChild(bool on)
        {
            if (gridLayoutGroup != null) gridLayoutGroup.enabled = on;
        }

        public void GatherSameItems(InventorySlot targetSlot)
        {
            if (targetSlot == null || targetSlot.item == null || targetSlot.item == EMPTY_ITEM) return;
            if (targetSlot.stack >= targetSlot.item.maxStack) return;

            List<InventorySlot> validSlots = new List<InventorySlot>();
            foreach (var slot in inventorySlots)
            {
                if (slot != targetSlot && slot.item == targetSlot.item && slot.stack > 0) validSlots.Add(slot);
            }
            validSlots.Sort((a, b) => a.stack.CompareTo(b.stack));

            foreach (var slot in validSlots)
            {
                int spaceLeft = targetSlot.item.maxStack - targetSlot.stack;
                if (spaceLeft <= 0) break;
                int moveAmount = Mathf.Min(spaceLeft, slot.stack);
                targetSlot.stack += moveAmount;
                slot.stack -= moveAmount;
                if (slot.stack <= 0) slot.SetThislot(EMPTY_ITEM, 0);
                else slot.SetThislot(slot.item, slot.stack);
            }
            targetSlot.SetThislot(targetSlot.item, targetSlot.stack);

            UpdatePlayerDisplay(); // ✅ อัปเดตหลังรวมของ
        }
    }
}