public class BoardCell 
{
    public bool IsBlocked { get; private set; }
    public TileType? Type { get; set; } 

    public BoardCell(bool isBlocked)
    {
        IsBlocked = isBlocked;
        Type = null;
    }
}
public enum TileType
{
    Red,
    Blue,
    Green,
    Yellow,
    Purple,
    White,
    Lagune,
    Black
}
