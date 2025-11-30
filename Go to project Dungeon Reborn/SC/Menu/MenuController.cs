using UnityEngine;
using UnityEngine.UI;

public class MenuController : MonoBehaviour
{
    [Header("Panels (หน้าต่างต่างๆ)")]
    public GameObject panelInventory; // หน้ารวม Inventory + Crafting
    public GameObject upgradePage;         // หน้า Upgrade
    public GameObject craftingPanel;

    // ฟังก์ชันสำหรับเปิดหน้า Inventory (และปิดหน้าอื่น)
    public void ShowInventory()
    {
        panelInventory.SetActive(true); // เปิด
        craftingPanel.SetActive(true); // เปิด
        upgradePage.SetActive(false); //ปิด

        Debug.Log("Switched to Inventory Page");
    }

    // ฟังก์ชันสำหรับเปิดหน้า Equipment
    public void ShowUpgrade()
    {
        panelInventory.SetActive(true); //เปิด
        craftingPanel.SetActive(false); //ปิด
        upgradePage.SetActive(true); //เปิด

        Debug.Log("Switched to Equipment Page");
    }



}