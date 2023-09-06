using Spectre.Console;
using System.Diagnostics;
using Tetris;
using Tetris.Players;

bool isParametersOptimization = AnsiConsole.Prompt(new SelectionPrompt<bool>().Title("Search for optimal parameter weights?").AddChoices(new[] { false, true }).UseConverter(f => f ? "Yes" : "No"));
const int roundsCount = 1000;

if (isParametersOptimization)
{
    string searchType = AnsiConsole.Prompt(new SelectionPrompt<string>().Title("Please select search type.").AddChoices(new[] { "Random", "Brute-force" }));

    switch (searchType)
    {
        default:
        case "Random":
            bool useLocalSearch = AnsiConsole.Prompt(new SelectionPrompt<bool>().Title("Use local search?").AddChoices(new[] { false, true }).UseConverter(f => f ? "Yes" : "No"));
            RunRandomSearch(useLocalSearch);
            break;
        case "Brute-force":
            RunBruteForceSearch();
            break;
    }
}
else
{
    string playerType = AnsiConsole.Prompt(new SelectionPrompt<string>().Title("Please select player.").AddChoices(new[] { "Console", "Random", "My" }));
    IPlayer player = playerType switch
    {
        "Random" => new RandomPlayer(),
        "My" => new MyPlayer(),
        _ => new ConsolePlayer(),
    };
    bool isTournament = AnsiConsole.Prompt(new SelectionPrompt<bool>().Title("Run tournament?").AddChoices(new[] { false, true }).UseConverter(f => f ? "Yes" : "No"));

    if (isTournament)
    {
        RunTournament(player);
    }
    else
    {
        RunSingleGame(player);
    }
}

static void RunSingleGame(IPlayer player)
{
    int score = Run(0, 100000, player);
    AnsiConsole.WriteLine($"Final Score: {score}");
    _ = Console.ReadKey();
}

static void RunTournament(IPlayer player)
{
    if (player is not ConsolePlayer)
    {
        ConsoleRenderer.Render = ConsoleRenderer.NullRender;
    }

    int[] rounds = Enumerable.Range(0, roundsCount).ToArray();
    int score = 0;
    AnsiConsole.Progress().Columns(new ProgressColumn[]
    {
        new TaskDescriptionColumn(),
        new ProgressBarColumn(),
        new PercentageColumn(),
        new RemainingTimeColumn(),
        new SpinnerColumn()
    }).Start(ctx =>
    {
        ProgressTask task = ctx.AddTask("Running tournament", maxValue: roundsCount);

        score = rounds.AsParallel().Sum(round =>
        {
            int result = Run(round, 100000, player);
            task.Increment(1d);
            return result;
        });
    });

    AnsiConsole.WriteLine($"Final Score: {score}");
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
    catch (Exception)
    {
        //Console.WriteLine($"End of Game: {ex.Message}");
    }

    return state.Score;
}

static void RunRandomSearch(bool useLocalSearch)
{
    Stopwatch sw = new();
    sw.Start();

    ConsoleRenderer.Render = ConsoleRenderer.NullRender;

    double bestScore = 1450;
    int[] bestParameters = Array.Empty<int>();
    object lockObject = new();
    int numberOfTrials = 1000;

    AnsiConsole.Progress().Columns(new ProgressColumn[]
    {
        new TaskDescriptionColumn(),
        new ProgressBarColumn(),
        new PercentageColumn(),
        new RemainingTimeColumn(),
        new SpinnerColumn()
    }).Start(ctx =>
    {
        ProgressTask task = ctx.AddTask("Random search", maxValue: numberOfTrials);

        _ = Parallel.For(0, numberOfTrials, t =>
        {
            Random r = new();
            int[] parameters = new[] { r.Next(0, 10), r.Next(0, 10), r.Next(0, 10), r.Next(0, 10) };
            int[] scores = CalculateScorePerRound(parameters[0], parameters[1], parameters[2], parameters[3]);
            double score = scores.Average();

            lock (lockObject)
            {
                if (score >= bestScore)
                {
                    bestScore = score;
                    bestParameters = parameters;
                    AnsiConsole.WriteLine($"{bestScore} {string.Join(' ', bestParameters)}");
                }
            }

            task.Increment(1d);
        });

        if (useLocalSearch)
        {
            LocalSearch(bestScore, bestParameters, sw, ctx);
        }
        else
        {
            sw.Stop();
            AnsiConsole.WriteLine($"Best Average Score: {bestScore} Parameters: {string.Join(',', bestParameters)} Duration: {sw.Elapsed}");
            _ = Console.ReadKey();
        }
    });
}

