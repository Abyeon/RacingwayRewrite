using System;
using Dalamud.Hooking;
using FFXIVClientStructs.FFXIV.Client.Game.Control;

namespace RacingwayRewrite.Utils.Structs;

using PlayTimelineDelegate = ActionTimelineSequencer.Delegates.PlayTimeline;
using SetSlotDelegate = ActionTimelineSequencer.Delegates.SetSlotTimeline;

public unsafe class TimelineHook : IDisposable
{
    private readonly Hook<PlayTimelineDelegate>? timelineHook;
    private readonly Hook<SetSlotDelegate>? setSlotHook;

    public TimelineHook()
    {
        timelineHook = Plugin.GameInteropProvider.HookFromAddress<PlayTimelineDelegate>(
            ActionTimelineSequencer.MemberFunctionPointers.PlayTimeline,
            DetourPlayTimeline);

        setSlotHook = Plugin.GameInteropProvider.HookFromAddress<SetSlotDelegate>(
            ActionTimelineSequencer.MemberFunctionPointers.SetSlotTimeline,
            DetourSetSlot);
        
        timelineHook.Enable();
    }

    private void DetourSetSlot(ActionTimelineSequencer* sequencer, uint slot, ushort actionTimelineId)
    {
        Plugin.Log.Verbose("Detouring SetSlotTimeline.");

        try
        {
            Plugin.Log.Debug("Started setting slot " + slot + " with animation: " + actionTimelineId);
        }
        catch (Exception ex)
        {
            Plugin.Log.Error(ex, "An error occured while detouring SetSlotTimeline.");
        }
        
        setSlotHook!.Original(sequencer, slot, actionTimelineId);
    }

    public void Dispose()
    {
        timelineHook?.Disable();
    }

    private void DetourPlayTimeline(ActionTimelineSequencer* sequencer, ushort actionTimelineId, void* a3 = null)
    {
        Plugin.Log.Verbose("Detouring PlayTimeline.");
        
        try
        {
            // Started playing an animation.
            Plugin.Log.Debug("Started playing: " + actionTimelineId);
        }
        catch (Exception ex)
        {
            Plugin.Log.Error(ex, "An error occured while detouring PlayTimeline.");
        }
        
        timelineHook!.Original(sequencer, actionTimelineId, a3);
    }
}
