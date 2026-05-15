namespace Tetris;

public class StateSnapshot
{
    /// <summary> Current board state, indexing starts from the top left corner </summary>
    public readonly bool[,] board = new bool[GameState.Width, GameState.Height];

    /// <summary> Current Piece to place</summary>
    public GamePiece? Piece { get; set; }

    /// <summary> Current Score </summary>
    public int Score { get; set; }
}
