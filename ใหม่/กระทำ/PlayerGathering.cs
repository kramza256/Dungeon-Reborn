using UnityEngine;
using UnityEngine.InputSystem; // อย่าลืมบรรทัดนี้

public class PlayerGathering : MonoBehaviour
{
    // --- ส่วนตัวแปรที่หายไป (เติมให้แล้วครับ) ---
    public Animator playerAnim;

    [Header("ประเภทการทำงาน")]
    [Tooltip("0=ว่าง, 1=ขุดหิน, 2=ตัดไม้, 3=เกี่ยวข้าว")]
    public int actionID = 0;

    void Update()
    {
        // ใช้ระบบ Input แบบใหม่ (New Input System)
        if (Keyboard.current != null && Keyboard.current.eKey.isPressed)
        {
            if (playerAnim != null)
            {
                playerAnim.SetBool("IsWorking", true);
                playerAnim.SetInteger("ActionID", actionID);
            }
        }
        else
        {
            if (playerAnim != null)
            {
                playerAnim.SetBool("IsWorking", false);
            }
        }
    }

    // ฟังก์ชันสำหรับเปลี่ยนเครื่องมือ
    public void SetToolType(int typeID)
    {
        actionID = typeID;
    }
}