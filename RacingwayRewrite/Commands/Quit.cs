namespace RacingwayRewrite.Commands;

public class Quit() : ICommand
{
    public string Name => "RaceQuit";
    public string Description => "Kicks the player out of their current race.";
    public bool ShowInHelp => true;
    public int DisplayOrder => 1;
    public void Execute(string command, string args)
    {
        if (Plugin.ClientState.LocalPlayer == null) return;
            
        Plugin.RaceManager.Players[Plugin.ClientState.LocalPlayer.EntityId].State.SilentFail();
        Plugin.Chat.Print("You have quit the race.");
    }
    
    public void Dispose() { }
}
