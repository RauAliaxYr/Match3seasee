using UnityEngine;

public class TileInputHandler : MonoBehaviour
{
    private BoardController boardController;
    private Vector2Int coords;

    public void Initialize(BoardController controller, Vector2Int coords)
    {
        this.boardController = controller;
        this.coords = coords;
    }

    private void OnMouseDown()
    {
        Debug.Log($"Tile clicked at {coords}");
        boardController.OnTileClicked(coords);
    }
}
