using UnityEngine;

// สร้างเมนูคลิกขวา Create > Game > Skill Data
[CreateAssetMenu(fileName = "New Skill", menuName = "Game/Skill Data")]
public class SO_Skill : ScriptableObject
{
    public string skillName;
    public Sprite icon;
    public float cooldown = 5f;
    public float staminaCost = 10f;

    // ประเภทของสกิล
    public SkillType type;
    public float value; // ความแรง (Damage) หรือ ปริมาณฮีล (Heal)
    public GameObject effectPrefab; // เอฟเฟกต์ (เช่น ลูกไฟ)

    public enum SkillType
    {
        Heal,       // ฮีล
        BuffSpeed,  // วิ่งไว
        AttackAOE   // โจมตีระเบิด
    }
}
