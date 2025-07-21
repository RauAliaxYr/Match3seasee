using UnityEngine;

/// <summary>
/// Manages the overall game flow and board setup.
/// </summary>
public class GameController : MonoBehaviour
{
    [Header("Board")]
    [SerializeField] private Transform boardRoot;
    [SerializeField] private TileFactory tileFactory;
    [SerializeField] private BoardController boardController; // Assign via inspector or GetComponent

    [Header("Layout")]
    [SerializeField] private Vector2 boardSizeInUnits = new(6f, 6f);
    [SerializeField] private float tileSpacing = 0.1f;

    public float TileSize { get; private set; }
    private BoardData boardData;

    private void Awake()
    {
        if (boardController == null)
            boardController = GetComponent<BoardController>();
    }

    private void Start()
    {
        LevelGameplayData config = GameManager.Instance.SelectedLevel;
        boardData = BoardGenerator.Generate(config);

        TileSize = CalculateTileSize(boardData.Width, boardData.Height);

        // Set dependencies
        boardController.tileFactory = tileFactory;
        boardController.boardRoot = boardRoot;

        boardController.Initialize(boardData, TileSize, tileSpacing);

        // Инициализируем отслеживание прогресса уровня
        if (LevelProgressManager.Instance != null)
        {
            LevelProgressManager.Instance.InitializeLevel(config);
        }
    }

    private float CalculateTileSize(int width, int height)
    {
        float tileWidth = boardSizeInUnits.x / width;
        float tileHeight = boardSizeInUnits.y / height;
        return Mathf.Min(tileWidth, tileHeight);
    }
}