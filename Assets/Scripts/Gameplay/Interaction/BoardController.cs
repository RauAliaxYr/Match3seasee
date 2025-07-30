using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Controls the board logic, tile selection, swapping, and highlighting.
/// Handles tile instantiation and positioning.
/// </summary>
public class BoardController : MonoBehaviour
{
    [Header("Dependencies")]
    public TileFactory tileFactory;
    public Transform boardRoot;

    [Header("Layout")]
    public float tileSpacing = 0.1f;
    public float TileSize { get; private set; }

    private BoardData boardData;
    private readonly Dictionary<Vector2Int, GameObject> tileObjects = new();

    private Vector2Int? selectedCoords = null;
    private bool isInputBlocked = false;

    private float hintTimer = 0f;
    private float hintDelay = 7f; // Seconds until hint
    private bool hintActive = false;
    private List<Vector2Int> currentHint = null;

    private Dictionary<Vector2Int, Coroutine> hintVibrations = new();
    private Dictionary<Vector2Int, Vector3> originalPositions = new();

    private void Update()
    {
        // Hint: if player doesn't make moves for hintDelay seconds
        if (!isInputBlocked && !hintActive)
        {
            hintTimer += Time.deltaTime;
            if (hintTimer >= hintDelay)
            {
                ShowHint();
                hintTimer = 0f;
            }
        }
    }

    // Reset hint timer on any player action
    public void OnPlayerAction()
    {
        hintTimer = 0f;
        if (hintActive)
            HideHint();
    }

    /// <summary>
    /// Initializes the board with the given data, creating tile objects and positioning them.
    /// </summary>
    /// <param name="boardData">The board data to use for initialization.</param>
    /// <param name="tileSize">The size of each tile.</param>
    /// <param name="tileSpacing">The spacing between tiles.</param>
    public void Initialize(BoardData boardData, float tileSize, float tileSpacing)
    {
        this.boardData = boardData;
        this.TileSize = tileSize;
        this.tileSpacing = tileSpacing;
        tileObjects.Clear();

        for (int x = 0; x < boardData.Width; x++)
        {
            for (int y = 0; y < boardData.Height; y++)
            {
                Vector2Int coords = new(x, y);
                BoardCell cell = boardData.GetCell(x, y);

                GameObject tileObject;
                // Initial position - above the field
                Vector3 startPos = CalculateWorldPosition(new Vector2Int(x, y + boardData.Height));
                if (cell.IsBlocked)
                {
                    tileObject = tileFactory.CreateBlockedTile(Vector3.zero, boardRoot);
                    tileObject.transform.localPosition = CalculateWorldPosition(coords);
                }
                else if (cell.Type.HasValue)
                {
                    tileObject = tileFactory.CreateTile(cell.Type.Value, Vector3.zero, boardRoot);
                    tileObject.transform.localPosition = startPos;
                    var input = tileObject.GetComponent<TileInputHandler>() ?? tileObject.AddComponent<TileInputHandler>();
                    input.Initialize(this, coords);
                }
                else
                {
                    continue;
                }
                tileObjects[coords] = tileObject;
            }
        }
        // Start animation of all tiles falling
        StartCoroutine(AnimateInitialDrop());
    }

    /// <summary>
    /// Converts board coordinates to world position.
    /// </summary>
    private Vector3 CalculateWorldPosition(Vector2Int coords)
    {
        float spacing = TileSize + tileSpacing;
        Vector2 offset = new Vector2(
            -((boardData.Width - 1) * spacing) / 2f,
            -((boardData.Height - 1) * spacing) / 2f
        );
        return new Vector3(coords.x * spacing, coords.y * spacing, 0f) + (Vector3)offset;
    }

