using UnityEngine;

public static class BoardUtils
{
    public static int GetRecommendedTypeCount(int width, int height)
    {
        return Mathf.Clamp(Mathf.RoundToInt(3 + Mathf.Log(width * height, 2)), 3, 8);
    }
}
