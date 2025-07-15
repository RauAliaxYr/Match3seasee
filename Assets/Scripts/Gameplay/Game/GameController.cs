using UnityEngine;

public class GameController : MonoBehaviour
{
    [Header("Board")]
    [SerializeField] private Transform boardRoot;
    [SerializeField] private TileFactory tileFactory;

    [Header("Layout")]
    [SerializeField] private Vector2 boardSizeInUnits = new Vector2(6f, 6f);
    [SerializeField] private float tileSpacing = 0.1f;

    private BoardData board;

    private void Start()
    {
        LevelGameplayData config = GameManager.Instance.SelectedLevel;
        board = BoardGenerator.Generate(config);

        GenerateBoardVisual(board);
    }

    private void GenerateBoardVisual(BoardData boardData)
    {
        int width = boardData.Width;
        int height = boardData.Height;

        // Вычисляем размеры одного тайла с учётом отступа
        float tileWidth = boardSizeInUnits.x / width;
        float tileHeight = boardSizeInUnits.y / height;
        float tileSize = Mathf.Min(tileWidth, tileHeight);

        Vector2 spacingOffset = new Vector2(tileSize + tileSpacing, tileSize + tileSpacing);

        // Центрирование относительно центра boardRoot
        Vector2 offset = new Vector2(
            -((width - 1) * spacingOffset.x) / 2f,
            -((height - 1) * spacingOffset.y) / 2f
        );

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector3 position = new Vector3(
                    x * spacingOffset.x,
                    y * spacingOffset.y,
                    0f
                ) + (Vector3)offset;

                BoardCell cell = boardData.GetCell(x, y);

                if (cell.IsBlocked)
                {
                    tileFactory.CreateBlockedTile(position, boardRoot);
                }
                else if (cell.Type.HasValue )
                {
                    tileFactory.CreateTile(cell.Type.Value, position, boardRoot);
                }
            }
        }
    }
}
