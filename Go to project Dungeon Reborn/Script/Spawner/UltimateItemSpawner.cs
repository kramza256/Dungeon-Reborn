using UnityEngine;

public class UltimateItemSpawner_NoDestroy : MonoBehaviour
{
   
    public GameObject[] itemPrefabs;

    
    public float spawnInterval = 2f;

   
    public Vector2 areaSize = new Vector2(5, 5);

   
    public int maxItems = 10;

    private int currentItems = 0;

    void Start()
    {
        InvokeRepeating("SpawnItem", 0f, spawnInterval);
    }

    void SpawnItem()
    {
        
        if (currentItems >= maxItems)
            return;

        
        int r = Random.Range(0, itemPrefabs.Length);

        
        Vector3 pos = new Vector3(
            Random.Range(-areaSize.x, areaSize.x),
            Random.Range(-areaSize.y, areaSize.y),
            0
        );

        
        Instantiate(itemPrefabs[r], pos, Quaternion.identity);

        
        currentItems++;
    }
}

