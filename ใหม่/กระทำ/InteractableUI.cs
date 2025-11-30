using UnityEngine;

public class InteractableUI : InteractableBase
{
    [Header("UI Settings")]
    public GameObject uiPanel; // ลาก GameObject ของหน้าต่าง UI มาใส่ตรงนี้ (เช่น Panel)

    private bool isOpen = false;

    // เมื่อกด E (ทำงานตามคำสั่งจากแม่)
    public override void DoAction()
    {
        ToggleUI();
    }

    void ToggleUI()
    {
        isOpen = !isOpen; // สลับค่า จริง/เท็จ

        if (uiPanel != null)
        {
            uiPanel.SetActive(isOpen); // เปิดหรือปิดตามค่า isOpen
            HandleMouseCursor(isOpen); // จัดการเรื่องเมาส์
        }
    }

    // ฟังก์ชันจัดการเมาส์ (สำคัญมาก! ไม่งั้นเมาส์จะหมุนมุมกล้องแทนที่จะคลิกปุ่ม)
    void HandleMouseCursor(bool show)
    {
        if (show)
        {
            Cursor.lockState = CursorLockMode.None; // ปลดล็อกเมาส์
            Cursor.visible = true; // โชว์เมาส์
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked; // ล็อกเมาส์ไว้กลางจอ (สำหรับเกม FPS/TPS)
            Cursor.visible = false; // ซ่อนเมาส์
        }
    }

    // (เสริม) ถ้าเดินหนี ให้ปิด UI อัตโนมัติ
    protected override void OnTriggerExit(Collider other)
    {
        // เรียกคำสั่งเดิมของแม่ด้วย (เพื่อเซ็ต isPlayerInRange = false)
        base.OnTriggerExit(other);

        // สั่งปิด UI
        if (isOpen && other.CompareTag("Player"))
        {
            isOpen = false;
            uiPanel.SetActive(false);
            HandleMouseCursor(false);
        }
    }
}