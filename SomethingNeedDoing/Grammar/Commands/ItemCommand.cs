using ECommons.DalamudServices;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.System.Framework;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using SomethingNeedDoing.Exceptions;
using SomethingNeedDoing.Grammar.Modifiers;
using SomethingNeedDoing.Misc;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading;
using Sheets = Lumina.Excel.GeneratedSheets;

namespace SomethingNeedDoing.Grammar.Commands;

/// <summary>
/// The /item command.
/// </summary>
internal class ItemCommand : MacroCommand
{
    private static readonly Regex Regex = new(@"^/item\s+(?<name>.*?)\s*$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public static nint itemContextMenuAgent = nint.Zero;
    public delegate void UseItemDelegate(nint itemContextMenuAgent, uint itemID, uint inventoryPage, uint inventorySlot, short a5);
    public static UseItemDelegate UseItemSig;

    private readonly string itemName;
    private readonly ItemQualityModifier itemQualityMod;

    /// <summary>
    /// Initializes a new instance of the <see cref="ItemCommand"/> class.
    /// </summary>
    /// <param name="text">Original text.</param>
    /// <param name="itemName">Item name.</param>
    /// <param name="wait">Wait value.</param>
    /// <param name="itemQualityMod">Required quality of the item used.</param>
    private ItemCommand(string text, string itemName, WaitModifier wait, ItemQualityModifier itemQualityMod)
        : base(text, wait)
    {
        this.itemName = itemName.ToLowerInvariant();
        this.itemQualityMod = itemQualityMod;
        if (!Service.Configuration.UseItemStructsVersion)
        {
            try
            {
                UseItemSig = Marshal.GetDelegateForFunctionPointer<UseItemDelegate>(Service.SigScanner.ScanText("E8 ?? ?? ?? ?? E9 ?? ?? ?? ?? 48 8D 0D ?? ?? ?? ?? E8 ?? ?? ?? ?? 48 89 7C 24 38"));
                unsafe { itemContextMenuAgent = (nint)Framework.Instance()->GetUiModule()->GetAgentModule()->GetAgentByInternalId(AgentId.InventoryContext); }
            }
            catch { Svc.Log.Error($"Failed to load {nameof(UseItemSig)}"); }
        }
    }

    /// <summary>
    /// Parse the text as a command.
    /// </summary>
    /// <param name="text">Text to parse.</param>
    /// <returns>A parsed command.</returns>
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

    /// <inheritdoc/>
    public override async System.Threading.Tasks.Task Execute(ActiveMacro macro, CancellationToken token)
    {
        Service.Log.Debug($"Executing: {this.Text}");

        var itemId = this.SearchItemId(this.itemName);
        Service.Log.Debug($"Item found: {itemId}");

        var count = this.GetInventoryItemCount(itemId, this.itemQualityMod.IsHq);
        Service.Log.Debug($"Item Count: {count}");
        if (count == 0 && Service.Configuration.StopMacroIfItemNotFound)
            throw new MacroCommandError("You do not have that item");

        this.UseItem(itemId, this.itemQualityMod.IsHq);

        await this.PerformWait(token);
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
        var sheet = Service.DataManager.GetExcelSheet<Sheets.Item>()!;
        var item = sheet.FirstOrDefault(r => r.Name.ToString().ToLowerInvariant() == itemName);
        return item == null ? throw new MacroCommandError("Item not found") : item.RowId;
    }
}
