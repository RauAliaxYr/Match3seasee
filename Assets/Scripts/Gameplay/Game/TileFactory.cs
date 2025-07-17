using UnityEngine;
using System.Collections.Generic;

public class TileFactory : MonoBehaviour
{
    [Header("Настройки")]
    [SerializeField] private TilePrefabConfig tilePrefabConfig; // ScriptableObject с маппингом
    [SerializeField] private GameObject blockedTilePrefab;      // Префаб для заблокированных тайлов

    // Object pools
    private Dictionary<TileType, Queue<GameObject>> tilePools = new();
    private Queue<GameObject> blockedTilePool = new();

    /// <summary>
    /// Создаёт обычный тайл по типу с использованием пула.
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
                Debug.LogWarning($"Нет префаба для типа {type} в TilePrefabConfig");
                return null;
            }
            tile = Instantiate(prefab, position, Quaternion.identity, parent);
        }
        return tile;
    }

    /// <summary>
    /// Создаёт заблокированный тайл с использованием пула.
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
                Debug.LogWarning("Не назначен префаб заблокированного тайла.");
                return null;
            }
            tile = Instantiate(blockedTilePrefab, position, Quaternion.identity, parent);
        }
        return tile;
    }

    /// <summary>
    /// Возвращает обычный тайл в пул.
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
    /// Возвращает заблокированный тайл в пул.
    /// </summary>
    public void ReturnBlockedTile(GameObject tile)
    {
        tile.SetActive(false);
        tile.transform.SetParent(transform, false);
        blockedTilePool.Enqueue(tile);
    }
}