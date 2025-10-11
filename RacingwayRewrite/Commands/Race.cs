using System;

namespace RacingwayRewrite.Commands;

public class Race(Plugin plugin) : ICommand
{
    public string Name => "RaceRewrite";
    public string Description => "Toggles the main Racingway UI";
    public bool ShowInHelp => true;
    public int DisplayOrder => 0;
    
    public void Execute(string command, string args)
    {
        plugin.ToggleMainUI();
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}
