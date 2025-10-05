using System;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.Game;
using RacingwayRewrite.Race;

namespace RacingwayRewrite.Utils;

public class Chat : IDisposable
{
    internal readonly Plugin Plugin;
    internal readonly IChatGui ChatGui;
    
    private const string tag = "Race";
    
    public const uint OpenRacingwayId = 0;
    public const uint OpenLogId = 1;
    public const uint MoveDoorId = 2;
    
    private DalamudLinkPayload OpenRacingway { get; set;}
    private DalamudLinkPayload OpenLog { get; set;}
    private DalamudLinkPayload MoveDoor { get; set;}
    
    public enum Colors : ushort
    {
        Tag = 57,
        Print = 2,
        ErrorGlow = 5,
        Error = 17,
        Warning = 32
    }
    
    public Chat(Plugin plugin, IChatGui chatGui)
    {
        Plugin = plugin;
        ChatGui = chatGui;
        
        OpenRacingway = Plugin.ChatGui.AddChatLinkHandler(OpenRacingwayId, OnOpenRacingway);
        OpenLog = Plugin.ChatGui.AddChatLinkHandler(OpenLogId, OnOpenLog);
        MoveDoor = Plugin.ChatGui.AddChatLinkHandler(MoveDoorId, OnMoveDoor);
    }

    private void OnOpenRacingway(uint id, SeString message)
    {
        Plugin.Log.Verbose($"OpenRacingway payload clicked: {id}, {message}");
        Plugin.ToggleMainUI();
    }

    private void OnOpenLog(uint id, SeString message)
    {
        Plugin.Log.Verbose($"OpenLog payload clicked: {id}, {message}");
        Plugin.CommandManager.ProcessCommand("/xllog");
    }

    private void OnMoveDoor(uint id, SeString message)
    {
        Plugin.Log.Verbose($"MoveDoor payload clicked: {id}, {message}");
        Plugin.TerritoryTools.MoveToEntry();
    }

    // public SeStringBuilder Tag()
    // {
    //     return new SeStringBuilder()
    //            .AddUiForeground((ushort)Colors.Tag)
    //            .Add(OpenRacingway)
    //            .AddText($"[{tag}]")
    //            .Add(RawPayload.LinkTerminator)
    //            .AddText(" ")
    //            .AddUiForegroundOff();
    // }

    public SeStringBuilder Tag(BitmapFontIcon? icon = null)
    {
        var builder = new SeStringBuilder();
        if (icon != null) builder = builder.AddIcon(icon.Value);
        return builder
               .AddUiForeground((ushort)Colors.Tag)
               .Add(OpenRacingway)
               .AddText($"[{tag}]")
               .Add(RawPayload.LinkTerminator)
               .AddText(" ")
               .AddUiForegroundOff();
    }

    public void TestPrintIcons()
    {
        uint[] iconValues = (uint[])Enum.GetValues(typeof(BitmapFontIcon));
        string[] iconNames = Enum.GetNames(typeof(BitmapFontIcon));
        
        var builder = new SeStringBuilder();
        
        for (int i = 0; i < iconValues.Length; i++)
        {
            if (Enum.IsDefined(typeof(BitmapFontIcon), iconValues[i]))
            {
                builder.AddIcon((BitmapFontIcon)iconValues[i])
                       .AddText(iconNames[i]);
            }
        }
        
        ChatGui.Print(builder.BuiltString);
    }

    public void Print(string message, BitmapFontIcon? icon = null)
    {
        SeString msg = Tag(icon).AddUiForeground(message, (ushort)Colors.Print).BuiltString;
        ChatGui.Print(msg);
    }
    
    public void Error(string message, BitmapFontIcon? icon = BitmapFontIcon.Warning)
    {
        SeString msg = Tag(icon)
                       .AddUiForeground(message + " ", (ushort)Colors.Error)
                       .Add(OpenLog)
                       .AddUiForeground("/xllog", (ushort)Colors.Print)
                       .Add(RawPayload.LinkTerminator).BuiltString;
        
        ChatGui.Print(msg);
    }
    
    public void Warning(string message, BitmapFontIcon? icon = BitmapFontIcon.Warning)
    {
        SeString msg = Tag(icon)
                       .AddItalicsOn()
                       .AddUiForeground(message, (ushort)Colors.Warning)
                       .AddItalicsOff().BuiltString;
        
        ChatGui.Print(msg);
    }

    public unsafe void PrintPlayer(Player player, string message, bool addDoorLink = false)
    {
        // Have to run on framework thread since we're checking for the local player
        Plugin.Framework.RunOnFrameworkThread(() =>
        {
            if (Plugin.ClientState.LocalPlayer == null) return;

            SeStringBuilder payload = Tag()
                                      .AddPlayer(player)
                                      .AddUiForeground(" " + message, (ushort)Colors.Print);

            var manager = HousingManager.Instance();
            
            if (addDoorLink && manager->IsInside())
            {
                payload = payload.AddUiForeground(" ", (ushort)Colors.Print)
                                 .Add(MoveDoor)
                                 .AddUiForeground("Return To Door", (ushort)Colors.Tag)
                                 .Add(RawPayload.LinkTerminator);
            }
            
            ChatGui.Print(payload.BuiltString);
        });
    }

    public void Dispose()
    {
        Plugin.ChatGui.RemoveChatLinkHandler(OpenRacingwayId);
        Plugin.ChatGui.RemoveChatLinkHandler(OpenLogId);
    }
}
