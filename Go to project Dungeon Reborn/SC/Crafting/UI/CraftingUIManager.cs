using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using GameInventory;
using UnityEngine.InputSystem;

public class CraftingUIManager : MonoBehaviour
{
    [Header("References")]
    public Inventory playerInventory;
    public CraftingManager craftingManager;

    [Header("UI Elements")]
    public Transform recipeListParent;
    public GameObject recipeButtonPrefab;
    public Button craftButton;
    public TextMeshProUGUI selectedRecipeText;
    public TextMeshProUGUI infoText;

    [Header("Crafting Grid")]
    public CraftingSlotUI[] craftingGridSlots;
    public Transform craftingGridParent;

    private CraftRecipe selectedRecipe;

    void Start()
    {
        PopulateRecipeList();
        if (craftButton)
        {
            craftButton.onClick.RemoveAllListeners();
            craftButton.onClick.AddListener(OnCraftButtonClicked);
        }
    }

    public void PopulateRecipeList()
    {
        foreach (Transform child in recipeListParent) Destroy(child.gameObject);
        foreach (var recipe in craftingManager.availableRecipes)
        {
            if (recipe == null) continue;
            GameObject btn = Instantiate(recipeButtonPrefab, recipeListParent);
            if (btn.TryGetComponent(out CraftingRecipeButton button))
            {
                button.Setup(recipe, this);
            }
        }
    }

    public void SelectRecipe(CraftRecipe recipe, bool fillMax = false)
    {
        // 1. เช็คว่าคืนของเก่าสำเร็จไหม (กันของหายกรณีเป๋าเต็ม)
        bool returnSuccess = ReturnCurrentGridToInventory();
        if (!returnSuccess)
        {
            if (infoText) infoText.text = "<color=red>Inventory Full! Clear space first.</color>";
            return;
        }

        selectedRecipe = recipe;
        if (selectedRecipeText) selectedRecipeText.text = recipe.outputItem.itemName;

        if (fillMax) FillGridBalanced(recipe);
        else FillGridSmart(recipe);

        CheckCraftingGridMatch();

        if (!IsGridMatchingRecipe(recipe))
        {
            if (infoText) infoText.text = "<color=red>Missing Materials!</color>";
        }
    }

    // ✅✅✅ แก้ไขจุดสำคัญ: เช็คของก่อนดึง (Prevent Item Loss)
    public void FillGridSmart(CraftRecipe recipe)
    {
        for (int i = 0; i < craftingGridSlots.Length; i++)
        {
            if (craftingGridSlots[i] == null) continue;
            if (i >= recipe.ingredients.Length)
            {
                craftingGridSlots[i].ClearDisplay();
                continue;
            }

            var ing = recipe.ingredients[i];

            if (ing.item != null && ing.item != craftingManager.EMPTY_ITEM && ing.amount > 0)
            {
                // 🔥 แก้ตรงนี้: เช็คก่อนว่ามีของพอไหม (HasItem)
                if (playerInventory.HasItem(ing.item, ing.amount))
                {
                    // ถ้ามีพอ -> ถึงค่อยดึงออก
                    playerInventory.RemoveItem(ing.item, ing.amount);
                    craftingGridSlots[i].SetItemDisplay(ing.item, ing.amount);
                }
                else
                {
                    // ถ้ามีไม่พอ -> ไม่ดึงของ! ให้ของคากระเป๋าไว้ แล้วโชว์ Ghost แทน
                    // (ไม้ 3 ชิ้นของคุณจะปลอดภัยอยู่ในกระเป๋า)
                    craftingGridSlots[i].SetGhostDisplay(ing.item, ing.amount);
                }
            }
            else
            {
                craftingGridSlots[i].ClearDisplay();
            }
        }
    }

    public void FillGridBalanced(CraftRecipe recipe)
    {
        ClearCraftingGrid();
        Dictionary<SO_Item, List<int>> itemDistributionMap = new Dictionary<SO_Item, List<int>>();

        for (int i = 0; i < recipe.ingredients.Length; i++)
        {
            var ing = recipe.ingredients[i];
            if (ing.item == null || ing.item == craftingManager.EMPTY_ITEM || ing.amount <= 0) continue;

            if (!itemDistributionMap.ContainsKey(ing.item))
                itemDistributionMap[ing.item] = new List<int>();

            itemDistributionMap[ing.item].Add(i);
        }

        foreach (var kvp in itemDistributionMap)
        {
            SO_Item item = kvp.Key;
            List<int> slotsToFill = kvp.Value;
            int totalInventoryCount = playerInventory.GetItemCount(item); // อันนี้ปลอดภัยอยู่แล้วเพราะเช็คจำนวนก่อน
            int slotsCount = slotsToFill.Count;

            int amountPerSlot = totalInventoryCount / slotsCount;
            int remainder = totalInventoryCount % slotsCount;

            for (int j = 0; j < slotsCount; j++)
            {
                int slotIndex = slotsToFill[j];
                int amountToPlace = amountPerSlot;
                if (j < remainder) amountToPlace += 1;
                amountToPlace = Mathf.Min(amountToPlace, item.maxStack);

                if (amountToPlace > 0)
                {
                    bool success = playerInventory.RemoveItem(item, amountToPlace);
                    if (success) craftingGridSlots[slotIndex].SetItemDisplay(item, amountToPlace);
                    else craftingGridSlots[slotIndex].SetGhostDisplay(item, 1);
                }
                else
                {
                    int recipeReqAmount = recipe.ingredients[slotIndex].amount;
                    craftingGridSlots[slotIndex].SetGhostDisplay(item, recipeReqAmount);
                }
            }
        }
    }

