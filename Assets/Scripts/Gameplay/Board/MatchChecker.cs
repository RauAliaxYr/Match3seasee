using System.Collections.Generic;
using UnityEngine;

public static class MatchChecker
{
    public static List<List<Vector2Int>> FindAllMatches(BoardData boardData)
    {
        var matches = new List<List<Vector2Int>>();

        int width = boardData.Width;
        int height = boardData.Height;

        // Horizontally
        for (int y = 0; y < height; y++)
        {
            int matchStart = 0;
            for (int x = 1; x <= width; x++)
            {
                bool endOfLine = x == width || !IsSameType(boardData, x, y, x - 1, y);
                if (endOfLine)
                {
                    int count = x - matchStart;
                    if (count >= 3)
                    {
                        var match = new List<Vector2Int>();
                        for (int i = matchStart; i < x; i++)
                            match.Add(new Vector2Int(i, y));
                        matches.Add(match);
                    }

                    matchStart = x;
                }
            }
        }

        // Vertically
        for (int x = 0; x < width; x++)
        {
            int matchStart = 0;
            for (int y = 1; y <= height; y++)
            {
                bool endOfLine = y == height || !IsSameType(boardData, x, y, x, y - 1);
                if (endOfLine)
                {
                    int count = y - matchStart;
                    if (count >= 3)
                    {
                        var match = new List<Vector2Int>();
                        for (int i = matchStart; i < y; i++)
                            match.Add(new Vector2Int(x, i));
                        matches.Add(match);
                    }

                    matchStart = y;
                }
            }
        }

        return matches;
    }

    private static bool IsSameType(BoardData board, int x1, int y1, int x2, int y2)
    {
        if (!InBounds(board, x1, y1) || !InBounds(board, x2, y2)) return false;
        var a = board.GetCell(x1, y1);
        var b = board.GetCell(x2, y2);
        return !a.IsBlocked && !b.IsBlocked && a.Type == b.Type;
    }

    private static bool InBounds(BoardData board, int x, int y)
    {
        return x >= 0 && y >= 0 && x < board.Width && y < board.Height;
    }
}