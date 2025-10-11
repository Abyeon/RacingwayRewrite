using System;
using System.Collections.Generic;
using System.Linq;
using Dalamud.Game.Command;

namespace RacingwayRewrite.Commands;

public class CommandHandler : IDisposable
{
    public readonly List<ICommand> Commands;

    public CommandHandler(Plugin plugin)
    {
        Commands = [
            new Race(plugin),
            new Door(),
            new Quit(),
            new Edit(plugin)
        ];
        
        // Sorting the local list just for viewing purposes in the About tab.
        if (Commands.Any(x => x.DisplayOrder == -1))
        {
            Commands.Sort((a, b) => string.CompareOrdinal(a.Name, b.Name));
        }
        else
        {
            Commands.Sort((a, b) => a.DisplayOrder.CompareTo(b.DisplayOrder));
        }

        foreach (var command in Commands)
        {
            Plugin.Log.Verbose($"Adding command: {command.Name}");
            Plugin.CommandManager.AddHandler($"/{command.Name.ToLower()}", new CommandInfo(OnCommand)
            {
                HelpMessage = command.Description,
                ShowInHelp = command.ShowInHelp,
                DisplayOrder = command.DisplayOrder
            });
        }
    }

    public void OnCommand(string cmd, string args)
    {
        foreach (var command in Commands)
        {
            if (string.Equals(cmd, $"/{command.Name}", StringComparison.CurrentCultureIgnoreCase))
            {
                Plugin.Log.Verbose($"Executing command: {command.Name}");
                command.Execute(cmd, args);
                return;
            }
        }
    }

    public void Dispose()
    {
        foreach (var command in Commands)
        {
            Plugin.CommandManager.RemoveHandler($"/{command.Name.ToLower()}");
            command.Dispose();
        }
        
        GC.SuppressFinalize(this);
    }
}
