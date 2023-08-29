using Tetris;
using Tetris.Players;

bool isHumanPlayer = false;
bool isTournament = false;

IPlayer player;

if (isHumanPlayer)
{
    player = new ConsolePlayer();
}
else
{
    player = new MyPlayer();
}

if (isTournament)
{
    RunTournament(player);
}
else
{
    RunSingleGame(player);
}

static void RunSingleGame(IPlayer player)
{
    int score = Run(0, 100000, player);
    Console.WriteLine($"Final Score: {score}");
    _ = Console.ReadKey();
}

static void RunTournament(IPlayer player)
{
    // Here will be a set of random seeds, right now this is a test placeholder
    int[] rounds = new[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };

    if ((player as ConsolePlayer) == null)
    {
        ConsoleRenderer.Render = ConsoleRenderer.NullRender;
    }

    int score = 0;

    foreach (int round in rounds)
    {
        score += Run(round, 100000, player);
    }

    Console.WriteLine($"Final Score: {score}");
    _ = Console.ReadKey();
}

static int Run(int seed, int steps, IPlayer player)
{
    GameState state = new(seed);
    StateSnapshot snapshot = new();

    try
    {
        player.Init();

        for (int i = 0; i < steps; i++)
        {
            state.CopyTo(snapshot);
            state.Apply(player.Step(snapshot));
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"End of Game: {ex.Message}");
    }

    return state.Score;
}
