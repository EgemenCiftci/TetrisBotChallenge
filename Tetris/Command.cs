namespace Tetris;

public class Command
{
    /// <summary> Horizontal offset </summary>
    public readonly int offset;
    /// <summary> Index of rotation </summary>
    public readonly int rotation;

    public Command(int offset, int rotation)
    {
        this.offset = offset;
        this.rotation = rotation;
    }
}
