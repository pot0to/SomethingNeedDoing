using ECommons;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using FFXIVClientStructs.FFXIV.Component.GUI;
using SomethingNeedDoing.Exceptions;
using SomethingNeedDoing.Grammar.Modifiers;
using SomethingNeedDoing.Misc;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace SomethingNeedDoing.Grammar.Commands;

internal class RecipeCommand : MacroCommand
{
    private static readonly Regex Regex = new(@"^/recipe\s+(?<name>.*?)\s*$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private readonly string recipeName;

    private RecipeCommand(string text, string recipeName, WaitModifier wait) : base(text, wait) => this.recipeName = recipeName.ToLowerInvariant();

    public static RecipeCommand Parse(string text)
    {
        _ = WaitModifier.TryParse(ref text, out var waitModifier);

        var match = Regex.Match(text);
        if (!match.Success)
            throw new MacroSyntaxError(text);

        var nameValue = ExtractAndUnquote(match, "name");

        return new RecipeCommand(text, nameValue, waitModifier);
    }

    public override async Task Execute(ActiveMacro macro, CancellationToken token)
    {
        Svc.Log.Debug($"Executing: {this.Text}");

        if (AddonSynthesisIsOpen())
            throw new MacroCommandError("/recipe cannot be used while the Synthesis window is open.");

        var recipeId = this.SearchRecipeId(this.recipeName);
        Svc.Log.Debug($"Recipe found: {recipeId}");

        this.OpenRecipeNote(recipeId);

        await this.PerformWait(token);
    }

    private unsafe bool AddonSynthesisIsOpen() => GenericHelpers.TryGetAddonByName<AtkUnitBase>("Synthesis", out _);

    private unsafe void OpenRecipeNote(uint recipeID)
    {
        var agent = AgentRecipeNote.Instance();
        if (agent == null)
            throw new MacroCommandError("AgentRecipeNote not found");

        agent->OpenRecipeByRecipeId(recipeID);
    }

    private uint SearchRecipeId(string recipeName)
    {
        var sheet = Svc.Data.GetExcelSheet<Sheets.Recipe>()!;
        var recipes = sheet.Where(r => r.ItemResult.Value?.Name.ToString().ToLowerInvariant() == recipeName).ToList();

        switch (recipes.Count)
        {
            case 0: throw new MacroCommandError("Recipe not found");
            case 1: return recipes.First().RowId;
            default:
                var jobId = Svc.ClientState.LocalPlayer?.ClassJob.Id;

                var recipe = recipes.Where(r => this.GetClassJobID(r) == jobId).FirstOrDefault();
                if (recipe == default)
                    return recipes.First().RowId;

                return recipe.RowId;
        }
    }

    private uint GetClassJobID(Sheets.Recipe recipe) =>
        // Name           CraftType ClassJob
        // Carpenter      0         8
        // Blacksmith     1         9
        // Armorer        2         10
        // Goldsmith      3         11
        // Leatherworker  4         12
        // Weaver         5         13
        // Alchemist      6         14
        // Culinarian     7         15
        recipe.CraftType.Value!.RowId + 8;
}
