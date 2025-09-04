using System;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Dalamud.Plugin.Services;
using RacingwayRewrite.Race;

namespace RacingwayRewrite.Utils;

public class Chat : IDisposable
{
    internal readonly Plugin Plugin;
    internal readonly IChatGui ChatGui;
    
    private const string tag = "[RACE] ";
    
    public const uint OpenRacingwayId = 0;
    
    private DalamudLinkPayload OpenRacingway { get; set;}
    
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
        
        OpenRacingway = Plugin.ChatGui.AddChatLinkHandler(OpenRacingwayId, OnOpenRacingway);
    }

    private void OnOpenRacingway(uint id, SeString message)
    {
        Plugin.Log.Verbose($"OpenRacingway payload clicked: {id}, {message}");
        Plugin.ToggleMainUI();
    }

    public SeStringBuilder Tag()
    {
        return new SeStringBuilder()
               .AddUiForeground((ushort)Colors.Tag)
               .Add(OpenRacingway)
               .AddText(tag)
               .Add(RawPayload.LinkTerminator)
               .AddUiForegroundOff();
    }
    
    public SeString ColorMessage(string message, Colors color)
    {
        return Tag()
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
        
        SeString payload = Tag()
                           .AddPlayer(player) 
                           .AddUiForeground(" " + message, (ushort)Colors.Print) 
                           .BuiltString;
        
        Print(payload);
    }

    public void Print(SeString message)
    {
        ChatGui.Print(message);
    }

    public void Dispose()
    {
        Plugin.ChatGui.RemoveChatLinkHandler(OpenRacingwayId);
    }
}
