using System.Collections.Generic;
using UnityEngine;

public static class SearchBlockWinAI
{
    public static Vector2Int GetMove(GameManager.PlayerType[,] playerTypeArray, GameManager.PlayerType aiType)
    {
        // 1/5 chance to play randomly instead of logically
        if (Random.Range(0, 5) == 0) // 1 out of 5
        {
            return GetRandomMove(playerTypeArray);
        }

        GameManager.PlayerType opponent = aiType == GameManager.PlayerType.Cross ? GameManager.PlayerType.Circle : GameManager.PlayerType.Cross;

        // Step 1: Try to win
        Vector2Int? winMove = FindWinningMove(playerTypeArray, aiType);
        if (winMove.HasValue)
            return winMove.Value;

        // Step 2: Block opponent
        Vector2Int? blockMove = FindWinningMove(playerTypeArray, opponent);
        if (blockMove.HasValue)
            return blockMove.Value;

        // Step 3: Prefer center
        if (playerTypeArray[1, 1] == GameManager.PlayerType.None)
            return new Vector2Int(1, 1);

        // Step 4: Prefer corners
        Vector2Int[] corners = { new Vector2Int(0, 0), new Vector2Int(2, 0),
                                 new Vector2Int(0, 2), new Vector2Int(2, 2) };
        foreach (var c in corners)
        {
            if (playerTypeArray[c.x, c.y] == GameManager.PlayerType.None)
                return c;
        }

        // Step 5: Pick first available side
        for (int x = 0; x < 3; x++)
        {
            for (int y = 0; y < 3; y++)
            {
                if (playerTypeArray[x, y] == GameManager.PlayerType.None)
                    return new Vector2Int(x, y);
            }
        }

        return new Vector2Int(-1, -1); // no moves left
    }

    private static Vector2Int? FindWinningMove(GameManager.PlayerType[,] board, GameManager.PlayerType type)
    {
        // Rows
        for (int y = 0; y < 3; y++)
        {
            int count = 0; int emptyX = -1;
            for (int x = 0; x < 3; x++)
            {
                if (board[x, y] == type) count++;
                if (board[x, y] == GameManager.PlayerType.None) emptyX = x;
            }
            if (count == 2 && emptyX != -1)
                return new Vector2Int(emptyX, y);
        }

        // Columns
        for (int x = 0; x < 3; x++)
        {
            int count = 0; int emptyY = -1;
            for (int y = 0; y < 3; y++)
            {
                if (board[x, y] == type) count++;
                if (board[x, y] == GameManager.PlayerType.None) emptyY = y;
            }
            if (count == 2 && emptyY != -1)
                return new Vector2Int(x, emptyY);
        }

        // Diagonal 1
        {
            int count = 0; Vector2Int empty = new Vector2Int(-1, -1);
            for (int i = 0; i < 3; i++)
            {
                if (board[i, i] == type) count++;
                if (board[i, i] == GameManager.PlayerType.None) empty = new Vector2Int(i, i);
            }
            if (count == 2 && empty.x != -1)
                return empty;
        }

        // Diagonal 2
        {
            int count = 0; Vector2Int empty = new Vector2Int(-1, -1);
            for (int i = 0; i < 3; i++)
            {
                if (board[i, 2 - i] == type) count++;
                if (board[i, 2 - i] == GameManager.PlayerType.None) empty = new Vector2Int(i, 2 - i);
            }
            if (count == 2 && empty.x != -1)
                return empty;
        }

        return null;
    }

    private static Vector2Int GetRandomMove(GameManager.PlayerType[,] board)
    {
        var emptySpots = new List<Vector2Int>();
        for (int col = 0; col < 3; col++)
        {
            for (int row = 0; row < 3; row++)
            {
                if (board[col, row] == GameManager.PlayerType.None)
                    emptySpots.Add(new Vector2Int(col, row));
            }
        }

        if (emptySpots.Count == 0)
            return new Vector2Int(-1, -1); // No moves available

        return emptySpots[Random.Range(0, emptySpots.Count)];
    }
}
