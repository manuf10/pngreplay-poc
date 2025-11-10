// This is in case we wish to load a screenshot dropped on top of the game's executable.
string gameSnapshotPath = string.Empty;
if (args.Length > 0)
{
    gameSnapshotPath = args[0];
}

using var game = new PNGReplayPoC.PNGReplayPoC(gameSnapshotPath);
game.Run();
