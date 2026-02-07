using System;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Services;
using Pictomancy;
using RacingwayRewrite.Commands;
using RacingwayRewrite.Race;
using RacingwayRewrite.Race.Territory;
using RacingwayRewrite.Storage;
using RacingwayRewrite.Utils;
using RacingwayRewrite.Utils.Interop;
using RacingwayRewrite.Utils.Interop.Structs;
using RacingwayRewrite.Utils.Props;
using RacingwayRewrite.Utils.Vfx;
using RacingwayRewrite.Windows;

namespace RacingwayRewrite;

public sealed class Plugin : IDalamudPlugin
{
    [PluginService] internal static IDalamudPluginInterface PluginInterface { get; private set; } = null!;
    [PluginService] internal static ICommandManager CommandManager { get; private set; } = null!;
    [PluginService] internal static IClientState ClientState { get; private set; } = null!;
    [PluginService] internal static IDataManager DataManager { get; private set; } = null!;
    [PluginService] internal static IPluginLog Log { get; private set; } = null!;
    [PluginService] internal static IChatGui ChatGui { get; private set; } = null!;
    [PluginService] internal static IFramework Framework { get; private set; } = null!;
    [PluginService] internal static IObjectTable ObjectTable { get; private set; } = null!;
    [PluginService] internal static ITargetManager TargetManager { get; private set; } = null!;
    [PluginService] internal static IGameInteropProvider GameInteropProvider { get; private set; } = null!;
    [PluginService] internal static ISigScanner SigScanner { get; private set; } = null!;

    internal LocalDatabase? Storage { get; private set; }
    internal static CommandHandler CommandHandler { get; private set; } = null!;
    internal static TerritoryTools TerritoryTools { get; private set; } = null!;
    internal static RaceManager RaceManager { get; private set; } = null!;
    internal static BgObjectFunctions BgObjectFunctions { get; private set; } = null!;
    internal static PropManager PropManager { get; private set; } = null!;
    internal static VfxFunctions VfxFunctions { get; private set; } = null!;
    internal static VfxManager VfxManager { get; private set; } = null!;
    internal static FontManager FontManager { get; private set; } = null!;
    internal static Chat Chat { get; private set; } = null!;
    internal static LifestreamIpcHandler LifestreamIpcHandler { get; private set; } = null!;

    public static Configuration Configuration { get; set; } = null!;

    public readonly WindowSystem WindowSystem = new("RacingwayRewrite");
    internal MainWindow MainWindow { get; init; }
    internal EditWindow EditWindow { get; init; }
    private TimerWindow TimerWindow { get; init; }
    private Overlay Overlay { get; init; } 

    public Plugin()
    {
        Configuration = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
        Chat = new Chat(this, ChatGui);

        try
        {
            Storage = new LocalDatabase(this, $"{PluginInterface.GetPluginConfigDirectory()}\\data.db");
        }
        catch (Exception e)
        {
            Chat.Error(e.Message);
            Log.Error(e.ToString());
        }
        
        LifestreamIpcHandler = new LifestreamIpcHandler(PluginInterface);
        PictoService.Initialize(PluginInterface);
        TerritoryTools = new TerritoryTools(this);
        RaceManager = new RaceManager(this, Framework, ObjectTable, ClientState);

        BgObjectFunctions = new BgObjectFunctions();
        PropManager = new PropManager(ClientState);
        VfxFunctions = new VfxFunctions();
        VfxManager = new VfxManager(ClientState, Framework);
        FontManager = new FontManager();
        
        MainWindow = new MainWindow(this);
        EditWindow = new EditWindow(this);
        TimerWindow = new TimerWindow(this);
        Overlay = new Overlay(this);
        
        WindowSystem.AddWindow(MainWindow);
        WindowSystem.AddWindow(EditWindow);
        WindowSystem.AddWindow(TimerWindow);
        WindowSystem.AddWindow(Overlay);

        if (Configuration is { DebugMode: true, OpenWindowsOnStartup: true })
        {
            EditWindow.IsOpen = true;
            MainWindow.IsOpen = true;
            TimerWindow.IsOpen = true;
            //TimerWindow.IsOpen = true;
        }
        
        ShowHideOverlay();

        CommandHandler = new CommandHandler(this);
        
        PluginInterface.UiBuilder.Draw += DrawUI;
        
        // Register plugin installer buttons
        PluginInterface.UiBuilder.OpenConfigUi += OpenConfigUI;
        PluginInterface.UiBuilder.OpenMainUi += ToggleMainUI;
    }

    public void Dispose()
    {
        PictoService.Dispose();
        RaceManager.Dispose();
        PropManager.Dispose();
        VfxManager.Dispose();
        FontManager.Dispose();
        Chat.Dispose();

        WindowSystem.RemoveAllWindows();
        
        MainWindow.Dispose();
        EditWindow.Dispose();
        Overlay.Dispose();
        
        CommandHandler.Dispose();
        Storage?.Dispose();
    }
    
    private void DrawUI() => WindowSystem.Draw();

    public void ShowHideOverlay()
    {
        Overlay.IsOpen = Configuration.ShowOverlay;
        //TimerWindow.IsOpen = true;
    }

    public void OpenConfigUI() => MainWindow.SelectTab("Settings");
    public void ToggleMainUI() => MainWindow.Toggle();
    public void ToggleEditUI() => EditWindow.Toggle();
    public void ToggleTimerWindow() => TimerWindow.Toggle();
}
