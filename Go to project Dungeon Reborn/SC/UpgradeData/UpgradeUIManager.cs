using UnityEngine;
using UnityEngine.UI;
using TMPro;
using GameInventory; // ใช้ Inventory ของคุณ

public class UpgradeUIManager : MonoBehaviour
{
    [Header("References")]
    public Inventory playerInventory;
    public UpgradeData[] allUpgradeRecipes; // ลากไฟล์ UpgradeData มาใส่ที่นี่

    [Header("UI Slots")]
    public CraftingSlotUI equipmentSlot; // ช่องบน (ใส่อาวุธ)
    public CraftingSlotUI materialSlot;  // ช่องล่าง (ใส่หิน)

    [Header("UI Controls")]
    public Button upgradeButton;
    public TextMeshProUGUI infoText;     // แสดงข้อความ (Success/Fail)
    public TextMeshProUGUI chanceText;   // แสดง % ความสำเร็จ

    private UpgradeData currentRecipe;

    void Start()
    {
        upgradeButton.onClick.AddListener(TryUpgrade);
        ClearUI();
    }

    void Update()
    {
        // เช็คตลอดเวลาว่าของในช่องตรงกับสูตรไหนไหม
        CheckRecipe();
    }

    // ฟังก์ชันเช็คสูตร (คล้าย Crafting)
    void CheckRecipe()
    {
        currentRecipe = null;
        chanceText.text = "";

        // 1. ต้องมีของทั้ง 2 ช่อง
        if (equipmentSlot.currentItem == null || materialSlot.currentItem == null)
        {
            infoText.text = "Place items to upgrade.";
            return;
        }

        // 2. วนหาใน Database ว่ามีสูตรไหนตรงกับคู่ผสมนี้ไหม
        foreach (var recipe in allUpgradeRecipes)
        {
            if (recipe.inputEquipment == equipmentSlot.currentItem &&
                recipe.upgradeMaterial == materialSlot.currentItem)
            {
                currentRecipe = recipe;

                // โชว์ % ความสำเร็จ
                chanceText.text = $"Chance: {recipe.successChance}% | Cost: {recipe.materialCost}";

                // เช็คว่าหินพอไหม
                if (materialSlot.currentAmount >= recipe.materialCost)
                {
                    infoText.text = "Ready to Upgrade!";
                }
                else
                {
                    infoText.text = "<color=red>Not enough stones!</color>";
                }
                return;
            }
        }

        infoText.text = "Invalid Combination";
    }

    // ฟังก์ชันกดปุ่มตีบวก
    public void TryUpgrade()
    {
        if (currentRecipe == null) return;

        // เช็คหินอีกรอบ
        if (materialSlot.currentAmount < currentRecipe.materialCost)
        {
            infoText.text = "Need more materials!";
            return;
        }

        // --- เริ่มการตีบวก ---

        // 1. หักหินออก
        materialSlot.currentAmount -= currentRecipe.materialCost;
        if (materialSlot.currentAmount <= 0) materialSlot.ClearDisplay();
        else materialSlot.SetItemDisplay(materialSlot.currentItem, materialSlot.currentAmount);

        // 2. สุ่มดวง (RNG)
        float randomValue = Random.Range(0f, 100f);
        bool isSuccess = randomValue <= currentRecipe.successChance;

        if (isSuccess)
        {
            // ✅ สำเร็จ! เปลี่ยนของในช่องบนเป็นของใหม่
            equipmentSlot.SetItemDisplay(currentRecipe.successOutput, 1);
            infoText.text = "<color=green>Upgrade SUCCESS! (+1)</color>";

            // (Optional) เล่นเสียงสำเร็จ
            // SoundManager.Instance.PlaySound("UpgradeSuccess");
        }
        else
        {
            // ❌ ล้มเหลว!
            if (currentRecipe.breakOnFail)
            {
                equipmentSlot.ClearDisplay(); // ของหาย
                infoText.text = "<color=red>Upgrade FAILED! Item Broken.</color>";
            }
            else if (currentRecipe.failOutput != null)
            {
                equipmentSlot.SetItemDisplay(currentRecipe.failOutput, 1); // ลดขั้น
                infoText.text = "<color=orange>Upgrade FAILED! Level Down.</color>";
            }
            else
            {
                // ล้มเหลวแต่ของยังอยู่ (แค่เสียหิน)
                infoText.text = "<color=yellow>Upgrade FAILED! (Item Safe)</color>";
            }

            // (Optional) เล่นเสียงแตก
            // SoundManager.Instance.PlaySound("UpgradeFail");
        }

        // บันทึกคืนเข้า Inventory (Optional: ถ้าอยากให้ Auto เก็บ)
        // หรือจะให้คาไว้ในช่องแล้วผู้เล่นหยิบออกเองก็ได้
    }

    public void ClearUI()
    {
        equipmentSlot.ClearDisplay();
        materialSlot.ClearDisplay();
        infoText.text = "Place items...";
        chanceText.text = "";
    }

    // ฟังก์ชันสำหรับปุ่มปิดหน้าต่าง (ถ้ามี)
    public void OnClosePanel()
    {
        // คืนของเข้าตัวก่อนปิด
        if (equipmentSlot.currentItem != null)
            playerInventory.AddItem(equipmentSlot.currentItem, equipmentSlot.currentAmount);

        if (materialSlot.currentItem != null)
            playerInventory.AddItem(materialSlot.currentItem, materialSlot.currentAmount);

        ClearUI();
    }

    // ✅ เพิ่มฟังก์ชันนี้: เมื่อหน้าต่าง Upgrade ถูกปิด ให้คืนของทันที
    private void OnDisable()
    {
        // คืนของช่องบน (Equipment)
        if (equipmentSlot != null && equipmentSlot.currentItem != null)
        {
            playerInventory.AddItem(equipmentSlot.currentItem, equipmentSlot.currentAmount);
            equipmentSlot.ClearDisplay();
        }

        // คืนของช่องล่าง (Material)
        if (materialSlot != null && materialSlot.currentItem != null)
        {
            playerInventory.AddItem(materialSlot.currentItem, materialSlot.currentAmount);
            materialSlot.ClearDisplay();
        }

        // รีเซ็ตข้อความ
        if (infoText) infoText.text = "Place items...";
        if (chanceText) chanceText.text = "";
    }
}