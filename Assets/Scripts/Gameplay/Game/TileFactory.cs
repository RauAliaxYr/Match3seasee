using UnityEngine;

public class TileFactory : MonoBehaviour
{
    [Header("Настройки")]
    [SerializeField] private TilePrefabConfig tilePrefabConfig; // ScriptableObject с маппингом
    [SerializeField] private GameObject blockedTilePrefab;      // Префаб для заблокированных тайлов

    /// <summary>
    /// Создаёт обычный тайл по типу.
    /// </summary>
    public GameObject CreateTile(TileType type, Vector3 position, Transform parent)
    {
        GameObject prefab = tilePrefabConfig.GetPrefab(type);
        if (prefab == null)
        {
            Debug.LogWarning($"Нет префаба для типа {type} в TilePrefabConfig");
            return null;
        }

        return Instantiate(prefab, position, Quaternion.identity, parent);
    }

    /// <summary>
    /// Создаёт заблокированный тайл.
    /// </summary>
    public GameObject CreateBlockedTile(Vector3 position, Transform parent)
    {
        if (blockedTilePrefab == null)
        {
            Debug.LogWarning("Не назначен префаб заблокированного тайла.");
            return null;
        }

        return Instantiate(blockedTilePrefab, position, Quaternion.identity, parent);
    }
}