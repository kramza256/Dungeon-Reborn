using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using GameInventory;
using UnityEngine.InputSystem; // ✅ เพิ่มบรรทัดนี้

public class CraftingSlotUI : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    public Image iconImage;
    public TextMeshProUGUI amountText;

    [HideInInspector] public SO_Item currentItem;
    [HideInInspector] public int currentAmount;
    public bool isGhost = false;

    private CraftingUIManager _uiManager;

    private void Awake()
    {
        _uiManager = Object.FindFirstObjectByType<CraftingUIManager>();
    }

    public void SetItemDisplay(SO_Item item, int amount)
    {
        isGhost = false;
        iconImage.color = Color.white;
        if (item == null || amount <= 0) { ClearDisplay(); return; }

        currentItem = item;
        currentAmount = amount;

        iconImage.sprite = item.icon;
        iconImage.enabled = true;
        amountText.text = amount > 1 ? amount.ToString() : "";

        NotifyManager();
    }

    public void SetGhostDisplay(SO_Item item, int amount)
    {
        currentItem = null; currentAmount = 0; isGhost = true;
        if (item != null)
        {
            iconImage.sprite = item.icon;
            iconImage.enabled = true;
            iconImage.color = new Color(1f, 1f, 1f, 0.4f);
            amountText.text = amount > 1 ? amount.ToString() : "";
        }
        else ClearDisplay();
    }

    public void ClearDisplay()
    {
        currentItem = null; currentAmount = 0; isGhost = false;
        iconImage.sprite = null; iconImage.enabled = false;
        iconImage.color = Color.white; amountText.text = "";

        NotifyManager();
    }

    private void NotifyManager()
    {
        if (_uiManager != null)
        {
            _uiManager.CheckCraftingGridMatch();
        }
        else
        {
            _uiManager = Object.FindFirstObjectByType<CraftingUIManager>();
            _uiManager?.CheckCraftingGridMatch();
        }
    }

    // ✅ ฟังก์ชันที่เคยหายไป (กู้คืนมาแล้ว)
    private void GatherSameItemsInGrid()
    {
        if (isGhost || currentItem == null) return;
        if (currentAmount >= currentItem.maxStack) return;

        if (_uiManager == null || _uiManager.craftingGridSlots == null) return;

        foreach (var otherSlot in _uiManager.craftingGridSlots)
        {
            if (otherSlot == this) continue;
            if (!otherSlot.isGhost && otherSlot.currentItem == currentItem)
            {
                int spaceLeft = currentItem.maxStack - currentAmount;
                if (spaceLeft <= 0) break;
                int moveAmount = Mathf.Min(spaceLeft, otherSlot.currentAmount);
                SetItemDisplay(currentItem, currentAmount + moveAmount);
                otherSlot.currentAmount -= moveAmount;
                if (otherSlot.currentAmount <= 0) otherSlot.ClearDisplay();
                else otherSlot.SetItemDisplay(otherSlot.currentItem, otherSlot.currentAmount);
            }
        }
    }

    // ✅ ฟังก์ชันที่เคยหายไป (กู้คืนมาแล้ว)
    private void AttemptToPlaceOneItem()
    {
        if (isGhost) ClearDisplay();
        if (MouseItemData.Instance == null) return;
        MouseItemData mouseItem = MouseItemData.Instance;
        if (mouseItem.assignedItem == null) return;

        bool isSlotEmpty = (currentItem == null);
        bool isSameItem = (currentItem == mouseItem.assignedItem);

        if (isSlotEmpty || isSameItem)
        {
            if (!isSlotEmpty && currentAmount >= currentItem.maxStack) return;
            int newAmount = isSlotEmpty ? 1 : currentAmount + 1;
            SetItemDisplay(mouseItem.assignedItem, newAmount);
            mouseItem.assignedCount--;
            if (mouseItem.assignedCount <= 0) mouseItem.ClearSlot();
            else mouseItem.UpdateMouseItem(mouseItem.assignedItem, mouseItem.assignedCount);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        // ✅ แก้: เช็คคลิกขวาค้างแบบ New Input System
        if (MouseItemData.Instance != null && MouseItemData.Instance.assignedItem != null &&
            Mouse.current != null && Mouse.current.rightButton.isPressed)
        {
            AttemptToPlaceOneItem();
            return;
        }

        if (TooltipManager.Instance != null)
        {
            if (currentItem != null && MouseItemData.Instance.assignedItem == null)
            {
                TooltipManager.Instance.ShowTooltip(currentItem.itemName, currentItem.description);
            }
        }
    }

    // ✅ ฟังก์ชันที่เคยหายไป (กู้คืนมาแล้ว เพื่อแก้ Error CS0535)
    public void OnPointerExit(PointerEventData eventData)
    {
        if (TooltipManager.Instance != null)
        {
            TooltipManager.Instance.HideTooltip();
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (TooltipManager.Instance != null) TooltipManager.Instance.HideTooltip();

        // ✅ แก้: เช็คปุ่ม Ctrl แบบ New Input System (ใช้ leftCtrlKey)
        bool isCtrlPressed = false;
        if (Keyboard.current != null)
        {
            isCtrlPressed = Keyboard.current.leftCtrlKey.isPressed || Keyboard.current.rightCtrlKey.isPressed;
        }

        if (isCtrlPressed && eventData.clickCount >= 2 && eventData.button == PointerEventData.InputButton.Left)
        {
            GatherSameItemsInGrid();
            return;
        }

        if (MouseItemData.Instance == null) return;
        MouseItemData mouseItem = MouseItemData.Instance;
        if (isGhost && (mouseItem == null || mouseItem.assignedItem == null)) return;

        if (eventData.button == PointerEventData.InputButton.Left)
        {
            if (mouseItem.assignedItem == null)
            {
                if (currentItem != null)
                {
                    mouseItem.UpdateMouseItem(currentItem, currentAmount);
                    ClearDisplay();
                }
            }
            else
            {
                if (currentItem == null)
                {
                    SetItemDisplay(mouseItem.assignedItem, mouseItem.assignedCount);
                    mouseItem.ClearSlot();
                }
                else if (currentItem == mouseItem.assignedItem)
                {
                    int space = currentItem.maxStack - currentAmount;
                    int toAdd = Mathf.Min(space, mouseItem.assignedCount);
                    if (toAdd > 0)
                    {
                        SetItemDisplay(currentItem, currentAmount + toAdd);
                        mouseItem.assignedCount -= toAdd;
                        if (mouseItem.assignedCount <= 0) mouseItem.ClearSlot();
                        else mouseItem.UpdateMouseItem(mouseItem.assignedItem, mouseItem.assignedCount);
                    }
                }
                else
                {
                    SO_Item tempItem = currentItem;
                    int tempAmount = currentAmount;
                    SetItemDisplay(mouseItem.assignedItem, mouseItem.assignedCount);
                    mouseItem.UpdateMouseItem(tempItem, tempAmount);
                }
            }
        }
        else if (eventData.button == PointerEventData.InputButton.Right)
        {
            if (mouseItem.assignedItem == null)
            {
                if (currentItem != null && currentAmount > 0)
                {
                    int half = Mathf.CeilToInt(currentAmount / 2f);
                    int remain = currentAmount - half;
                    mouseItem.UpdateMouseItem(currentItem, half);
                    SetItemDisplay(currentItem, remain);
                }
            }
            else
            {
                AttemptToPlaceOneItem();
            }
        }

        NotifyManager();
    }
}