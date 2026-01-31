using System;
using Dalamud.Game.Text;
using Dalamud.Hooking;
using FFXIVClientStructs.FFXIV.Client.System.String;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using Lumina.Text;
using Lumina.Text.Payloads;
using Lumina.Text.ReadOnly;
using SeString = Dalamud.Game.Text.SeStringHandling.SeString;
using LSeStringBuilder = Lumina.Text.SeStringBuilder;

namespace RacingwayRewrite.Utils.Interop.Hooks;

using FormatLogMessage = RaptureLogModule.Delegates.FormatLogMessage;

public unsafe class MessageHooks : IDisposable
{
    private readonly Hook<FormatLogMessage>? formatLogHook;

    public MessageHooks()
    {
        formatLogHook = Plugin.GameInteropProvider.HookFromAddress<FormatLogMessage>(
            RaptureLogModule.MemberFunctionPointers.FormatLogMessage, DetourFormatLog);
        
        Plugin.ChatGui.ChatMessageUnhandled += ChatGuiOnChatMessageUnhandled;
        
        formatLogHook.Enable();
        ReloadChat();
    }
    
    public void Dispose()
    {
        formatLogHook?.Dispose();
        Plugin.ChatGui.ChatMessageUnhandled -= ChatGuiOnChatMessageUnhandled;
        
        ReloadChat();
        GC.SuppressFinalize(this);
    }
    
    private static bool IsBattleType(XivChatType type) {
        var channel = ((int)type & 0x7F);
        switch (channel) {
            case 41: // Damage
            case 42: // Miss
            case 43: // Action
            case 44: // Item
            case 45: // Healing
            case 46: // GainBeneficialStatus
            case 48: // LoseBeneficialStatus
            case 47: // GainDetrimentalStatus
            case 49: // LoseDetrimentalStatus
            case 58: // BattleSystem
                return true;
            default:
                return false;
        }
    }

    private void ChatGuiOnChatMessageUnhandled(XivChatType type, int timestamp, SeString sender, SeString message)
    {
        if (IsBattleType(type)) return; // Filter out any battle related chats
        LastMessage = new LogMessage(message, timestamp);
    }

    public static void ReloadChat()
    {
        var raptureLogModule = RaptureLogModule.Instance();
        for (var i = 0; i < 4; i++)
        {
            raptureLogModule->ChatTabIsPendingReload[i] = true;
        }
    }

    private LogMessage? lastMessage;
    public LogMessage? LastMessage
    {
        get => lastMessage;
        set
        {
            if (lastMessage != null && value != null && 
                value.Message.Encode().SequenceEqual(lastMessage.Message.Encode()))
            {
                if (!value.IsRacingway) return;
                Dupes++;
                if (Dupes > 1) ReloadChat();
                return;
            }
            
            Dupes = 1;
            lastMessage = value;
            ReloadChat();
        }
    }
    
    public uint Dupes;

    private uint DetourFormatLog(RaptureLogModule* thisPtr, uint logKindId, Utf8String* sender, Utf8String* message, int* timestamp, void* a6, Utf8String* a7, int chatTabIndex)
    {
        try
        {
            if (LastMessage is not { IsRacingway: true })
            {
                return formatLogHook!.Original(thisPtr, logKindId, sender, message, timestamp, a6, a7, chatTabIndex);
            }
            
            var msg = new LogMessage(message, *timestamp);
            using var newMsg = new Utf8String();
            
            if (LastMessage.Equals(msg) && Dupes > 1)
            {
                var chat = new XivChatEntry
                {
                    Message = LastMessage.Message
                };

                var sb = new LSeStringBuilder();
                
                // Yoinked straight from Dalamud's ChatGui.cs
                foreach (var c in UtfEnumerator.From(chat.MessageBytes, UtfEnumeratorFlags.Utf8SeString))
                {
                    if (c.IsSeStringPayload)
                        sb.Append((ReadOnlySeStringSpan)chat.MessageBytes.AsSpan(c.ByteOffset, c.ByteLength));
                    else if (c.Value.IntValue == 0x202F)
                        sb.BeginMacro(MacroCode.NonBreakingSpace).EndMacro();
                    else
                        sb.Append(c);
                }

                sb.PushColorType(4).Append(" (x" + Dupes + ")").PopColorType();
                newMsg.SetString(sb.GetViewAsSpan());
                
                return formatLogHook!.Original(thisPtr, logKindId, sender, &newMsg, timestamp, a6, a7, chatTabIndex);
            }
        }
        catch (Exception e)
        {
            Plugin.Log.Error(e.ToString());
        }
        
        return formatLogHook!.Original(thisPtr, logKindId, sender, message, timestamp, a6, a7, chatTabIndex);
    }

    public class LogMessage(SeString message, int timestamp, bool isRacingway = false)
    {
        public readonly SeString Message = message;
        public readonly int Timestamp = timestamp;
        public readonly bool IsRacingway = isRacingway;

        public LogMessage(Utf8String* message, int timestamp) : this(SeString.Parse(message->AsSpan()), timestamp) { }

        public bool Equals(LogMessage other)
        {
            var distance = Math.Abs(other.Timestamp - Timestamp);
            
            return Message.Encode().SequenceEqual(other.Message.Encode()) && distance <= 5;
        }
    }
}
