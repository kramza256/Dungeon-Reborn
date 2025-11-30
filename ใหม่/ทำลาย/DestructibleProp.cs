using UnityEngine;
using System;
using GameInventory;

// โครงสร้างข้อมูลของดรอป
[Serializable]
public class PropLoot
{
    public SO_Item item;        // ไอเท็มที่จะดรอป
    public int amount = 1;      // จำนวน
    [Range(0, 100)] public float dropChance = 100f; // โอกาสดรอป
}

[RequireComponent(typeof(Collider))]
public class DestructibleProp : MonoBehaviour, Idestoryable
{
    // ============================================
    // ส่วน Interface Idestoryable (ต้องมีตามกฎ)
    // ============================================
    [SerializeField] private int _maxHealth = 20;
    private int _currentHealth;

    public int health
    {
        get { return _currentHealth; }
        set { _currentHealth = Mathf.Clamp(value, 0, _maxHealth); }
    }

    public int maxHealth { get => _maxHealth; set => _maxHealth = value; }
    public event Action<Idestoryable> OnDestory;
    // ============================================

    [Header("--- Settings ---")]
    public GameObject destroyEffect; // เอฟเฟกต์ตอนพัง (เศษไม้/ฝุ่น)

    [Header("--- Drop System ---")]
    public GameObject itemPickupPrefab; // ⚠️ สำคัญ: ต้องลาก Prefab ที่มีสคริปต์ ItemObject มาใส่
    public PropLoot[] lootTable;       // รายการของที่จะสุ่มดรอป
    public float dropForce = 5f;        // แรงดีดของไอเท็มตอนตก

    void Start()
    {
        health = maxHealth;
    }

    // ฟังก์ชันรับดาเมจ (Player ตีจะเข้าอันนี้)
    public void TakeDamage(int amount)
    {
        health -= amount;
        Debug.Log($"Object Hit! HP: {health}");

        // (Optional) เล่น Animation สั่นๆ ตรงนี้ได้

        if (health <= 0)
        {
            BreakObject();
        }
    }

    public void Heal(int amount) { /* วัตถุสิ่งของฮีลไม่ได้ */ }

    void BreakObject()
    {
        // 1. เล่นเอฟเฟกต์ (ถ้ามี)
        if (destroyEffect != null)
            Instantiate(destroyEffect, transform.position, Quaternion.identity);

        // 2. คำนวณของดรอป
        DropLoot();

        // 3. แจ้งเตือนระบบ (ถ้ามี) และทำลายตัวเอง
        OnDestory?.Invoke(this);
        Destroy(gameObject);
    }

    void DropLoot()
    {
        if (itemPickupPrefab == null || lootTable == null) return;

        foreach (var loot in lootTable)
        {
            // สุ่มโอกาสดรอป
            if (UnityEngine.Random.Range(0f, 100f) <= loot.dropChance)
            {
                SpawnItem(loot);
            }
        }
    }

    void SpawnItem(PropLoot loot)
    {
        // สุ่มตำแหน่งเล็กน้อยไม่ให้ของกองทับกัน
        Vector3 randomPos = UnityEngine.Random.insideUnitSphere * 0.5f;
        Vector3 spawnPos = transform.position + Vector3.up * 0.5f + randomPos;

        // สร้าง Item Pickup ขึ้นมา
        GameObject drop = Instantiate(itemPickupPrefab, spawnPos, Quaternion.identity);

        // ตั้งค่าข้อมูลไอเท็มให้กับของที่ตก
        var itemObj = drop.GetComponent<ItemObject>();
        if (itemObj != null)
        {
            itemObj.item = loot.item;
            itemObj.SetAmount(loot.amount);
        }

        // ใส่แรงดีดให้ของกระเด็นออกมาสวยๆ
        Rigidbody rb = drop.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.AddForce(Vector3.up * dropForce + UnityEngine.Random.insideUnitSphere * 2f, ForceMode.Impulse);
        }
    }
}