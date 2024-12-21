using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Dalamud.Plugin.Services;
using Dalamud.Utility.Signatures;
using ECommons.Automation;
using ECommons.ChatMethods;
using FFXIVClientStructs.FFXIV.Client.UI;
using System;
using System.Threading.Channels;

namespace SomethingNeedDoing.Managers;

internal class ChatManager : IDisposable
{
    private readonly Channel<string> chatBoxMessages = Channel.CreateUnbounded<string>();

    [Signature("48 89 5C 24 ?? 48 89 74 24 ?? 57 48 83 EC 20 48 8B F2 48 8B F9 45 84 C9")]
    private readonly ProcessChatBoxDelegate processChatBox = null!;

    public ChatManager()
    {
        Svc.Hook.InitializeFromAttributes(this);
        Svc.Framework.Update += FrameworkUpdate;
    }

    private unsafe delegate void ProcessChatBoxDelegate(UIModule* uiModule, IntPtr message, IntPtr unused, byte a4);

    public void Dispose()
    {
        Svc.Framework.Update -= FrameworkUpdate;
        chatBoxMessages.Writer.Complete();
    }

    public void PrintMessage(string message)
        => Svc.Chat.Print(new XivChatEntry()
        {
            Type = Service.Configuration.ChatType,
            Message = $"[{Plugin.Prefix}] {message}",
        });

    public void PrintColor(string message, UIColor color)
        => Svc.Chat.Print(new XivChatEntry()
        {
            Type = Service.Configuration.ChatType,
            Message = new SeString(
                new UIForegroundPayload((ushort)color),
                new TextPayload($"[{Plugin.Prefix}] {message}"),
                UIForegroundPayload.UIForegroundOff),
        });

    public void PrintError(string message)
        => Svc.Chat.Print(new XivChatEntry()
        {
            Type = Service.Configuration.ErrorChatType,
            Message = $"[{Plugin.Prefix}] {message}",
        });


    public async void SendMessage(string message) => await chatBoxMessages.Writer.WriteAsync(message);

    /// <summary>
    /// Clear the queue of messages to send to the chatbox.
    /// </summary>
    public void Clear()
    {
        var reader = chatBoxMessages.Reader;
        while (reader.Count > 0 && reader.TryRead(out var _))
            continue;
    }

    private void FrameworkUpdate(IFramework framework)
    {
        if (chatBoxMessages.Reader.TryRead(out var message))
            Chat.Instance.SendMessage(message);
    }
}
