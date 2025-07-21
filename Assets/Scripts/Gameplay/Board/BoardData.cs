public class BoardData 
{
    public int Width { get; }
    public int Height { get; }
    public BoardCell[,] Cells { get; }

    public BoardData(int width, int height, bool[,] blockedMask)
    {
        Width = width;
        Height = height;
        Cells = new BoardCell[width, height];

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
}
