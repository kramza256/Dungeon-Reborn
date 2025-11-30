using UnityEngine;
using GameInventory; // ใช้ Namespace ของคุณ

[CreateAssetMenu(fileName = "New Upgrade Data", menuName = "Game/Upgrade Data")]
public class UpgradeData : ScriptableObject
{
    [Header("Upgrade Info")]
    public SO_Item inputEquipment;      // อุปกรณ์ตั้งต้น (เช่น Sword)
    public SO_Item upgradeMaterial;     // วัตถุดิบ (เช่น Upgrade Stone)
    public SO_Item successOutput;       // ผลลัพธ์ถ้าสำเร็จ (เช่น Sword+1)

    [Header("Settings")]
    [Range(0, 100)] public float successChance = 50f; // โอกาสสำเร็จ 0-100%
    public int materialCost = 1;        // จำนวนหินที่ใช้

    [Header("Fail Consequence")]
    public bool breakOnFail = false;    // ถ้าแตก ของหายเลยไหม?
    public SO_Item failOutput;          // ถ้าแตก จะได้อะไรคืนมา (เช่น เศษเหล็ก หรือ Sword+0)
}