    public bool ReturnCurrentGridToInventory()
    {
        if (craftingGridSlots == null) return true;

        bool allReturned = true;

        foreach (var slot in craftingGridSlots)
        {
            if (slot == null) continue;

            if (slot.currentItem != null && slot.currentAmount > 0 && !slot.isGhost)
            {
                bool added = playerInventory.AddItem(slot.currentItem, slot.currentAmount);

                if (added)
                {
                    slot.ClearDisplay();
                }
                else
                {
                    Debug.LogWarning($"Cannot return {slot.currentItem.itemName} to inventory (Full).");
                    allReturned = false;
                }
            }
            else
            {
                slot.ClearDisplay();
            }
        }
        return allReturned;
    }

    public void OnCraftButtonClicked()
    {
        if (selectedRecipe == null)
        {
            if (infoText) infoText.text = "<color=red>No recipe selected!</color>";
            return;
        }

        if (!IsGridMatchingRecipe(selectedRecipe))
        {
            if (infoText) infoText.text = "<color=red>Missing Materials!</color>";
            return;
        }

        bool isMassCraft = false;
        if (Keyboard.current != null)
        {
            isMassCraft = Keyboard.current.leftShiftKey.isPressed || Keyboard.current.rightShiftKey.isPressed;
        }

        if (isMassCraft) CraftAll();
        else CraftSingle();
    }

    private void CraftSingle()
    {
        bool added = playerInventory.AddItem(selectedRecipe.outputItem, selectedRecipe.outputAmount);
        if (added)
        {
            ConsumeGridItems();
            if (infoText) infoText.text = "Crafted " + selectedRecipe.outputItem.itemName + "!";
        }
        else
        {
            if (infoText) infoText.text = "<color=red>Inventory Full!</color>";
        }
        CheckCraftingGridMatch();
    }

    private void CraftAll()
    {
        int maxCrafts = GetMaxCraftableAmount();
        int craftedCount = 0;

        for (int i = 0; i < maxCrafts; i++)
        {
            bool added = playerInventory.AddItem(selectedRecipe.outputItem, selectedRecipe.outputAmount);
            if (added)
            {
                ConsumeGridItems();
                craftedCount++;
            }
            else
            {
                if (infoText) infoText.text = "<color=red>Inventory Full!</color>";
                break;
            }
        }

        if (craftedCount > 0)
        {
            if (infoText) infoText.text = $"Crafted all! ({craftedCount}x)";
        }
        CheckCraftingGridMatch();
    }

    private int GetMaxCraftableAmount()
    {
        if (selectedRecipe == null) return 0;
        int maxPossible = int.MaxValue;

        for (int i = 0; i < craftingGridSlots.Length; i++)
        {
            if (craftingGridSlots[i] == null) continue;

            if (craftingGridSlots[i].currentItem != null && !craftingGridSlots[i].isGhost)
            {
                int required = 0;
                if (i < selectedRecipe.ingredients.Length)
                    required = selectedRecipe.ingredients[i].amount;

                if (required > 0)
                {
                    int available = craftingGridSlots[i].currentAmount;
                    int possibleForThisSlot = available / required;
                    if (possibleForThisSlot < maxPossible) maxPossible = possibleForThisSlot;
                }
            }
        }

        if (maxPossible == int.MaxValue) return 0;
        return maxPossible;
    }

    private void ConsumeGridItems()
    {
        CraftRecipe recipeToCraft = selectedRecipe;
        if (recipeToCraft == null || recipeToCraft.ingredients == null || craftingGridSlots == null) return;

        for (int i = 0; i < craftingGridSlots.Length; i++)
        {
            if (craftingGridSlots[i] == null) continue;

            if (craftingGridSlots[i].currentItem != null && !craftingGridSlots[i].isGhost)
            {
                int amountToRemove = 1;
                if (i < recipeToCraft.ingredients.Length)
                {
                    int recipeAmount = recipeToCraft.ingredients[i].amount;
                    if (recipeAmount > 0) amountToRemove = recipeAmount;
                }

                craftingGridSlots[i].currentAmount -= amountToRemove;

                if (craftingGridSlots[i].currentAmount <= 0) craftingGridSlots[i].ClearDisplay();
                else craftingGridSlots[i].SetItemDisplay(craftingGridSlots[i].currentItem, craftingGridSlots[i].currentAmount);
            }
        }
    }

    public void CheckCraftingGridMatch()
    {
        foreach (var recipe in craftingManager.availableRecipes)
        {
            if (IsGridMatchingRecipe(recipe))
            {
                selectedRecipe = recipe;
                if (infoText) infoText.text = "Ready to craft " + recipe.outputItem.itemName;
                return;
            }
        }
        if (infoText) infoText.text = "Arrange items to match a recipe.";
    }

    private bool IsGridMatchingRecipe(CraftRecipe recipe)
    {
        if (recipe == null || recipe.ingredients == null || craftingGridSlots == null) return false;
        if (craftingGridSlots.Length != recipe.ingredients.Length) return false;

        for (int i = 0; i < craftingGridSlots.Length; i++)
        {
            var slot = craftingGridSlots[i];
            if (slot == null) return false;
            var expected = recipe.ingredients[i];
            var slotItem = slot.isGhost ? null : slot.currentItem;
            var slotAmount = slot.isGhost ? 0 : slot.currentAmount;

            if (expected.item == null || expected.item == craftingManager.EMPTY_ITEM)
            {
                if (slotItem != null && slotItem != craftingManager.EMPTY_ITEM) return false;
                continue;
            }

            if (slotItem != expected.item || slotAmount < expected.amount) return false;
        }
        return true;
    }

    private void OnDisable() => ReturnCurrentGridToInventory();

    public void ClearCraftingGrid()
    {
        if (craftingGridSlots == null) return;
        foreach (var slot in craftingGridSlots)
            if (slot != null) slot.ClearDisplay();
    }
}