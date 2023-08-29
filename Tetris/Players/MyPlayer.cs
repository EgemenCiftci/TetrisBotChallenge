namespace Tetris.Players;

public class MyPlayer : IPlayer
{

    public void Init() { }

    public Command Step(StateSnapshot snapshot)
    {
        GamePiece currentPiece = snapshot.piece;
        long bestScore = long.MinValue;
        Command bestMove = new(0, 0);

        if (currentPiece.type == 'I')
        {
            bestMove = new(GetDeepestX(snapshot.board), 0);
        }
        else
        {
            int width = snapshot.board.GetLength(0);

            for (int rotationCount = 0; rotationCount < currentPiece.rotations; rotationCount++)
            {
                GamePiece rotatedPiece = currentPiece.Rotate(rotationCount);

                for (int offset = 0; offset < width - rotatedPiece.width + 1; offset++)
                {
                    int[] piecelayout = rotatedPiece.GetLayout(offset);
                    bool[,] newBoard = PlacePiece(snapshot.board, piecelayout);

                    long score = CalculateScore(newBoard);

                    if (score > bestScore)
                    {
                        bestScore = score;
                        bestMove = new Command(offset, rotationCount);
                    }
                }
            }
        }

        ConsoleRenderer.Render(bestMove.offset, bestMove.rotation, snapshot);

        return bestMove;
    }

    private static long CalculateScore(bool[,] board)
    {
        return (GetClearedLines(board) * 10) +
               (GetToppedOut(board) * -1000000) +
               (GetHoleCount(board) * -8) +
               (GetMaxHeight(board) * -8) +
               (GetTotalHeight(board) * -4) +
               (GetBumpiness(board) * -3);
    }

    private static bool[,] PlacePiece(bool[,] board, int[] pieceLayout)
    {
        int width = board.GetLength(0);
        int height = board.GetLength(1);

        for (int h = 0; h < height; h++)
        {
            foreach (int index in pieceLayout)
            {
                int x = index % width;
                int y = (index / width) + h;

                if (y >= height || board[x, y])
                {
                    h--;
                    return Apply(board, pieceLayout, width, h);
                }
            }
        }

        return Apply(board, pieceLayout, width, 0);
    }

    private static bool[,] Apply(bool[,] board, int[] pieceLayout, int width, int h)
    {
        bool[,] newBoard = (bool[,])board.Clone();

        foreach (int index in pieceLayout)
        {
            int x = index % width;
            int y = (index / width) + h;

            newBoard[x, y] = true;
        }

        return newBoard;
    }

    private static int GetDeepestX(bool[,] board)
    {
        int width = board.GetLength(0);
        int height = board.GetLength(1);
        int deepestX = 0;
        int maxY = int.MinValue;

        for (int x = width - 1; x >= 0; x--)
        {
            for (int y = 0; y < height; y++)
            {
                if (board[x, y])
                {
                    if (y > maxY)
                    {
                        maxY = y;
                        deepestX = x;
                    }

                    break;
                }
                else
                {
                    if (y == height - 1)
                    {
                        if (y + 1 > maxY)
                        {
                            maxY = y + 1;
                            deepestX = x;
                        }
                    }
                }
            }
        }

        return deepestX;
    }

    /// <summary>
    /// Holes are empty spaces beneath filled cells. 
    /// More holes can restrict movement and create problems. 
    /// We can assign a negative weight to the number of holes.
    /// </summary>
    /// <param name="board"></param>
    /// <returns></returns>
    private static int GetHoleCount(bool[,] board)
    {
        int count = 0;

        for (int x = 0; x < board.GetLength(0); x++)
        {
            bool isUnder = false;

            for (int y = 0; y < board.GetLength(1); y++)
            {
                if (board[x, y])
                {
                    isUnder = true;
                }
                else
                {
                    if (isUnder)
                    {
                        count++;
                    }
                }
            }
        }

        return count;
    }

    /// <summary>
    /// Higher columns can lead to more difficult situations later. 
    /// Weights for heights could decrease as columns get higher.
    /// </summary>
    /// <param name="board"></param>
    /// <returns></returns>
    private static int GetTotalHeight(bool[,] board)
    {
        int height = board.GetLength(1);
        int totalHeight = 0;

        for (int x = 0; x < board.GetLength(0); x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (board[x, y])
                {
                    totalHeight += height - y;
                    break;
                }
            }
        }

        return totalHeight;
    }

    private static int GetMaxHeight(bool[,] board)
    {
        int height = board.GetLength(1);
        int maxHeight = 0;

        for (int x = 0; x < board.GetLength(0); x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (board[x, y])
                {
                    int currentHeight = height - y;

                    if (currentHeight > maxHeight)
                    {
                        maxHeight = currentHeight;
                    }

                    break;
                }
            }
        }

        return maxHeight;
    }

    /// <summary>
    /// This measures the difference in column heights. 
    /// A smoother playing field is generally preferable. 
    /// We can assign a negative weight to the sum of differences between adjacent column heights.
    /// </summary>
    /// <param name="board"></param>
    /// <returns></returns>
    private static int GetBumpiness(bool[,] board)
    {
        int height = board.GetLength(1);
        int sumOfDeltas = 0;
        int previousHeight = 0;

        for (int x = 0; x < board.GetLength(0); x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (board[x, y])
                {
                    int currentHeight = height - y;

                    if (x != 0)
                    {
                        sumOfDeltas += Math.Abs(currentHeight - previousHeight);
                    }

                    previousHeight = currentHeight;

                    break;
                }
                else
                {
                    previousHeight = 0;
                }
            }
        }

        return sumOfDeltas;
    }

    /// <summary>
    /// Clearing lines is essential. 
    /// We can assign a positive weight to the number of lines cleared in a move.
    /// </summary>
    /// <param name="board"></param>
    /// <returns>0, 1, 2, 3, 4</returns>
    private static int GetClearedLines(bool[,] board)
    {
        int clearedLines = 0;

        for (int y = 0; y < board.GetLength(1); y++)
        {
            clearedLines++;

            for (int x = 0; x < board.GetLength(0); x++)
            {
                if (!board[x, y])
                {
                    clearedLines--;
                    break;
                }
            }
        }

        return clearedLines;
    }

    /// <summary>
    /// Detecting if a move results in a topped-out situation (game over) is crucial. 
    /// We can assign a very negative weight to this situation.
    /// </summary>
    /// <param name="board"></param>
    /// <returns>0, 1</returns>
    private static int GetToppedOut(bool[,] board)
    {
        for (int x = 0; x < board.GetLength(0); x++)
        {
            if (board[x, 0] || board[x, 1] || board[x, 2])
            {
                return 1;
            }
        }

        return 0;
    }
}
