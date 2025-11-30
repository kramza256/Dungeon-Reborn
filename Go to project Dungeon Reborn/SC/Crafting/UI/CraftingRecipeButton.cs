using UnityEngine;
using UnityEngine.UI;
using TMPro;
using GameInventory;
using UnityEngine.InputSystem; // เพิ่ม

public class CraftingRecipeButton : MonoBehaviour
{
    // ... (ตัวแปรเดิม) ...
    public Button button;
    public TextMeshProUGUI recipeNameText;
    private CraftRecipe recipe;
    private CraftingUIManager uiManager;

    public void Setup(CraftRecipe r, CraftingUIManager manager)
    {
        // ... (เหมือนเดิม) ...
        recipe = r;
        uiManager = manager;
        recipeNameText.text = r.outputItem.itemName;
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(OnClicked);
    }

    void OnClicked()
    {
        // ✅ แก้เช็ค Shift แบบใหม่
        bool isMaxFill = Keyboard.current != null &&
                         (Keyboard.current.leftShiftKey.isPressed || Keyboard.current.rightShiftKey.isPressed);

        uiManager.SelectRecipe(recipe, isMaxFill);
    }
}