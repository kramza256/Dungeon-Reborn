using UnityEngine;

namespace GameInventory
{
    [CreateAssetMenu(fileName = "New Item", menuName = "Game/Item Data", order = 1)]
    public class SO_Item : ScriptableObject
    {
        [Header("Basic Info")]
        public Sprite icon;
        public string id;
        public string itemName;
        [TextArea] public string description;
        public int maxStack = 1;
        public GameObject gamePrefab;

        [Header("Upgrade System")]
        public int upgradeLevel = 0;

        [Header("Item Type")]
        public ItemType itemType = ItemType.None;

        [Header("Weapon Stats")]
        [Tooltip("ดาเมจที่จะบวกเพิ่มให้ตัวละคร")]
        public int attackDamage = 0;

        [Tooltip("ระยะโจมตีของอาวุธ (เช่น มีด=1.5, หอก=3.0)")]
        public float attackRange = 1.5f;

        // ✅✅✅ เพิ่ม: ดีเลย์การตี (วินาที) ยิ่งเลขเยอะ ยิ่งตีช้า ✅✅✅
        [Tooltip("ความเร็วในการตี (วินาทีต่อครั้ง) เช่น 0.5 = ตีไว, 2.0 = ตีช้า")]
        public float attackDelay = 1.0f;

        public virtual void Use()
        {
            Debug.Log($"Use item: {itemName}");
        }

        private void OnValidate()
        {
            if (string.IsNullOrEmpty(itemName)) return;
            if (itemName.Contains("+"))
            {
                string[] parts = itemName.Split('+');
                if (parts.Length > 1)
                {
                    string numberString = parts[parts.Length - 1].Trim();
                    if (int.TryParse(numberString, out int level)) upgradeLevel = level;
                }
            }
            else upgradeLevel = 0;
        }

        public enum ItemType { None, Sword, Axe, Other }
    }
}