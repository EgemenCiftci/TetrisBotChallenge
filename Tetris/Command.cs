namespace Tetris;

public class Command(int offset, int rotation)
{
    /// <summary> Horizontal offset </summary>
    public readonly int offset = offset;
    /// <summary> Index of rotation </summary>
    public readonly int rotation = rotation;
}