    /// <summary>
    /// Handles tile click events, managing selection and swapping.
    /// </summary>
    /// <param name="coords">The coordinates of the clicked tile.</param>
    public void OnTileClicked(Vector2Int coords)
    {
        OnPlayerAction(); // reset hint on click
        Debug.Log($"OnTileClicked: {coords}");
        if (selectedCoords != null)
            Debug.Log($"Selected: {selectedCoords.Value}");
        if (isInputBlocked || !tileObjects.ContainsKey(coords))
            return;

        if (selectedCoords == null)
        {
            selectedCoords = coords;
            HighlightTile(coords, true);
            // Tile selection sound
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayTileSelect();
            }
        }
        else
        {
            Vector2Int selected = selectedCoords.Value;

            if (AreAdjacent(selected, coords))
            {
                HighlightTile(selected, false);
                // Tile swap sound
                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlayTileSwap();
                }
                SwapTiles(selected, coords);
                selectedCoords = null;
            }
            else
            {
                HighlightTile(selected, false);
                selectedCoords = coords;
                HighlightTile(coords, true);
                // New tile selection sound
                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlayTileSelect();
                }
            }
        }
    }

    /// <summary>
    /// Checks if two coordinates are adjacent on the board.
    /// </summary>
    private bool AreAdjacent(Vector2Int a, Vector2Int b)
    {
        Debug.Log($"AreAdjacent: {a} <-> {b}");
        int dx = Mathf.Abs(a.x - b.x);
        int dy = Mathf.Abs(a.y - b.y);
        return (dx + dy) == 1;
    }

    /// <summary>
    /// Swaps two tiles and animates the swap.
    /// </summary>
    private void SwapTiles(Vector2Int a, Vector2Int b)
    {
        if (!tileObjects.TryGetValue(a, out var tileA) || !tileObjects.TryGetValue(b, out var tileB))
            return;

        // Cannot swap blocked or empty cells
        if (boardData.GetCell(a.x, a.y).IsBlocked || boardData.GetCell(b.x, b.y).IsBlocked)
            return;
        if (!boardData.GetCell(a.x, a.y).Type.HasValue || !boardData.GetCell(b.x, b.y).Type.HasValue)
            return;

        isInputBlocked = true;

        Vector3 posA = tileA.transform.position;
        Vector3 posB = tileB.transform.position;

        StartCoroutine(SwapAndCheckMatchCoroutine(a, b, tileA, tileB, posA, posB));
    }

    private IEnumerator SwapAndCheckMatchCoroutine(Vector2Int a, Vector2Int b, GameObject tileA, GameObject tileB, Vector3 posA, Vector3 posB)
    {
        // Save original objects
        var objA = tileObjects[a];
        var objB = tileObjects[b];

        // 1. Animate swap
        yield return StartCoroutine(SwapAnimationCoroutine(tileA, tileB, posA, posB));

        // 2. Change data
        var temp = boardData.GetCell(a.x, a.y).Type;
        boardData.GetCell(a.x, a.y).Type = boardData.GetCell(b.x, b.y).Type;
        boardData.GetCell(b.x, b.y).Type = temp;

        // Swap objects in tileObjects
        tileObjects[a] = tileObjects[b];
        tileObjects[b] = objA;
        if (tileObjects[a] != null) tileObjects[a].GetComponent<TileInputHandler>()?.Initialize(this, a);
        if (tileObjects[b] != null) tileObjects[b].GetComponent<TileInputHandler>()?.Initialize(this, b);

        // 3. Check matches
        var matches = MatchChecker.FindAllMatches(boardData);
        if (matches.Count > 0)
        {
            if (LevelProgressManager.Instance != null)
            {
                LevelProgressManager.Instance.OnMoveMade();
            }
            yield return StartCoroutine(HandleMatches(matches));
        }
        else
        {
            // No match - return everything back
            temp = boardData.GetCell(a.x, a.y).Type;
            boardData.GetCell(a.x, a.y).Type = boardData.GetCell(b.x, b.y).Type;
            boardData.GetCell(b.x, b.y).Type = temp;

            // Return objects and coordinates
            tileObjects[a] = objA;
            tileObjects[b] = objB;
            if (objA != null) objA.GetComponent<TileInputHandler>()?.Initialize(this, a);
            if (objB != null) objB.GetComponent<TileInputHandler>()?.Initialize(this, b);

            // Animate return
            yield return StartCoroutine(SwapAnimationCoroutine(tileA, tileB, posB, posA));
            if (LevelProgressManager.Instance != null)
            {
                LevelProgressManager.Instance.OnMoveMade(); // decrease move and on failed swap
            }
            isInputBlocked = false;
        }
    }

    // Animation of swapping two tiles (universal)
    private IEnumerator SwapAnimationCoroutine(GameObject tileA, GameObject tileB, Vector3 fromA, Vector3 fromB)
    {
        float duration = 0.25f;
        float time = 0f;
        while (time < duration)
        {
            time += Time.deltaTime;
            float t = time / duration;
            tileA.transform.position = Vector3.Lerp(fromA, fromB, t);
            tileB.transform.position = Vector3.Lerp(fromB, fromA, t);
            yield return null;
        }
        tileA.transform.position = fromB;
        tileB.transform.position = fromA;
    }

    /// <summary>
    /// Highlights or unhighlights a tile at the given coordinates.
    /// </summary>
    private void HighlightTile(Vector2Int coords, bool highlight)
    {
        if (tileObjects.TryGetValue(coords, out var tileObj))
        {
            var renderer = tileObj.GetComponent<SpriteRenderer>();
            if (renderer != null)
            {
                renderer.color = highlight ? Color.yellow : Color.white;
            }
        }
    }
    /// <summary>
    /// Calculates the center position of all matches in world coordinates.
    /// </summary>
    private Vector3 CalculateMatchCenter(List<List<Vector2Int>> allMatches)
    {
        if (allMatches == null || allMatches.Count == 0)
            return Vector3.zero;

        Vector3 totalPosition = Vector3.zero;
        int totalTiles = 0;

        foreach (var match in allMatches)
        {
            foreach (var coords in match)
            {
                // Use the actual tile object position instead of calculated position
                if (tileObjects.TryGetValue(coords, out var tileObj) && tileObj != null)
                {
                    totalPosition += tileObj.transform.position;
                    totalTiles++;
                }
            }
        }

        return totalTiles > 0 ? totalPosition / totalTiles : Vector3.zero;
    }

    private IEnumerator HandleMatches(List<List<Vector2Int>> allMatches)
    {
        OnPlayerAction(); // reset hint on any action
        
        // Match sound
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayMatch3();
        }
        
        // Notify LevelProgressManager about match
        int totalTilesMatched = 0;
        int maxMatchSize = 0;
        foreach (var group in allMatches)
        {
            totalTilesMatched += group.Count;
            maxMatchSize = Mathf.Max(maxMatchSize, group.Count);
        }
        
        if (LevelProgressManager.Instance != null)
        {
            // Calculate center position of all matches
            Vector3 matchCenter = CalculateMatchCenter(allMatches);
            LevelProgressManager.Instance.OnTilesMatched(totalTilesMatched, maxMatchSize, matchCenter);
        }

        // 1. Remove matches (visually and from BoardData)
        HashSet<Vector2Int> toRemove = new HashSet<Vector2Int>();
        foreach (var group in allMatches)
            foreach (var pos in group)
                toRemove.Add(pos);

        foreach (var pos in toRemove)
        {
            if (tileObjects.TryGetValue(pos, out var tile) && tile != null)
            {
                Destroy(tile); // Can be replaced with pool.ReturnTile(...)
                tileObjects[pos] = null;
            }
            boardData.GetCell(pos.x, pos.y).Type = null;
        }

        yield return new WaitForSeconds(0.15f); // Short delay for effect

        // 2. Drop and fill
        yield return StartCoroutine(DropTilesAndFill());

        // 3. Cascades: find new matches and repeat the process if they exist
        var newMatches = MatchChecker.FindAllMatches(boardData);
        if (newMatches.Count > 0)
        {
            yield return StartCoroutine(HandleMatches(newMatches));
        }
        else
        {
            isInputBlocked = false;
            CheckAndRegenerateIfNoMoves(); // Check after all cascades
        }
    }

    public void SyncVisualsWithBoardData(bool animateFromTop = false)
    {
        // Remove all old visual objects
        foreach (var obj in tileObjects.Values)
        {
            if (obj != null)
                Destroy(obj);
        }
        tileObjects.Clear();

        for (int x = 0; x < boardData.Width; x++)
        {
            for (int y = 0; y < boardData.Height; y++)
            {
                Vector2Int pos = new Vector2Int(x, y);
                BoardCell cell = boardData.GetCell(x, y);

                GameObject tileObject;
                Vector3 startPos = animateFromTop
                    ? CalculateWorldPosition(new Vector2Int(x, y + boardData.Height))
                    : CalculateWorldPosition(pos);

                if (cell.IsBlocked)
                {
                    tileObject = tileFactory.CreateBlockedTile(Vector3.zero, boardRoot);
                    tileObject.transform.localPosition = CalculateWorldPosition(pos);
                }
                else if (cell.Type.HasValue)
                {
                    tileObject = tileFactory.CreateTile(cell.Type.Value, Vector3.zero, boardRoot);
                    tileObject.transform.localPosition = startPos;
                    var input = tileObject.GetComponent<TileInputHandler>() ?? tileObject.AddComponent<TileInputHandler>();
                    input.Initialize(this, pos);
                }
                else
                {
                    continue; // Empty cell
                }
                tileObjects[pos] = tileObject;
            }
        }
    }

    // Called after dropping and restart
    private IEnumerator DropTilesAndFill()
    {
        int width = boardData.Width;
        int height = boardData.Height;
        float dropDuration = 0.18f;
        float maxDropDistance = 0f;

        // 1. Drop existing tiles
        for (int x = 0; x < width; x++)
        {
            for (int y = 1; y < height; y++) // Start from the second row (y=1)
            {
                var cell = boardData.GetCell(x, y);
                if (cell.IsBlocked || cell.Type == null) continue;

                int targetY = y;
                // Find the nearest empty unblocked cell below
                for (int yBelow = y - 1; yBelow >= 0; yBelow--)
                {
                    var belowCell = boardData.GetCell(x, yBelow);
                    if (belowCell.IsBlocked) break; // Block - don't fall further
                    if (belowCell.Type == null)
                    {
                        targetY = yBelow;
                    }
                }
                if (targetY != y)
                {
                    // Shift data
                    boardData.GetCell(x, targetY).Type = cell.Type;
                    cell.Type = null;

                    // Shift visual object
                    var from = new Vector2Int(x, y);
                    var to = new Vector2Int(x, targetY);
                    if (tileObjects.TryGetValue(from, out var tile) && tile != null)
                    {
                        tileObjects[to] = tile;
                        tileObjects[from] = null;
                        // Update coordinates in TileInputHandler
                        var input = tile.GetComponent<TileInputHandler>();
                        if (input != null)
                        {
                            input.Initialize(this, to);
                            Debug.Log($"[VIBRATION] Tile moved: {from} -> {to}, TileInputHandler coords set to {to}");
                        }
                        StartCoroutine(MoveTile(tile, CalculateWorldPosition(from), CalculateWorldPosition(to), dropDuration));
                        if ((y - targetY) > maxDropDistance) maxDropDistance = y - targetY;
                    }
                }
            }
        }

        // 2. Spawn new tiles from above
        for (int x = 0; x < width; x++)
        {
            // Count unblocked empty cells in the column
            List<int> emptyRows = new List<int>();
            for (int y = 0; y < height; y++)
            {
                var cell = boardData.GetCell(x, y);
                if (cell.IsBlocked) continue;
                if (cell.Type == null) emptyRows.Add(y);
            }
            int emptyCount = emptyRows.Count;
            for (int i = 0; i < emptyCount; i++)
            {
                int y = emptyRows[i];
                var pos = new Vector2Int(x, y);
                var cell = boardData.GetCell(x, y);
                if (cell.IsBlocked || cell.Type != null) continue;

                var newType = boardData.GetRandomAllowedTileType();
                cell.Type = newType;
                // Spawn from above: create tile above the field and animate down
                // Find spawn height: above the highest unblocked tile
                int spawnRow = y;
                for (int yAbove = y + 1; yAbove < height; yAbove++)
                {
                    if (boardData.GetCell(x, yAbove).IsBlocked) break;
                    spawnRow = yAbove;
                }
                Vector3 spawnPos = CalculateWorldPosition(new Vector2Int(x, spawnRow + 1));
                Vector3 targetPos = CalculateWorldPosition(pos);
                var tile = tileFactory.CreateTile(newType, Vector3.zero, boardRoot);
                tile.transform.localPosition = spawnPos;
                tileObjects[pos] = tile;
                var input = tile.GetComponent<TileInputHandler>() ?? tile.AddComponent<TileInputHandler>();
                input.Initialize(this, pos);
                Debug.Log($"[VIBRATION] New tile at {pos}, TileInputHandler coords set to {pos}");
                StartCoroutine(MoveTile(tile, spawnPos, targetPos, dropDuration));
            }
        }

        yield return new WaitForSeconds(dropDuration + 0.05f);

        // --- Check for duplicate objects in tileObjects ---
        var seen = new HashSet<GameObject>();
        foreach (var kvp in tileObjects)
        {
            if (kvp.Value == null) continue;
            if (!seen.Add(kvp.Value))
            {
                Debug.LogWarning($"Duplicate object in tileObjects! Coordinates: {kvp.Key}");
            }
        }
    }

    private IEnumerator AnimateInitialDrop()
    {
        int width = boardData.Width;
        int height = boardData.Height;
        float dropDuration = 0.25f;

        List<Coroutine> coroutines = new List<Coroutine>();

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector2Int pos = new Vector2Int(x, y);
                if (tileObjects.TryGetValue(pos, out var tile) && tile != null)
                {
                    Vector3 from = tile.transform.localPosition;
                    Vector3 to = CalculateWorldPosition(pos);
                    coroutines.Add(StartCoroutine(MoveTile(tile, from, to, dropDuration)));
                }
            }
        }
        yield return new WaitForSeconds(dropDuration + 0.05f);

        // Check for possible moves after initial board setup
        CheckAndRegenerateIfNoMoves();
    }

    // Tile movement animation
    private IEnumerator MoveTile(GameObject tile, Vector3 from, Vector3 to, float duration)
    {
        if (tile == null) yield break;
        float time = 0f;
        while (time < duration)
        {
            if (tile == null) yield break;
            time += Time.deltaTime;
            float t = time / duration;
            tile.transform.localPosition = Vector3.Lerp(from, to, t);
            yield return null;
        }
        if (tile != null)
            tile.transform.localPosition = to;
    }

    public void RestartBoard(LevelGameplayData levelData, float tileSize, float tileSpacing)
    {
        // Reset regeneration attempts counter
        regenerationAttempts = 0;
        
        // Stop all vibrations and reset positions
        foreach (var kvp in hintVibrations)
        {
            if (kvp.Value != null)
                StopCoroutine(kvp.Value);
        }
        hintVibrations.Clear();
        originalPositions.Clear();
        HideHint();
        // Remove all old tiles
        foreach (var obj in tileObjects.Values)
        {
            if (obj != null)
                Destroy(obj);
        }
        tileObjects.Clear();

        // Generate new board data
        boardData = BoardGenerator.Generate(levelData);

        // Reinitialize the board
        Initialize(boardData, tileSize, tileSpacing);

        // Reset selection and input
        selectedCoords = null;
        isInputBlocked = false;
        SyncVisualsWithBoardData(true); // Tiles appear from above
        StartCoroutine(AnimateInitialDrop());
        CheckAndRegenerateIfNoMoves(); // Check after regeneration
    }

    // --- Professional input blocking/unblocking ---
    public void BlockInput() { isInputBlocked = true; }
    public void UnblockInput() { isInputBlocked = false; }

    // --- Feature 1: Hint for possible move ---
    private void ShowHint()
    {
        var hint = FindAnyPossibleMatch();
        if (hint != null && hint.Count > 0)
        {
            currentHint = hint;
            hintActive = true;
            foreach (var pos in hint)
            {
                if (tileObjects.TryGetValue(pos, out var tile) && tile != null)
                {
                    // Start vibration
                    if (!hintVibrations.ContainsKey(pos) || hintVibrations[pos] == null)
                        hintVibrations[pos] = StartCoroutine(HintVibrationCoroutine(tile.transform, pos));
                }
            }
        }
    }
    private void HideHint()
    {
        if (currentHint != null)
        {
            foreach (var pos in currentHint)
            {
                if (tileObjects.TryGetValue(pos, out var tile) && tile != null)
                {
                    // Stop vibration and return position
                    if (hintVibrations.TryGetValue(pos, out var coroutine) && coroutine != null)
                    {
                        StopCoroutine(coroutine);
                        hintVibrations[pos] = null;
                    }
                    if (originalPositions.ContainsKey(pos))
                        tile.transform.localPosition = originalPositions[pos];
                }
            }
        }
        currentHint = null;
        hintActive = false;
    }

    private IEnumerator HintVibrationCoroutine(Transform tileTransform, Vector2Int pos)
    {
        if (!originalPositions.ContainsKey(pos))
            originalPositions[pos] = tileTransform.localPosition;

        float t = 0f;
        float amplitude = 0.08f; // vibration amplitude
        float frequency = 12f;   // frequency

        while (true)
        {
            t += Time.unscaledDeltaTime;
            float offset = Mathf.Sin(t * frequency) * amplitude;
            tileTransform.localPosition = originalPositions[pos] + new Vector3(offset, 0, 0);
            yield return null;
        }
    }

    // --- Feature 2: Regenerate board if no possible matches ---
    private int regenerationAttempts = 0;
    private const int maxRegenerationAttempts = 10;
    
    private void CheckAndRegenerateIfNoMoves()
    {
        if (!HasAnyPossibleMatch())
        {
            regenerationAttempts++;
            if (regenerationAttempts >= maxRegenerationAttempts)
            {
                Debug.LogWarning($"Failed to generate a playable board after {maxRegenerationAttempts} attempts. Using current board.");
                regenerationAttempts = 0;
                return;
            }
            
            Debug.Log($"No possible moves! Regenerating board... (Attempt {regenerationAttempts}/{maxRegenerationAttempts})");
            RestartBoard(LevelProgressManager.Instance.CurrentLevel, TileSize, tileSpacing);
            // Tiles will be animated from above through SyncVisualsWithBoardData(true) and AnimateInitialDrop
        }
        else
        {
            // Reset counter if board is playable
            regenerationAttempts = 0;
        }
    }

    // --- Search for possible match (hint) ---
    private List<Vector2Int> FindAnyPossibleMatch()
    {
        // Iterate through all pairs of adjacent tiles and check if swapping would lead to a match
        int width = boardData.Width;
        int height = boardData.Height;
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                var cell = boardData.GetCell(x, y);
                if (cell.IsBlocked || !cell.Type.HasValue) continue;
                Vector2Int pos = new Vector2Int(x, y);
                // Check neighbors (right and up)
                Vector2Int[] directions = { new Vector2Int(1, 0), new Vector2Int(0, 1) };
                foreach (var dir in directions)
                {
                    int nx = x + dir.x;
                    int ny = y + dir.y;
                    if (nx < 0 || ny < 0 || nx >= width || ny >= height) continue;
                    var neighbor = boardData.GetCell(nx, ny);
                    if (neighbor.IsBlocked || !neighbor.Type.HasValue) continue;
                    // Try swapping and check matches
                    SwapTypes(pos, new Vector2Int(nx, ny));
                    var matches = MatchChecker.FindAllMatches(boardData);
                    SwapTypes(pos, new Vector2Int(nx, ny)); // Back
                    if (matches.Count > 0)
                    {
                        // Return the first found pair
                        return new List<Vector2Int> { pos, new Vector2Int(nx, ny) };
                    }
                }
            }
        }
        return null;
    }
    private bool HasAnyPossibleMatch()
    {
        return FindAnyPossibleMatch() != null;
    }
    private void SwapTypes(Vector2Int a, Vector2Int b)
    {
        var temp = boardData.GetCell(a.x, a.y).Type;
        boardData.GetCell(a.x, a.y).Type = boardData.GetCell(b.x, b.y).Type;
        boardData.GetCell(b.x, b.y).Type = temp;
    }
}