public static class MatchChecker
{
    public static bool HasAnyMatch(BoardData board)
    {
        return HasHorizontalMatch(board) || HasVerticalMatch(board);
    }

    private static bool HasHorizontalMatch(BoardData board)
    {
        for (int y = 0; y < board.Height; y++)
        {
            int matchLength = 1;
            TileType? prev = null;

            for (int x = 0; x < board.Width; x++)
            {
                var cell = board.GetCell(x, y);
                if (cell.IsBlocked || !cell.Type.HasValue)
                {
                    matchLength = 1;
                    prev = null;
                    continue;
                }

                if (cell.Type == prev)
                {
                    matchLength++;
                    if (matchLength >= 3)
                        return true;
                }
                else
                {
                    matchLength = 1;
                    prev = cell.Type;
                }
            }
        }
        return false;
    }

    private static bool HasVerticalMatch(BoardData board)
    {
        for (int x = 0; x < board.Width; x++)
        {
            int matchLength = 1;
            TileType? prev = null;

            for (int y = 0; y < board.Height; y++)
            {
                var cell = board.GetCell(x, y);
                if (cell.IsBlocked || !cell.Type.HasValue)
                {
                    matchLength = 1;
                    prev = null;
                    continue;
                }

                if (cell.Type == prev)
                {
                    matchLength++;
                    if (matchLength >= 3)
                        return true;
                }
                else
                {
                    matchLength = 1;
                    prev = cell.Type;
                }
            }
        }
        return false;
    }
}
