using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem; // ✅ เพิ่ม

public class TooltipManager : MonoBehaviour
{
    public static TooltipManager Instance;

    [Header("UI References")]
    public GameObject tooltipPanel;
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI descriptionText;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        HideTooltip();
    }

    private void Update()
    {
        if (tooltipPanel.activeSelf)
        {
            // ✅ แก้: รับตำแหน่งเมาส์แบบ New Input System
            if (Mouse.current != null)
            {
                Vector2 mousePos = Mouse.current.position.ReadValue();
                transform.position = mousePos + new Vector2(15, -15);
            }
        }
    }

    // ในไฟล์ TooltipManager.cs

    public void ShowTooltip(string title, string desc)
    {
        // ตั้งค่าข้อความ
        if (titleText != null) titleText.text = title;
        if (descriptionText != null) descriptionText.text = desc;

        // ✅ เปิด Panel ให้แสดงผล
        if (tooltipPanel != null)
        {
            tooltipPanel.SetActive(true);

            // ปรับตำแหน่งทันทีที่เปิด กันการกระพริบที่ตำแหน่งเก่า
            if (Mouse.current != null)
            {
                Vector2 mousePos = Mouse.current.position.ReadValue();
                transform.position = mousePos + new Vector2(15, -15);
            }
        }
    }

    public void HideTooltip()
    {
        // ✅ ปิด Panel เมื่อไม่ได้ใช้งาน
        if (tooltipPanel != null)
        {
            tooltipPanel.SetActive(false);
        }
    }
}