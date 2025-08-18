using Dalamud.Game.Command;
using Dalamud.IoC;
using Dalamud.Plugin;
using System.IO;
using System.Numerics;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Services;
using Pictomancy;
using RacingwayRewrite.Race;
using RacingwayRewrite.Race.Collision;
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
    
    public static RaceManager RaceManager { get; private set; } = null!;
    public static Chat Chat { get; private set; } = null!;

    private const string CommandName = "/racerewrite";

    public Configuration Configuration { get; init; }

    public readonly WindowSystem WindowSystem = new("RacingwayRewrite");
    private ConfigWindow ConfigWindow { get; init; }
    private MainWindow MainWindow { get; init; }
    private Overlay Overlay { get; init; } 

    public Plugin()
    {
        Configuration = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();

        // You might normally want to embed resources and load them from the manifest stream
        var goatImagePath = Path.Combine(PluginInterface.AssemblyLocation.Directory?.FullName!, "goat.png");
        
        PictoService.Initialize(PluginInterface);
        RaceManager = new RaceManager(this, Framework, ObjectTable, ClientState);
        Chat = new Chat(this, ChatGui);

        ConfigWindow = new ConfigWindow(this);
        MainWindow = new MainWindow(this, goatImagePath);
        Overlay = new Overlay(this);

        WindowSystem.AddWindow(ConfigWindow);
        WindowSystem.AddWindow(MainWindow);
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
        
        // Announce I exist!
        Chat.Print("Racingway has loaded.");
        Chat.Error("Racingway ran into a fatal error!");
        Chat.Warning("Careful! Too many triggers may cause issues!");
    }

    public void Dispose()
    {
        PictoService.Dispose();
        
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

        if (ClientState.LocalPlayer != null)
        {
            RaceManager.Cubes.Add(new Cube(ClientState.LocalPlayer.Position, Vector3.One, Vector3.One));
        }
    }
    
    private void DrawUI() => WindowSystem.Draw();

    public void ToggleConfigUI() => ConfigWindow.Toggle();
    public void ToggleMainUI() => MainWindow.Toggle();
}
