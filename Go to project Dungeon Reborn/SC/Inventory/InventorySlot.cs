using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using GameInventory;
using UnityEngine.InputSystem;

namespace GameInventory
{
    public class InventorySlot : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
    {
        [Header("References")]
        public Inventory inventory;
        public SO_Item item;
        public int stack;
        public Image icon;
        public TextMeshProUGUI stackText;

        // ✅ เพิ่ม: ตัวแปรสำหรับ Text เลเวล
        public TextMeshProUGUI levelText;

        void Start() => RefreshUI();

        public void SetThislot(SO_Item newItem, int amount)
        {
            item = newItem;
            stack = amount;
            if (stack <= 0) item = null;
            if (item == null || item == inventory.EMPTY_ITEM)
            {
                item = inventory.EMPTY_ITEM;
                stack = 0;
            }
            RefreshUI();
        }

        private void RefreshUI()
        {
            // 1. จัดการ Icon
            if (icon)
            {
                icon.sprite = (item != null && item != inventory.EMPTY_ITEM) ? item.icon : null;
                icon.enabled = (item != null && item != inventory.EMPTY_ITEM);
            }

            // 2. จัดการตัวเลข (Stack vs Level)
            bool hasItem = (item != null && item != inventory.EMPTY_ITEM);

            // เช็คว่าควรโชว์ Stack ไหม (ถ้ามีมากกว่า 1 ชิ้น)
            if (stackText)
            {
                bool showStack = (hasItem && stack > 1);
                stackText.text = showStack ? stack.ToString() : "";
                stackText.gameObject.SetActive(showStack);
            }

            // ✅ เช็คว่าควรโชว์ Level ไหม (ถ้ามีชิ้นเดียว และ เลเวล > 0)
            if (levelText)
            {
                // โชว์เมื่อมีของ + ของมีแค่ 1 ชิ้น + เลเวลมากกว่า 0
                bool showLevel = (hasItem && stack == 1 && item.upgradeLevel > 0);

                if (showLevel)
                {
                    levelText.text = "+" + item.upgradeLevel.ToString();
                    levelText.gameObject.SetActive(true);
                }
                else
                {
                    levelText.gameObject.SetActive(false);
                }
            }
        }

        private void AttemptToPlaceOneItem()
        {
            if (MouseItemData.Instance == null) return;
            MouseItemData mouseItem = MouseItemData.Instance;
            if (mouseItem.assignedItem == null) return;

            bool isSlotEmpty = (item == null || item == inventory.EMPTY_ITEM);
            bool isSameItem = (item == mouseItem.assignedItem);

            if (isSlotEmpty || isSameItem)
            {
                if (!isSlotEmpty && stack >= item.maxStack) return;

                int newAmount = isSlotEmpty ? 1 : stack + 1;
                SetThislot(mouseItem.assignedItem, newAmount);

                mouseItem.assignedCount--;
                if (mouseItem.assignedCount <= 0) mouseItem.ClearSlot();
                else mouseItem.UpdateMouseItem(mouseItem.assignedItem, mouseItem.assignedCount);
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (MouseItemData.Instance == null) return;

            // ถ้าเราถือของอยู่แล้วคลิกขวา (เพื่อวางทีละชิ้น)
            if (MouseItemData.Instance.assignedItem != null &&
                Mouse.current != null && Mouse.current.rightButton.isPressed)
            {
                AttemptToPlaceOneItem();
                return;
            }

            // ถ้าเราแค่เอาเมาส์ไปชี้เฉยๆ (เพื่อดู Tooltip)
            if (item != null && item != inventory.EMPTY_ITEM && MouseItemData.Instance.assignedItem == null)
            {
                if (TooltipManager.Instance != null)
                {
                    // ✅ แก้ไข: เช็คก่อนว่าในชื่อมีเลขบวกแล้วหรือยัง
                    string nameToShow = item.itemName;
                    string levelSuffix = "+" + item.upgradeLevel;

                    // ถ้ามีเลเวล > 0 และ ในชื่อ "ยังไม่มี" คำว่า +1, +2
                    if (item.upgradeLevel > 0 && !item.itemName.Contains(levelSuffix))
                    {
                        // ค่อยเติมเลขต่อท้าย
                        nameToShow = $"{item.itemName} {levelSuffix}";
                    }

                    TooltipManager.Instance.ShowTooltip(nameToShow, item.description);
                }
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (TooltipManager.Instance != null) TooltipManager.Instance.HideTooltip();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (TooltipManager.Instance != null) TooltipManager.Instance.HideTooltip();

            bool isCtrlPressed = false;
            if (Keyboard.current != null)
            {
                isCtrlPressed = Keyboard.current.leftCtrlKey.isPressed || Keyboard.current.rightCtrlKey.isPressed;
            }

            if (isCtrlPressed && eventData.clickCount >= 2 && eventData.button == PointerEventData.InputButton.Left)
            {
                if (inventory != null) inventory.GatherSameItems(this);
                return;
            }

            if (MouseItemData.Instance == null) return;
            MouseItemData mouseItem = MouseItemData.Instance;

            if (eventData.button == PointerEventData.InputButton.Left)
            {
                if (mouseItem.assignedItem == null)
                {
                    if (item != null && item != inventory.EMPTY_ITEM)
                    {
                        mouseItem.UpdateMouseItem(item, stack);
                        SetThislot(inventory.EMPTY_ITEM, 0);
                    }
                }
                else
                {
                    if (item == null || item == inventory.EMPTY_ITEM)
                    {
                        SetThislot(mouseItem.assignedItem, mouseItem.assignedCount);
                        mouseItem.ClearSlot();
                    }
                    else if (item == mouseItem.assignedItem)
                    {
                        if (stack < item.maxStack)
                        {
                            int total = stack + mouseItem.assignedCount;
                            if (total <= item.maxStack)
                            {
                                SetThislot(item, total); mouseItem.ClearSlot();
                            }
                            else
                            {
                                int toAdd = item.maxStack - stack;
                                SetThislot(item, item.maxStack);
                                mouseItem.assignedCount -= toAdd;
                                mouseItem.UpdateMouseItem(mouseItem.assignedItem, mouseItem.assignedCount);
                            }
                        }
                    }
                    else
                    {
                        SO_Item tempItem = item; int tempStack = stack;
                        SetThislot(mouseItem.assignedItem, mouseItem.assignedCount);
                        mouseItem.UpdateMouseItem(tempItem, tempStack);
                    }
                }
            }
            else if (eventData.button == PointerEventData.InputButton.Right)
            {
                if (mouseItem.assignedItem == null)
                {
                    if (item != null && stack > 0)
                    {
                        int half = Mathf.CeilToInt(stack / 2f);
                        int remain = stack - half;
                        mouseItem.UpdateMouseItem(item, half);
                        SetThislot(item, remain);
                    }
                }
                else
                {
                    AttemptToPlaceOneItem();
                }
            }
        }
    }
}