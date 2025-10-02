using Dalamud.Plugin;
using Dalamud.Plugin.Ipc;

namespace RacingwayRewrite.Utils;

public class LifestreamIpcHandler(IDalamudPluginInterface pluginInterface)
{
    public readonly ICallGateSubscriber<string, object> ExecuteCommand = pluginInterface.GetIpcSubscriber<string, object>("Lifestream.ExecuteCommand");
}
