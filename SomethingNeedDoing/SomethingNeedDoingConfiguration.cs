using Dalamud.Configuration;
using Dalamud.Game.Text;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;

namespace SomethingNeedDoing;

public class SomethingNeedDoingConfiguration : IPluginConfiguration
{
    public int Version { get; set; } = 1;
    public bool LockWindow { get; set; } = false;
    public FolderNode RootFolder { get; private set; } = new FolderNode { Name = "/" };
    public bool CraftSkip { get; set; } = true;
    public bool SmartWait { get; set; } = false;
    public bool QualitySkip { get; set; } = true;
    public bool LoopTotal { get; set; } = false;
    public bool LoopEcho { get; set; } = false;
    public bool DisableMonospaced { get; set; } = false;
    public bool UseCraftLoopTemplate { get; set; } = false;
    public string CraftLoopTemplate { get; set; } =
        "/craft {{count}}\n" +
        "/waitaddon \"RecipeNote\" <maxwait.5>" +
        "/click \"synthesize\"" +
        "/waitaddon \"Synthesis\" <maxwait.5>" +
        "{{macro}}" +
        "/loop";

    public bool CraftLoopFromRecipeNote { get; set; } = true;
    public int CraftLoopMaxWait { get; set; } = 5;
    public bool CraftLoopEcho { get; set; } = false;
    public int MaxTimeoutRetries { get; set; } = 0;
    public bool NoisyErrors { get; set; } = false;
    public int BeepFrequency { get; set; } = 900;
    public int BeepDuration { get; set; } = 250;
    public int BeepCount { get; set; } = 3;
    public bool UseSNDTargeting { get; set; } = true;
    public bool UseItemStructsVersion { get; set; } = true;

    public bool StopMacroIfActionTimeout { get; set; } = true;
    public bool StopMacroIfItemNotFound { get; set; } = true;
    public bool StopMacroIfCantUseItem { get; set; } = true;
    public bool StopMacroIfTargetNotFound { get; set; } = true;
    public bool StopMacroIfAddonNotFound { get; set; } = true;
    public bool StopMacroIfAddonNotVisible { get; set; } = true;

    /// <summary>
    /// Gets or sets the chat channel to use.
    /// </summary>
    public XivChatType ChatType { get; set; } = XivChatType.Debug;

    /// <summary>
    /// Gets or sets the error chat channel to use.
    /// </summary>
    public XivChatType ErrorChatType { get; set; } = XivChatType.Urgent;

    /// <summary>
    /// Loads the configuration.
    /// </summary>
    /// <param name="configDirectory">Configuration directory.</param>
    /// <returns>A configuration.</returns>
    internal static SomethingNeedDoingConfiguration Load(DirectoryInfo configDirectory)
    {
        var pluginConfigPath = new FileInfo(Path.Combine(configDirectory.Parent!.FullName, $"SomethingNeedDoing.json"));

        if (!pluginConfigPath.Exists)
            return new SomethingNeedDoingConfiguration();

        var data = File.ReadAllText(pluginConfigPath.FullName);
        var conf = JsonConvert.DeserializeObject<SomethingNeedDoingConfiguration>(data);
        return conf ?? new SomethingNeedDoingConfiguration();
    }

    internal void Save() => Svc.PluginInterface.SavePluginConfig(this);

    internal IEnumerable<INode> GetAllNodes() => new INode[] { RootFolder }.Concat(GetAllNodes(RootFolder.Children));

    internal IEnumerable<INode> GetAllNodes(IEnumerable<INode> nodes)
    {
        foreach (var node in nodes)
        {
            yield return node;
            if (node is FolderNode folder)
            {
                var childNodes = GetAllNodes(folder.Children);
                foreach (var childNode in childNodes)
                {
                    yield return childNode;
                }
            }
        }
    }

    internal bool TryFindParent(INode node, out FolderNode? parent)
    {
        foreach (var candidate in GetAllNodes())
        {
            if (candidate is FolderNode folder && folder.Children.Contains(node))
            {
                parent = folder;
                return true;
            }
        }

        parent = null;
        return false;
    }

    internal void SetProperty(string key, string value)
    {
        var property = typeof(SomethingNeedDoingConfiguration).GetProperty(key);
        if (property != null && property.Name != "Version" && property.CanWrite && (property.PropertyType == typeof(int) || property.PropertyType == typeof(bool)))
        {
            if (property.PropertyType == typeof(int) && int.TryParse(value, out int intValue))
                property.SetValue(this, intValue);
            else if (property.PropertyType == typeof(bool) && bool.TryParse(value, out bool boolValue))
                property.SetValue(this, boolValue);
            else
                Svc.Log.Error($"Value type does not match property type for {key}: {value.GetType()} != {property.PropertyType}");
        }
        else
            Svc.Log.Error($"Invalid configuration key or type");
    }

    internal object GetProperty(string key)
    {
        var property = typeof(SomethingNeedDoingConfiguration).GetProperty(key);
        if (property != null && property.Name != "Version" && property.CanWrite)
            return property.GetValue(this)!;
        else
            return null;
    }
}
