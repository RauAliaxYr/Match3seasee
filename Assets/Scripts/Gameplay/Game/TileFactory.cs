using UnityEngine;
using System.Collections.Generic;

public class TileFactory : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private TilePrefabConfig tilePrefabConfig; // ScriptableObject with mapping
    [SerializeField] private GameObject blockedTilePrefab;      // Prefab for blocked tiles

    // Object pools
    private Dictionary<TileType, Queue<GameObject>> tilePools = new();
    private Queue<GameObject> blockedTilePool = new();

    /// <summary>
    /// Creates a regular tile by type using pool.
    /// </summary>
    public GameObject CreateTile(TileType type, Vector3 position, Transform parent)
    {
        if (!tilePools.TryGetValue(type, out var pool))
        {
            pool = new Queue<GameObject>();
            tilePools[type] = pool;
        }

        GameObject tile;
        if (pool.Count > 0)
        {
            tile = pool.Dequeue();
            tile.transform.SetPositionAndRotation(position, Quaternion.identity);
            tile.transform.SetParent(parent, false);
            tile.SetActive(true);
        }
        else
        {
            GameObject prefab = tilePrefabConfig.GetPrefab(type);
            if (prefab == null)
            {
                Debug.LogWarning($"No prefab for type {type} in TilePrefabConfig");
                return null;
            }
            tile = Instantiate(prefab, position, Quaternion.identity, parent);
        }
        return tile;
    }

    /// <summary>
    /// Creates a blocked tile using pool.
    /// </summary>
    public GameObject CreateBlockedTile(Vector3 position, Transform parent)
    {
        GameObject tile;
        if (blockedTilePool.Count > 0)
        {
            tile = blockedTilePool.Dequeue();
            tile.transform.SetPositionAndRotation(position, Quaternion.identity);
            tile.transform.SetParent(parent, false);
            tile.SetActive(true);
        }
        else
        {
            if (blockedTilePrefab == null)
            {
                Debug.LogWarning("Blocked tile prefab is not assigned.");
                return null;
            }
            tile = Instantiate(blockedTilePrefab, position, Quaternion.identity, parent);
        }
        return tile;
    }

    /// <summary>
    /// Returns a regular tile to pool.
    /// </summary>
    public void ReturnTile(TileType type, GameObject tile)
    {
        tile.SetActive(false);
        tile.transform.SetParent(transform, false);
        if (!tilePools.TryGetValue(type, out var pool))
        {
            pool = new Queue<GameObject>();
            tilePools[type] = pool;
        }
        pool.Enqueue(tile);
    }

    /// <summary>
    /// Returns a blocked tile to pool.
    /// </summary>
    public void ReturnBlockedTile(GameObject tile)
    {
        tile.SetActive(false);
        tile.transform.SetParent(transform, false);
        blockedTilePool.Enqueue(tile);
    }
}