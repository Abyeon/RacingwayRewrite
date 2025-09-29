using System;
using Dalamud.Game.Command;
using Dalamud.IoC;
using Dalamud.Plugin;
using System.IO;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Services;
using Pictomancy;
using RacingwayRewrite.Race;
using RacingwayRewrite.Storage;
using RacingwayRewrite.Utils;
using RacingwayRewrite.Windows;

namespace RacingwayRewrite;

public sealed class Plugin : IDalamudPlugin
{
    [PluginService] internal static IDalamudPluginInterface PluginInterface { get; private set; } = null!;
    [PluginService] internal static ITextureProvider TextureProvider { get; private set; } = null!;
    [PluginService] internal static ICommandManager CommandManager { get; private set; } = null!;
    [PluginService] internal static IClientState ClientState { get; private set; } = null!;
    [PluginService] internal static IDataManager DataManager { get; private set; } = null!;
    [PluginService] internal static IPluginLog Log { get; private set; } = null!;
    [PluginService] internal static IChatGui ChatGui { get; private set; } = null!;
    [PluginService] internal static IFramework Framework { get; private set; } = null!;
    [PluginService] internal static IObjectTable ObjectTable { get; private set; } = null!;
    
    internal LocalDatabase? Storage { get; private set; }
    internal static RaceManager RaceManager { get; private set; } = null!;
    internal static VfxManager VfxManager { get; private set; } = null!;
    internal static Chat Chat { get; private set; } = null!;

    private const string CommandName = "/racerewrite";

    public static Configuration Configuration { get; set; } = null!;

    public readonly WindowSystem WindowSystem = new("RacingwayRewrite");
    private ConfigWindow ConfigWindow { get; init; }
    private MainWindow MainWindow { get; init; }
    private EditWindow EditWindow { get; init; }
    private Overlay Overlay { get; init; } 

    public Plugin()
    {
        Configuration = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
        Chat = new Chat(this, ChatGui);

        try
        {
            Storage = new(this, $"{PluginInterface.GetPluginConfigDirectory()}\\data.db");
        }
        catch (Exception e)
        {
            Chat.Error(e.Message);
            Log.Error(e.ToString());
        }
        
        PictoService.Initialize(PluginInterface);
        RaceManager = new RaceManager(this, Framework, ObjectTable, ClientState);
        VfxManager = new VfxManager(Framework);
        
        ConfigWindow = new ConfigWindow(this);
        MainWindow = new MainWindow(this);
        EditWindow = new EditWindow(this);
        Overlay = new Overlay(this);

        WindowSystem.AddWindow(ConfigWindow);
        WindowSystem.AddWindow(MainWindow);
        WindowSystem.AddWindow(EditWindow);
        WindowSystem.AddWindow(Overlay);
        
        Overlay.IsOpen = true;

        CommandManager.AddHandler(CommandName, new CommandInfo(OnCommand)
        {
            HelpMessage = "A useful message to display in /xlhelp"
        });

        PluginInterface.UiBuilder.Draw += DrawUI;
        
        // Register plugin installer buttons
        PluginInterface.UiBuilder.OpenConfigUi += ToggleConfigUI;
        PluginInterface.UiBuilder.OpenMainUi += ToggleMainUI;
    }

    public void Dispose()
    {
        PictoService.Dispose();
        RaceManager.Dispose();
        VfxManager.Dispose();
        Chat.Dispose();
        Storage?.Dispose();

        WindowSystem.RemoveAllWindows();

        ConfigWindow.Dispose();
        MainWindow.Dispose();
        Overlay.Dispose();

        CommandManager.RemoveHandler(CommandName);
    }

    private void OnCommand(string command, string args)
    {
        // In response to the slash command, toggle the display status of our main ui
        ToggleMainUI();
    }
    
    private void DrawUI() => WindowSystem.Draw();

    public void ToggleConfigUI() => ConfigWindow.Toggle();
    public void ToggleMainUI() => MainWindow.Toggle();
    public void ToggleEditUI() => EditWindow.Toggle();
}
