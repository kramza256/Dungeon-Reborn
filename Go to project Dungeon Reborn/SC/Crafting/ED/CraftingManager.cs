using System.Collections.Generic;
using UnityEngine;

namespace GameInventory
{
    public class CraftingManager : MonoBehaviour
    {
        [Header("Configuration")]
        public SO_Item EMPTY_ITEM; // ไอเท็มว่างเปล่า (ใช้เช็คในสูตร)

        [Header("Database")]
        public CraftRecipe[] availableRecipes; // รายชื่อสูตรทั้งหมดในเกม

        // ❌ ลบฟังก์ชัน CraftItem() และ CanCraft() ออกทั้งหมด
        // เพราะหน้าที่การเช็คสูตรและการหักของ ตอนนี้ไปอยู่ที่ CraftingUIManager แล้วครับ
        // การเก็บไว้จะทำให้สับสนและอาจเกิดบั๊กซ้อนซ่อนเงื่อน
    }
}