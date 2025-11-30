// Assets/Sc/ItemSpawner.cs
using UnityEngine;
using GameInventory;  // ให้ตรงกับ namespace ของ SO_Item และ ItemObject

public class ItemSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    public SO_Item itemToSpawn;            // ScriptableObject ใน namespace GameInventory
    public int amount = 1;
    public GameObject itemObjectPrefab;    // Prefab ที่มีสคริปต์ ItemObject
    public float spawnInterval = 5f;
    public Vector3 spawnAreaSize = Vector3.one;

    private float timer;

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= spawnInterval)
        {
            Spawn();
            timer = 0f;
        }
    }

    void Spawn()
    {
        // สุ่มตำแหน่งภายในกล่อง spawnAreaSize
        Vector3 local = new Vector3(
            Random.Range(-spawnAreaSize.x * 0.5f, spawnAreaSize.x * 0.5f),
            Random.Range(-spawnAreaSize.y * 0.5f, spawnAreaSize.y * 0.5f),
            Random.Range(-spawnAreaSize.z * 0.5f, spawnAreaSize.z * 0.5f)
        );
        Vector3 pos = transform.position + local;

        // สปาวน์ prefab และเซ็ตค่า SO_Item กับจำนวน
        var go = Instantiate(itemObjectPrefab, pos, Quaternion.identity);
        var io = go.GetComponent<ItemObject>();
        if (io != null)
        {
            io.item = itemToSpawn;    // กำหนดไอเท็มที่สปาวน์
            io.SetAmount(amount);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(transform.position, spawnAreaSize);
    }
}
