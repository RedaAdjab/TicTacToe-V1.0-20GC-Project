using UnityEngine;

public static class MinMaxAI
{
    public static Vector2Int GetBestMove(GameManager.PlayerType[,] board, GameManager.PlayerType aiPlayer)
    {
        int bestScore = int.MinValue;
        Vector2Int bestMove = new Vector2Int(-1, -1);

        // Try every possible move
        for (int col = 0; col < 3; col++)
        {
            for (int row = 0; row < 3; row++)
            {
                if (board[col, row] == GameManager.PlayerType.None)
                {
                    // Simulate move
                    board[col, row] = aiPlayer;
                    int score = Minimax(board, 0, false, aiPlayer);
                    board[col, row] = GameManager.PlayerType.None;

                    if (score > bestScore)
                    {
                        bestScore = score;
                        bestMove = new Vector2Int(col, row);
                    }
                }
            }
        }

        return bestMove;
    }

    private static int Minimax(GameManager.PlayerType[,] board, int depth, bool isMaximizing, GameManager.PlayerType aiPlayer)
    {
        GameManager.PlayerType winner = CheckWinner(board);
        if (winner == aiPlayer) return 10 - depth;
        if (winner != GameManager.PlayerType.None) return depth - 10; // Opponent wins
        if (IsBoardFull(board)) return 0;

        GameManager.PlayerType opponent = aiPlayer == GameManager.PlayerType.Cross ? GameManager.PlayerType.Circle : GameManager.PlayerType.Cross;

        int bestScore = isMaximizing ? int.MinValue : int.MaxValue;

        for (int col = 0; col < 3; col++)
        {
            for (int row = 0; row < 3; row++)
            {
                if (board[col, row] == GameManager.PlayerType.None)
                {
                    board[col, row] = isMaximizing ? aiPlayer : opponent;
                    int score = Minimax(board, depth + 1, !isMaximizing, aiPlayer);
                    board[col, row] = GameManager.PlayerType.None;

                    if (isMaximizing)
                        bestScore = Mathf.Max(score, bestScore);
                    else
                        bestScore = Mathf.Min(score, bestScore);
                }
            }
        }

        return bestScore;
    }

    private static bool IsBoardFull(GameManager.PlayerType[,] board)
    {
        for (int col = 0; col < 3; col++)
            for (int row = 0; row < 3; row++)
                if (board[col, row] == GameManager.PlayerType.None)
                    return false;
        return true;
    }

    private static GameManager.PlayerType CheckWinner(GameManager.PlayerType[,] board)
    {
        // Rows
        for (int row = 0; row < 3; row++)
        {
            if (board[0, row] != GameManager.PlayerType.None &&
                board[0, row] == board[1, row] &&
                board[1, row] == board[2, row])
                return board[0, row];
        }

        // Columns
        for (int col = 0; col < 3; col++)
        {
            if (board[col, 0] != GameManager.PlayerType.None &&
                board[col, 0] == board[col, 1] &&
                board[col, 1] == board[col, 2])
                return board[col, 0];
        }

        // Diagonals
        if (board[0, 0] != GameManager.PlayerType.None &&
            board[0, 0] == board[1, 1] &&
            board[1, 1] == board[2, 2])
            return board[0, 0];

        if (board[2, 0] != GameManager.PlayerType.None &&
            board[2, 0] == board[1, 1] &&
            board[1, 1] == board[0, 2])
            return board[2, 0];

        return GameManager.PlayerType.None;
    }
}
