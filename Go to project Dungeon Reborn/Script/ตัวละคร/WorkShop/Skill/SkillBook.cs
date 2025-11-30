using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class SkillBook : MonoBehaviour
{
    public SkillButtonUI[] skillButtons;   // ลาก UI ปุ่มสกิลมาใส่

    public List<Skill> skillsSet = new List<Skill>();
    public GameObject[] skillEffects;
    List<Skill> DulationSkills = new List<Skill>();

    Player player;
    public void Start()
    {
        // เพิ่มสกิลต่างๆ เข้าไปใน List
        player = GetComponent<Player>();

        skillsSet.Add(new FireballSkill());
        skillsSet.Add(new HealSkill());
        skillsSet.Add(new BuffSkillMoveSpeed());
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            UseSkill(0); // ใช้สกิลที่ 1 (Fireball)
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            UseSkill(1); // ใช้สกิลที่ 2 (Heal)
        }
        else if(Input.GetKeyDown(KeyCode.Alpha3))
        {
            UseSkill(2); // ใช้สกิลที่ 3 (Buff Move Speed)
        }
        // อัปเดตสกิลที่มีผลต่อเนื่อง
        for (int i = DulationSkills.Count - 1; i >= 0; i--)
        {
            DulationSkills[i].UpdateSkill(player);
            if (DulationSkills[i].timer <= 0)
            {
                DulationSkills.RemoveAt(i);
            }
        }
    }

    public void UseSkill(int index)
    {
        if (index >= 0 && index < skillsSet.Count)
        {
            Skill skill = skillsSet[index];

            if (!skill.IsReady(Time.time))
            {
                Debug.Log("Skill on cooldown");
                return;
            }

            // สร้าง Effect
            GameObject g = Instantiate(skillEffects[index], transform.position, Quaternion.identity, transform);
            Destroy(g, 1);

            // ใช้สกิล
            skill.Activate(player);
            skill.TimeStampSkill(Time.time);

            // เริ่มคูลดาวน์ UI (สำหรับกดคีย์บอร์ด)
            if (skillButtons != null && index < skillButtons.Length)
            {
                skillButtons[index].StartCooldown(skill.cooldownTime);
            }

            // ถ้าเป็นสกิลแบบมีระยะเวลา
            if (skill.timer > 0)
            {
                DulationSkills.Add(skill);
            }
        }
    }

    private void OnDrawGizmos()
    {
        // Set the gizmo color
        Gizmos.color = Color.yellow;
        // Draw a wire sphere at the player's position with the fireball's search radius
        Gizmos.DrawWireSphere(transform.position, 5);
        
    }
}
