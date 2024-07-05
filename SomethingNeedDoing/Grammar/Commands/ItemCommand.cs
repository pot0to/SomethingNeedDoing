using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.System.Framework;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using SomethingNeedDoing.Exceptions;
using SomethingNeedDoing.Grammar.Modifiers;
using SomethingNeedDoing.Misc;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading;

namespace SomethingNeedDoing.Grammar.Commands;

internal class ItemCommand : MacroCommand
{
    public static string[] Commands => ["item"];
    public static string Description => "Use an item, stopping the macro if the item is not present.";
    public static string[] Examples => ["/item Calamari Ripieni", "/item Calamari Ripieni <hq> <wait.3>"];

    private static readonly Regex Regex = new($@"^/{string.Join("|", Commands)}\s+(?<name>.*?)\s*$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public static nint itemContextMenuAgent = nint.Zero;
    public delegate void UseItemDelegate(nint itemContextMenuAgent, uint itemID, uint inventoryPage, uint inventorySlot, short a5);
    public static UseItemDelegate UseItemSig = null!;

    private readonly string itemName;
    private readonly ItemQualityModifier itemQualityMod;

    private ItemCommand(string text, string itemName, WaitModifier wait, ItemQualityModifier itemQualityMod) : base(text, wait)
    {
        this.itemName = itemName.ToLowerInvariant();
        this.itemQualityMod = itemQualityMod;
        if (!Service.Configuration.UseItemStructsVersion)
        {
            try
            {
                UseItemSig = Marshal.GetDelegateForFunctionPointer<UseItemDelegate>(Svc.SigScanner.ScanText("E8 ?? ?? ?? ?? E9 ?? ?? ?? ?? 48 8D 0D ?? ?? ?? ?? E8 ?? ?? ?? ?? 48 89 7C 24 38"));
                unsafe { itemContextMenuAgent = (nint)Framework.Instance()->GetUIModule()->GetAgentModule()->GetAgentByInternalId(AgentId.InventoryContext); }
            }
            catch { Svc.Log.Error($"Failed to load {nameof(UseItemSig)}"); }
        }
    }

    public static ItemCommand Parse(string text)
    {
        _ = WaitModifier.TryParse(ref text, out var waitModifier);
        _ = ItemQualityModifier.TryParse(ref text, out var itemQualityModifier);

        var match = Regex.Match(text);
        if (!match.Success)
            throw new MacroSyntaxError(text);

        var nameValue = ExtractAndUnquote(match, "name");

        return new ItemCommand(text, nameValue, waitModifier, itemQualityModifier);
    }

    public override async System.Threading.Tasks.Task Execute(ActiveMacro macro, CancellationToken token)
    {
        Svc.Log.Debug($"Executing: {Text}");

        var itemId = SearchItemId(itemName);
        Svc.Log.Debug($"Item found: {itemId}");

        var count = GetInventoryItemCount(itemId, itemQualityMod.IsHq);
        Svc.Log.Debug($"Item Count: {count}");
        if (count == 0 && Service.Configuration.StopMacroIfItemNotFound)
            throw new MacroCommandError("You do not have that item");

        UseItem(itemId, itemQualityMod.IsHq);

        await PerformWait(token);
    }

    private unsafe void UseItem(uint itemID, bool isHQ = false)
    {
        var agent = AgentInventoryContext.Instance();
        if (agent == null)
            throw new MacroCommandError("AgentInventoryContext not found");

        if (isHQ)
            itemID += 1_000_000;

        if (Service.Configuration.UseItemStructsVersion)
        {
            var result = agent->UseItem(itemID);
            if (result != 0 && Service.Configuration.StopMacroIfCantUseItem)
                throw new MacroCommandError("Failed to use item");
        }
        else
            UseItemSig(itemContextMenuAgent, itemID, 9999, 0, 0);
    }

    private unsafe int GetInventoryItemCount(uint itemID, bool isHQ)
    {
        var inventoryManager = InventoryManager.Instance();
        return inventoryManager == null
            ? throw new MacroCommandError("InventoryManager not found")
            : inventoryManager->GetInventoryItemCount(itemID, isHQ);
    }

    private uint SearchItemId(string itemName)
    {
        var sheet = Svc.Data.GetExcelSheet<Sheets.Item>()!;
        var item = sheet.FirstOrDefault(r => r.Name.ToString().Equals(itemName, System.StringComparison.InvariantCultureIgnoreCase));
        return item == null ? throw new MacroCommandError("Item not found") : item.RowId;
    }
}
