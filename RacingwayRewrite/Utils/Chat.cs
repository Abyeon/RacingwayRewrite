using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Plugin.Services;
using RacingwayRewrite.Race;

namespace RacingwayRewrite.Utils;

public class Chat
{
    internal readonly Plugin Plugin;
    internal readonly IChatGui ChatGui;
    
    private const string tag = "[RACE] ";
    
    public enum Colors : ushort
    {
        Tag = 57,
        Print = 2,
        Error = 16,
        Warning = 71
    }

    public Chat(Plugin plugin, IChatGui chatGui)
    {
        Plugin = plugin;
        ChatGui = chatGui;
    }

    public static SeStringBuilder Tag()
    {
        return new SeStringBuilder()
            .AddUiForeground(tag, (ushort)Colors.Tag);
    }
    
    public static SeString ColorMessage(string message, Colors color)
    {
        return new SeStringBuilder()
                .AddUiForeground(tag, (ushort)Colors.Tag)
                .AddUiForeground(message, (ushort)color)
                .BuiltString;
    }

    public void Print(string message)
    {
        ChatGui.Print(ColorMessage(message, Colors.Print));
    }
    
    public void Error(string message)
    {
        ChatGui.Print(ColorMessage(message, Colors.Error));
    }
    
    public void Warning(string message)
    {
        ChatGui.Print(ColorMessage(message, Colors.Warning));
    }

    public void PrintPlayer(Player player, string message)
    {
        if (Plugin.ClientState.LocalPlayer == null) return; 
        if (player.Id == Plugin.ClientState.LocalPlayer.EntityId) return;
        
        SeString payload = Tag()
                           .AddPlayer(player) 
                           .AddUiForeground(message, (ushort)Colors.Print) 
                           .BuiltString;
        
        Print(payload);
    }

    public void Print(SeString message)
    {
        ChatGui.Print(message);
    }
}
