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
    private float hintDelay = 7f; // Секунд до подсказки
    private bool hintActive = false;
    private List<Vector2Int> currentHint = null;

    private Dictionary<Vector2Int, Coroutine> hintVibrations = new();
    private Dictionary<Vector2Int, Vector3> originalPositions = new();

    private void Update()
    {
        // Подсказка: если игрок не делает ходов hintDelay секунд
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

    // Сброс таймера подсказки при любом действии игрока
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

        float spacing = tileSize + tileSpacing;
        Vector2 offset = new Vector2(
            -((boardData.Width - 1) * spacing) / 2f,
            -((boardData.Height - 1) * spacing) / 2f
        );

        for (int x = 0; x < boardData.Width; x++)
        {
            for (int y = 0; y < boardData.Height; y++)
            {
                Vector3 position = new Vector3(x * spacing, y * spacing, 0f) + (Vector3)offset;
                Vector2Int coords = new(x, y);
                BoardCell cell = boardData.GetCell(x, y);

                GameObject tileObject;
                if (cell.IsBlocked)
                {
                    tileObject = tileFactory.CreateBlockedTile(position, boardRoot);
                }
                else
                {
                    tileObject = tileFactory.CreateTile(cell.Type.Value, position, boardRoot);
                    var input = tileObject.GetComponent<TileInputHandler>() ?? tileObject.AddComponent<TileInputHandler>();
                    input.Initialize(this, coords);
                }
                tileObjects[coords] = tileObject;
            }
        }
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
        OnPlayerAction(); // сбросить подсказку при клике
        Debug.Log($"OnTileClicked: {coords}");
        if (selectedCoords != null)
            Debug.Log($"Selected: {selectedCoords.Value}");
        if (isInputBlocked || !tileObjects.ContainsKey(coords))
            return;

        if (selectedCoords == null)
        {
            selectedCoords = coords;
            HighlightTile(coords, true);
        }
        else
        {
            Vector2Int selected = selectedCoords.Value;

            if (AreAdjacent(selected, coords))
            {
                HighlightTile(selected, false);
                SwapTiles(selected, coords);
                selectedCoords = null;
            }
            else
            {
                HighlightTile(selected, false);
                selectedCoords = coords;
                HighlightTile(coords, true);
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

        // Нельзя свапать заблокированные или пустые клетки
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
        // 1. Анимируем свап
        yield return StartCoroutine(SwapAnimationCoroutine(tileA, tileB, posA, posB));

        // 2. Меняем данные
        var temp = boardData.GetCell(a.x, a.y).Type;
        boardData.GetCell(a.x, a.y).Type = boardData.GetCell(b.x, b.y).Type;
        boardData.GetCell(b.x, b.y).Type = temp;

        // Меняем объекты местами в tileObjects
        var tempObj = tileObjects[a];
        tileObjects[a] = tileObjects[b];
        tileObjects[b] = tempObj;
        // Обновляем координаты в TileInputHandler
        if (tileObjects[a] != null)
            tileObjects[a].GetComponent<TileInputHandler>()?.Initialize(this, a);
        if (tileObjects[b] != null)
            tileObjects[b].GetComponent<TileInputHandler>()?.Initialize(this, b);

        // 3. Проверяем мэтчи
        var matches = MatchChecker.FindAllMatches(boardData);
        if (matches.Count > 0)
        {
            // Есть мэтч — запускаем обработку
            if (LevelProgressManager.Instance != null)
            {
                LevelProgressManager.Instance.OnMoveMade();
            }
            yield return StartCoroutine(HandleMatches(matches));
        }
        else
        {
            // Нет мэтча — возвращаем тайлы обратно
            // Меняем данные обратно
            temp = boardData.GetCell(a.x, a.y).Type;
            boardData.GetCell(a.x, a.y).Type = boardData.GetCell(b.x, b.y).Type;
            boardData.GetCell(b.x, b.y).Type = temp;
            // Анимируем возврат
            yield return StartCoroutine(SwapAnimationCoroutine(tileA, tileB, posB, posA));
            isInputBlocked = false;
        }
    }

    // Анимация свапа двух тайлов (универсальная)
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
    private IEnumerator HandleMatches(List<List<Vector2Int>> allMatches)
    {
        OnPlayerAction(); // сбросить подсказку при любом действии
        // Уведомляем LevelProgressManager о мэтче
        int totalTilesMatched = 0;
        int maxMatchSize = 0;
        foreach (var group in allMatches)
        {
            totalTilesMatched += group.Count;
            maxMatchSize = Mathf.Max(maxMatchSize, group.Count);
        }
        
        if (LevelProgressManager.Instance != null)
        {
            LevelProgressManager.Instance.OnTilesMatched(totalTilesMatched, maxMatchSize);
        }

        // 1. Удаляем мэтчи (визуально и из BoardData)
        HashSet<Vector2Int> toRemove = new HashSet<Vector2Int>();
        foreach (var group in allMatches)
            foreach (var pos in group)
                toRemove.Add(pos);

        foreach (var pos in toRemove)
        {
            if (tileObjects.TryGetValue(pos, out var tile) && tile != null)
            {
                Destroy(tile); // Можно заменить на pool.ReturnTile(...)
                tileObjects[pos] = null;
            }
            boardData.GetCell(pos.x, pos.y).Type = null;
        }

        yield return new WaitForSeconds(0.15f); // Короткая задержка для эффекта

        // 2. Падение и заполнение
        yield return StartCoroutine(DropTilesAndFill());

        // 3. Каскады: ищем новые мэтчи и повторяем процесс, если они есть
        var newMatches = MatchChecker.FindAllMatches(boardData);
        if (newMatches.Count > 0)
        {
            yield return StartCoroutine(HandleMatches(newMatches));
        }
        else
        {
            isInputBlocked = false;
            CheckAndRegenerateIfNoMoves(); // проверка после всех каскадов
        }
    }

    // Тайлы падают вниз, новые появляются сверху
    private IEnumerator DropTilesAndFill()
    {
        int width = boardData.Width;
        int height = boardData.Height;
        float dropDuration = 0.18f;
        float maxDropDistance = 0f;

        // 1. Падение существующих тайлов
        for (int x = 0; x < width; x++)
        {
            int emptyBelow = 0;
            for (int y = 0; y < height; y++)
            {
                var cell = boardData.GetCell(x, y);
                if (cell.IsBlocked)
                {
                    emptyBelow = 0;
                    continue;
                }
                if (cell.Type == null)
                {
                    emptyBelow++;
                }
                else if (emptyBelow > 0)
                {
                    // Сдвигаем данные
                    boardData.GetCell(x, y - emptyBelow).Type = cell.Type;
                    cell.Type = null;

                    // Сдвигаем визуальный объект
                    var from = new Vector2Int(x, y);
                    var to = new Vector2Int(x, y - emptyBelow);
                    if (tileObjects.TryGetValue(from, out var tile) && tile != null)
                    {
                        tileObjects[to] = tile;
                        tileObjects[from] = null;
                        // Обновляем координаты в TileInputHandler!
                        var input = tile.GetComponent<TileInputHandler>();
                        if (input != null)
                            input.Initialize(this, to);
                        StartCoroutine(MoveTile(tile, CalculateWorldPosition(from), CalculateWorldPosition(to), dropDuration));
                        if (emptyBelow > maxDropDistance) maxDropDistance = emptyBelow;
                    }
                }
            }
        }

        // 2. Появление новых тайлов сверху
        for (int x = 0; x < width; x++)
        {
            int emptyCount = 0;
            for (int y = 0; y < height; y++)
            {
                var cell = boardData.GetCell(x, y);
                if (cell.IsBlocked) continue;
                if (cell.Type == null) emptyCount++;
            }
            for (int i = 0; i < emptyCount; i++)
            {
                int y = height - 1 - i;
                var pos = new Vector2Int(x, y);
                var cell = boardData.GetCell(x, y);
                if (cell.IsBlocked || cell.Type != null) continue;

                var newType = (TileType)Random.Range(0, System.Enum.GetValues(typeof(TileType)).Length);
                cell.Type = newType;
                // Появление сверху: создаём тайл выше поля и анимируем вниз
                Vector3 spawnPos = CalculateWorldPosition(new Vector2Int(x, y + emptyCount));
                Vector3 targetPos = CalculateWorldPosition(pos);
                var tile = tileFactory.CreateTile(newType, spawnPos, boardRoot);
                tileObjects[pos] = tile;
                var input = tile.GetComponent<TileInputHandler>() ?? tile.AddComponent<TileInputHandler>();
                input.Initialize(this, pos);
                StartCoroutine(MoveTile(tile, spawnPos, targetPos, dropDuration));
            }
        }

        // Ждём окончания всех анимаций
        yield return new WaitForSeconds(dropDuration + 0.05f);

        // --- Проверка на дублирование объектов в tileObjects ---
        var seen = new HashSet<GameObject>();
        foreach (var kvp in tileObjects)
        {
            if (kvp.Value == null) continue;
            if (!seen.Add(kvp.Value))
            {
                Debug.LogWarning($"Дублирование объекта в tileObjects! Координаты: {kvp.Key}");
            }
        }
    }

    // Анимация перемещения тайла
    private IEnumerator MoveTile(GameObject tile, Vector3 from, Vector3 to, float duration)
    {
        float time = 0f;
        while (time < duration)
        {
            time += Time.deltaTime;
            float t = time / duration;
            tile.transform.position = Vector3.Lerp(from, to, t);
            yield return null;
        }
        tile.transform.position = to;
    }

    public void RestartBoard(LevelGameplayData levelData, float tileSize, float tileSpacing)
    {
        // Останавливаем все вибрации и сбрасываем позиции
        foreach (var kvp in hintVibrations)
        {
            if (kvp.Value != null)
                StopCoroutine(kvp.Value);
        }
        hintVibrations.Clear();
        originalPositions.Clear();
        HideHint();
        // Удаляем все старые тайлы
        foreach (var obj in tileObjects.Values)
        {
            if (obj != null)
                Destroy(obj);
        }
        tileObjects.Clear();

        // Генерируем новые данные поля
        boardData = BoardGenerator.Generate(levelData);

        // Инициализируем поле заново
        Initialize(boardData, tileSize, tileSpacing);

        // Сбросить выделение и ввод
        selectedCoords = null;
        isInputBlocked = false;
        CheckAndRegenerateIfNoMoves(); // проверка после перегенерации
    }

    // --- Профессиональная блокировка/разблокировка ввода ---
    public void BlockInput() { isInputBlocked = true; }
    public void UnblockInput() { isInputBlocked = false; }

    // --- Фича 1: Подсказка возможного хода ---
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
                    // Запускаем вибрацию
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
                    // Останавливаем вибрацию и возвращаем позицию
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
        float amplitude = 0.08f; // амплитуда вибрации
        float frequency = 12f;   // частота

        while (true)
        {
            t += Time.unscaledDeltaTime;
            float offset = Mathf.Sin(t * frequency) * amplitude;
            tileTransform.localPosition = originalPositions[pos] + new Vector3(offset, 0, 0);
            yield return null;
        }
    }

    // --- Фича 2: Перегенерация поля если нет возможных мэтчей ---
    private void CheckAndRegenerateIfNoMoves()
    {
        if (!HasAnyPossibleMatch())
        {
            Debug.Log("Нет возможных ходов! Перегенерируем поле...");
            RestartBoard(LevelProgressManager.Instance.CurrentLevel, TileSize, tileSpacing);
        }
    }

    // --- Поиск возможного мэтча (подсказка) ---
    private List<Vector2Int> FindAnyPossibleMatch()
    {
        // Перебираем все пары соседних тайлов и проверяем, приведёт ли свап к мэтчу
        int width = boardData.Width;
        int height = boardData.Height;
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                var cell = boardData.GetCell(x, y);
                if (cell.IsBlocked || !cell.Type.HasValue) continue;
                Vector2Int pos = new Vector2Int(x, y);
                // Проверяем соседей (вправо и вверх)
                Vector2Int[] directions = { new Vector2Int(1, 0), new Vector2Int(0, 1) };
                foreach (var dir in directions)
                {
                    int nx = x + dir.x;
                    int ny = y + dir.y;
                    if (nx < 0 || ny < 0 || nx >= width || ny >= height) continue;
                    var neighbor = boardData.GetCell(nx, ny);
                    if (neighbor.IsBlocked || !neighbor.Type.HasValue) continue;
                    // Пробуем свапнуть и проверить мэтчи
                    SwapTypes(pos, new Vector2Int(nx, ny));
                    var matches = MatchChecker.FindAllMatches(boardData);
                    SwapTypes(pos, new Vector2Int(nx, ny)); // Обратно
                    if (matches.Count > 0)
                    {
                        // Возвращаем первую найденную пару
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



