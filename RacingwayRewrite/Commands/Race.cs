namespace RacingwayRewrite.Commands;

public class Race(Plugin Plugin) : ICommand
{
    public string Name => "RaceRewrite";
    public string Description => "Opens the main Racingway UI";
    public bool ShowInHelp => true;
    public int DisplayOrder => 0;
    
    public void Execute(string command, string args)
    {
        Plugin.ToggleMainUI();
    }

    public void Dispose() { }
}
