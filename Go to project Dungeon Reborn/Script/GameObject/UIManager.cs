using UnityEngine;

public class UIManager : MonoBehaviour
{
    public GameObject inventoryPage;
    public GameObject upgradePage;

    // 嗷源斯橐 Inventory
    public void OpenInventory()
    {
        inventoryPage.SetActive(true);
        upgradePage.SetActive(false);
    }

    // 嗷源斯橐 Upgrade
    public void OpenUpgrade()
    {
        inventoryPage.SetActive(false);
        upgradePage.SetActive(true);
    }
}

