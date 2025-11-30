using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameInventory
{
    [CreateAssetMenu(fileName = "New Craft Recipe", menuName = "Game/Craft Recipe", order = 2)]
    public class CraftRecipe : ScriptableObject
    {
        [System.Serializable]
        public struct Ingredient
        {
            public SO_Item item;
            public int amount;
        }

        public Ingredient[] ingredients;  // ไอเท็มต้นทาง
        public SO_Item outputItem;         // ไอเท็มผลลัพธ์
        public int outputAmount = 1;        // จำนวนที่ได้
    }
}
