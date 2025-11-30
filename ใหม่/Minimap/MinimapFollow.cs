using UnityEngine;

public class MinimapFollow : MonoBehaviour
{
    [Header("Target")]
    public Transform player; // ลากตัว Player มาใส่ช่องนี้

    void LateUpdate() // ใช้ LateUpdate ภาพจะได้ไม่สั่น
    {
        if (player == null) return;

        // 1. ตามตำแหน่ง (Position): เอาแค่แกน X กับ Z (แนวนอน) ส่วนความสูง (Y) เอาเท่าเดิม
        Vector3 newPos = player.position;
        newPos.y = transform.position.y; // รักษาระดับความสูงเดิมของกล้องไว้
        transform.position = newPos;

        // 2. ล็อกการหมุน (Rotation): บังคับให้ก้มหน้าลง 90 องศา และหันทิศเหนือ (Y=0) เสมอ
        transform.rotation = Quaternion.Euler(90f, 0f, 0f);
    }
}