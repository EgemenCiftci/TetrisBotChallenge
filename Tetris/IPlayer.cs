namespace Tetris;

public interface IPlayer
{
    void Init();
    Command Step(StateSnapshot snapshot);
}