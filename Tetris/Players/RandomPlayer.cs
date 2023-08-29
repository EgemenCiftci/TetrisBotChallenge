namespace Tetris.Players;

public class RandomPlayer : IPlayer
{
    private readonly Random random = new();

    public void Init() { }

    public Command Step(StateSnapshot snapshot)
    {
        int rotation = random.Next(snapshot.piece.rotations);
        GamePiece piece = snapshot.piece.Rotate(rotation);
        int offset = random.Next(GameState.Width - piece.width + 1);

        ConsoleRenderer.Render(offset, rotation, snapshot);

        return new Command(offset, rotation);
    }
}
