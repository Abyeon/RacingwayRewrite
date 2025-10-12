using System;
using System.Collections.Generic;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.Game.Object;

namespace RacingwayRewrite.Utils;

/// <summary>
/// Handles spawning/deleting custom actors from the scene
/// </summary>
public class ActorManager : IDisposable
{
    internal readonly IFramework Framework;
    internal const uint MaxActors = 10;

    public ActorManager(IFramework framework)
    {
        Framework = framework;
        Framework.Update += Update;
    }
    
    private Stack<uint> ids = new Stack<uint>();
    
    // Probably want to implement a queue much like OrangeGuidanceTomestone's here

    private unsafe void Update(IFramework framework)
    {
        var manager = ClientObjectManager.Instance();
        
        throw new NotImplementedException();
    }

    public void Dispose()
    {
        Framework.Update -= Update;
    }
}
