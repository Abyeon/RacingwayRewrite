namespace RacingwayRewrite.Commands;

public class Door : ICommand
{
    public string Name => "RaceDoor";
    public string Description => "Moves the player to the door when in a house.";
    public bool ShowInHelp => true;
    public int DisplayOrder => 2;
    
    public void Execute(string command, string args)
    {
        Plugin.TerritoryTools.MoveToEntry();
    }
    
    public void Dispose() { }
}
