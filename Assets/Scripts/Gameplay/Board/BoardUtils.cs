using UnityEngine;

public static class BoardUtils
{
    public static int GetRecommendedTypeCount(int width, int height)
    {
        return Mathf.Clamp(Mathf.RoundToInt(3 + Mathf.Log(width * height, 2)), 3, 8);
    }

    public static int GetMaxTileTypes()
    {
        return System.Enum.GetValues(typeof(TileType)).Length;
    }

    public static int GetRecommendedTypeCountForLevel(int width, int height)
    {
        // This method can be used as a fallback or for level design suggestions
        return Mathf.Clamp(Mathf.RoundToInt(3 + Mathf.Log(width * height, 2)), 3, GetMaxTileTypes());
    }
}
