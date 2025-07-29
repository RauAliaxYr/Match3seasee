using System.Linq;
using UnityEngine;

public class BoardGenerator
{
    public static BoardData Generate(LevelGameplayData config)
    {
        int width = config.Width;
        int height = config.Height;
        int typeCount = BoardUtils.GetRecommendedTypeCount(width, height);

        bool[,] blockedMask = config.GetBlockedCells();

        BoardData board;
        int attempts = 0;
        const int maxAttempts = 100;

        do
        {
            board = new BoardData(width, height, blockedMask);
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

                var possibleTypes = Enumerable.Range(0, typeCount).Cast<TileType>().ToList();

                // Exclude types that will cause a match
                if (x >= 2 && 
                    board.GetCell(x - 1, y).Type == board.GetCell(x - 2, y).Type &&
                    board.GetCell(x - 1, y).Type.HasValue)
                {
                    possibleTypes.Remove(board.GetCell(x - 1, y).Type.Value);
                }

                if (y >= 2 && 
                    board.GetCell(x, y - 1).Type == board.GetCell(x, y - 2).Type &&
                    board.GetCell(x, y - 1).Type.HasValue)
                {
                    possibleTypes.Remove(board.GetCell(x, y - 1).Type.Value);
                }

                if (possibleTypes.Count == 0)
                {
                    cell.Type = (TileType)rand.Next(typeCount);
                }
                else
                {
                    cell.Type = possibleTypes[rand.Next(possibleTypes.Count)];
                }
            }
        }
    }
}
