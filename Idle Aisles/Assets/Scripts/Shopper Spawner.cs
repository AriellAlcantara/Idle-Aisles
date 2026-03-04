using UnityEngine;
using System.Collections.Generic;

public class ShopperSpawner : MonoBehaviour
{
    [Tooltip("Prefab to spawn (assign in Inspector)")]
    public GameObject shopperPrefab;

    [Tooltip("Maximum number of concurrent spawned objects")]
    public int maxSpawns = 10;

    private readonly List<GameObject> spawned = new List<GameObject>();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Optionally, you could prefill here. Leaving empty for now.
    }

    // Update is called once per frame
    void Update()
    {
        // Remove destroyed or missing entries from the list
        CleanupSpawnedList();

        if (shopperPrefab == null)
            return;

        // If we have fewer than the max, spawn immediately to fill the gap
        while (spawned.Count < maxSpawns)
        {
            GameObject go = Instantiate(shopperPrefab, transform.position, transform.rotation);
            spawned.Add(go);
        }
    }

    // Removes null references left when spawned objects are destroyed
    private void CleanupSpawnedList()
    {
        spawned.RemoveAll(item => item == null);
    }

    // Public helper to get the current number of active spawns
    public int GetCurrentSpawnCount()
    {
        CleanupSpawnedList();
        return spawned.Count;
    }
}
