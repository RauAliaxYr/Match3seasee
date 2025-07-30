using System.Linq;
using UnityEngine;
using System.Collections.Generic;

public class BoardGenerator
{
    public static BoardData Generate(LevelGameplayData config)
    {
        int width = config.Width;
        int height = config.Height;
        int typeCount = config.TileTypesCount; // Use level-specific tile types count

        // Validate tile types count
        int maxTypes = BoardUtils.GetMaxTileTypes();
        if (typeCount <= 0 || typeCount > maxTypes)
        {
            Debug.LogWarning($"Invalid tile types count: {typeCount}. Using default value of 5.");
            typeCount = 5;
        }

        // Create list of allowed tile types
        List<TileType> allowedTypes = new List<TileType>();
        for (int i = 0; i < typeCount; i++)
        {
            allowedTypes.Add((TileType)i);
        }

        bool[,] blockedMask = config.GetBlockedCells();

        BoardData board;
        int attempts = 0;
        const int maxAttempts = 100;

        do
        {
            board = new BoardData(width, height, blockedMask, allowedTypes);
            FillBoardAvoidingMatches(board, typeCount);
            attempts++;

            if (MatchChecker.FindAllMatches(board).Count == 0)
                break;

            if (attempts >= maxAttempts)
            {
                Debug.LogWarning("Failed to generate a valid board without matches after 100 attempts.");
                break;
            }
        }
        while (true);

        return board;
    }

    private static void FillBoardAvoidingMatches(BoardData board, int typeCount)
    {
        System.Random rand = new System.Random();

        for (int x = 0; x < board.Width; x++)
        {
            for (int y = 0; y < board.Height; y++)
            {
                var cell = board.GetCell(x, y);
                if (cell.IsBlocked)
                    continue;

                // Use allowed tile types from board data
                var allowedTypes = new List<TileType>();
                for (int i = 0; i < typeCount; i++)
                {
                    allowedTypes.Add((TileType)i);
                }

                // Exclude types that will cause a match
                if (x >= 2 && 
                    board.GetCell(x - 1, y).Type == board.GetCell(x - 2, y).Type &&
                    board.GetCell(x - 1, y).Type.HasValue)
                {
                    allowedTypes.Remove(board.GetCell(x - 1, y).Type.Value);
                }

                if (y >= 2 && 
                    board.GetCell(x, y - 1).Type == board.GetCell(x, y - 2).Type &&
                    board.GetCell(x, y - 1).Type.HasValue)
                {
                    allowedTypes.Remove(board.GetCell(x, y - 1).Type.Value);
                }

                if (allowedTypes.Count == 0)
                {
                    cell.Type = board.GetRandomAllowedTileType();
                }
                else
                {
                    cell.Type = allowedTypes[rand.Next(allowedTypes.Count)];
                }
            }
        }
    }
}
