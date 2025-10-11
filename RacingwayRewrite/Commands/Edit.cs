using System;

namespace RacingwayRewrite.Commands;

public class Edit(Plugin plugin) : ICommand
{
    public string Name => "RaceEdit";
    public string Description => "Toggles the editor window.";
    public bool ShowInHelp => true;
    public int DisplayOrder => 3;
    
    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
    
    public void Execute(string command, string args)
    {
        plugin.ToggleEditUI();
    }
}
