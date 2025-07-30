using System.Collections.Generic;
using System.Linq;

public class BoardData 
{
    public int Width { get; }
    public int Height { get; }
    public BoardCell[,] Cells { get; }
    private List<TileType> allowedTileTypes;

    public BoardData(int width, int height, bool[,] blockedMask, List<TileType> allowedTypes = null)
    {
        Width = width;
        Height = height;
        Cells = new BoardCell[width, height];
        allowedTileTypes = allowedTypes ?? new List<TileType> { TileType.Red, TileType.Blue, TileType.Green, TileType.Yellow, TileType.Purple };

        for (int x = 0; x < width; x++)
        for (int y = 0; y < height; y++)
        {
            bool isBlocked = blockedMask != null && blockedMask[x, y];
            Cells[x, y] = new BoardCell(isBlocked);
        }
    }

    public BoardCell GetCell(int x, int y)
    {
        return Cells[x, y];
    }

    public TileType GetRandomAllowedTileType()
    {
        if (allowedTileTypes == null || allowedTileTypes.Count == 0)
        {
            // Fallback to default types if none specified
            return (TileType)UnityEngine.Random.Range(0, 5);
        }
        return allowedTileTypes[UnityEngine.Random.Range(0, allowedTileTypes.Count)];
    }
}
