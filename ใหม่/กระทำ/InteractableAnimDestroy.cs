using UnityEngine;
using System.Collections; // จำเป็นต้องมีบรรทัดนี้เพื่อใช้ Coroutine (ระบบรอเวลา)

public class InteractableAnimDestroy : InteractableBase
{
    [Header("Animation Settings")]
    [Tooltip("ลาก Animator ของตัวโมเดลมาใส่ที่นี่")]
    public Animator anim;

    [Tooltip("ชื่อ Trigger ใน Animator Controller ที่จะสั่งให้เล่นท่า")]
    public string triggerName = "Play";

    [Tooltip("ความยาวของอนิเมชั่น (วินาที) ต้องตั้งให้ตรงกับความยาวท่าจริง")]
    public float animationDuration = 2.0f;

    // ตัวป้องกันไม่ให้กด E รัวๆ ระหว่างเล่นท่า
    private bool isAnimating = false;

    public override void DoAction()
    {
        // ถ้ากำลังเล่นท่าอยู่ ห้ามกดซ้ำ
        if (isAnimating) return;

        // เริ่มกระบวนการเล่นท่าและทำลาย
        StartCoroutine(PlayAnimAndDestroySequence());
    }

    // Coroutine: ฟังก์ชันที่ทำงานเป็นลำดับขั้นและรอเวลาได้
    private IEnumerator PlayAnimAndDestroySequence()
    {
        isAnimating = true;

        // 1. ปิด Collider ทันที เพื่อไม่ให้เมาส์ชี้โดน หรือเดินชนได้อีก
        // (การปิด Collider จะทำให้ OnTriggerExit ทำงานอัตโนมัติ และข้อความ Press E จะหายไปเอง)
        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;

        // 2. สั่งเล่นอนิเมชั่น
        if (anim != null)
        {
            anim.SetTrigger(triggerName);
            Debug.Log("เริ่มเล่นอนิเมชั่น...");
            // อาจจะเพิ่มเสียงตรงนี้ เช่น audioSource.Play();
        }
        else
        {
            Debug.LogWarning("ลืมใส่ Animator ใน Inspector ครับ!");
        }

        // 3. ***สำคัญที่สุด*** รอเวลาตามความยาวของอนิเมชั่น
        yield return new WaitForSeconds(animationDuration);

        // 4. เมื่อรอจนครบเวลาแล้ว ให้ทำลายตัวเองทิ้ง
        Debug.Log("อนิเมชั่นจบแล้ว ทำลายวัตถุ!");
        Destroy(gameObject);
    }
}