static void RunBruteForceSearch()
{
    Stopwatch sw = new();
    sw.Start();

    ConsoleRenderer.Render = ConsoleRenderer.NullRender;

    double bestScore = 1500;
    int[] bestParameters = Array.Empty<int>();
    object lockObject = new();

    AnsiConsole.Progress().Columns(new ProgressColumn[]
    {
        new TaskDescriptionColumn(),
        new ProgressBarColumn(),
        new PercentageColumn(),
        new RemainingTimeColumn(),
        new SpinnerColumn()
    }).Start(ctx =>
    {
        ProgressTask task = ctx.AddTask("Brute-force", maxValue: 10 * 10 * 10 * 10);

        _ = Parallel.For(885, 895, a =>
        {
            for (int b = 915; b < 925; b++)
            {
                for (int c = 885; c < 895; c++)
                {
                    for (int d = 395; d < 405; d++)
                    {
                        int[] parameters = new[] { a, b, c, d };
                        int[] scores = CalculateScorePerRound(parameters[0], parameters[1], parameters[2], parameters[3]);
                        double score = scores.Average();

                        lock (lockObject)
                        {
                            if (score >= bestScore)
                            {
                                bestScore = score;
                                bestParameters = parameters;
                                AnsiConsole.WriteLine($"{bestScore} {string.Join(' ', bestParameters)}");
                            }
                        }

                        task.Increment(1d);
                    }
                }
            }
        });

        sw.Stop();
        AnsiConsole.WriteLine($"Best Average Score: {bestScore} Parameters: {string.Join(',', bestParameters)} Duration: {sw.Elapsed}");
        _ = Console.ReadKey();
    });
}

static int[] CalculateScorePerRound(int a, int b, int c, int d)
{
    MyPlayer player = new()
    {
        aggregateHeightWeight = a,
        clearedLinesWeight = b,
        holesWeight = c,
        bumpinessWeight = d
    };

    int[] rounds = Enumerable.Range(0, roundsCount).ToArray();
    int[] scores = new int[rounds.Length];

    for (int i = 0; i < rounds.Length; i++)
    {
        scores[i] = Run(rounds[i], 100000, player);
    }

    return scores;
}

static void LocalSearch(double score, int[] parameters, Stopwatch sw, ProgressContext ctx)
{
    ProgressTask task = ctx.AddTask("Local search", maxValue: 3 * 3 * 3 * 3);

    double bestScore = score;
    int[] bestParameters = parameters;
    object lockObject = new();

    _ = Parallel.For(parameters[0] - 1, parameters[0] + 2, a =>
    {
        for (int b = parameters[1] - 1; b < parameters[1] + 2; b++)
        {
            for (int c = parameters[2] - 1; c < parameters[2] + 2; c++)
            {
                for (int d = parameters[3] - 1; d < parameters[3] + 2; d++)
                {
                    int[] par = new[] { a, b, c, d };
                    int[] scores = CalculateScorePerRound(par[0], par[1], par[2], par[3]);
                    double score = scores.Average();

                    lock (lockObject)
                    {
                        if (score > bestScore)
                        {
                            bestScore = score;
                            bestParameters = par;
                            AnsiConsole.WriteLine($"{bestScore} {string.Join(' ', bestParameters)}");
                        }
                    }

                    task.Increment(1d);
                }
            }
        }
    });

    sw.Stop();
    AnsiConsole.WriteLine($"Best Average Score: {bestScore} Parameters: {string.Join(',', bestParameters)} Duration: {sw.Elapsed}");
    _ = Console.ReadKey();
}

//10000 Rounds:
//1498,2739 89,92,89,40
//1498,2739 94,94,88,41
//1498,1683 85,88,85,38