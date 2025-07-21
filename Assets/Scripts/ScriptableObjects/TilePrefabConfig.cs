using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "TilePrefabConfig", menuName = "Scriptable Objects/TilePrefabConfig")]
public class TilePrefabConfig : ScriptableObject
{
    [System.Serializable]
    public class TilePrefabEntry
    {
        public TileType type;
        public GameObject prefab;
    }

    public TilePrefabEntry[] tilePrefabs;

    private Dictionary<TileType, GameObject> prefabDict;

    public GameObject GetPrefab(TileType type)
    {
        if (prefabDict == null)
        {
            prefabDict = tilePrefabs.ToDictionary(entry => entry.type, entry => entry.prefab);
        }

        return prefabDict.TryGetValue(type, out var prefab) ? prefab : null;
    }
}