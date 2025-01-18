using Dalamud.Configuration;
using Dalamud.Game.Text;
using ECommons.Configuration;
using ECommons.Logging;
using Newtonsoft.Json;
using SomethingNeedDoing.Macros;
using SomethingNeedDoing.Misc;
using System.Collections.Generic;
using System.IO;
using YamlDotNet.Serialization;

namespace SomethingNeedDoing;

public class Config : IPluginConfiguration
{
    public int Version { get; set; } = 1;
    public bool LockWindow { get; set; } = false;
    public string DefaultFileName { get; set; } = "UntitledMacro";
    public string DefaultFileExtension { get; set; } = ".txt";
    public List<MacroFile> Files { get; set; } = [];
    public FolderNode RootFolder { get; set; } = new FolderNode { Name = "/" };
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
        "/click \"RecipeNote Synthesize\"" +
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

    public MacroNode? ARCharacterPostProcessMacro { get; set; }
    public List<ulong> ARCharacterPostProcessExcludedCharacters { get; set; } = [];

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
    /// Gets or sets the paths that lua macros will use when requiring files
    /// </summary>
    public string[] LuaRequirePaths { get; set; } = [];

    internal static Config Load(DirectoryInfo configDirectory)
    {
        var pluginConfigPath = new FileInfo(Path.Combine(configDirectory.Parent!.FullName, $"SomethingNeedDoing.json"));

        if (!pluginConfigPath.Exists)
            return new Config();

        var data = File.ReadAllText(pluginConfigPath.FullName);
        var conf = JsonConvert.DeserializeObject<Config>(data);
        return conf ?? new Config();
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
        var property = typeof(Config).GetProperty(key);
        if (property != null && property.Name != "Version" && property.CanWrite && (property.PropertyType == typeof(int) || property.PropertyType == typeof(bool)))
        {
            if (property.PropertyType == typeof(int) && int.TryParse(value, out var intValue))
                property.SetValue(this, intValue);
            else if (property.PropertyType == typeof(bool) && bool.TryParse(value, out var boolValue))
                property.SetValue(this, boolValue);
            else
                Svc.Log.Error($"Value type does not match property type for {key}: {value.GetType()} != {property.PropertyType}");
        }
        else
            Svc.Log.Error($"Invalid configuration key or type");
    }

    internal object? GetProperty(string key)
    {
        var property = typeof(Config).GetProperty(key);
        return property != null && property.Name != "Version" && property.CanWrite ? property.GetValue(this) : null;
    }

    internal string RootFolderPath
        => Directory.GetDirectories(Svc.PluginInterface.GetPluginConfigDirectory()).Select(x => new DirectoryInfo(x)).FirstOrDefault(x => x.Name == RootFolder.Name)?.FullName
        ?? Directory.CreateDirectory(Path.Combine(Svc.PluginInterface.GetPluginConfigDirectory(), RootFolder.Name)).FullName;
}

public class ConfigFactory : ISerializationFactory
{
    public string DefaultConfigFileName => $"{nameof(SomethingNeedDoing)}.yaml";
    public T Deserialize<T>(string inputData) => new DeserializerBuilder().IgnoreUnmatchedProperties().Build().Deserialize<T>(inputData);
    public string Serialize(object s, bool prettyPrint) => new SerializerBuilder().Build().Serialize(s);
}

public interface IMigration
{
    int Version { get; }
    void Migrate(ref Config config);
}

public class V2 : IMigration
{
    public int Version => 2;
    public void Migrate(ref Config config)
    {
        PluginLog.Information($"Starting {nameof(IMigration)}{nameof(V2)}");
        config.RootFolder.Name = "Macros";
        WriteNode(config.RootFolder);
    }

    private void WriteNode(INode node, string? path = null)
    {
        if (node is FolderNode folderNode)
        {
            path = Path.Combine(path ?? Svc.PluginInterface.GetPluginConfigDirectory(), folderNode.Name);
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            foreach (var child in folderNode.Children)
                WriteNode(child, path);
        }
        else if (node is MacroNode macroNode)
        {
            var file = new FileInfo(Path.Combine(path ?? Svc.PluginInterface.GetPluginConfigDirectory(), macroNode.Name + macroNode.Language.LanguageToFileExtension()));
            PluginLog.Information($"Writing macro {macroNode.Name} to file @ {file.FullName}");
            File.WriteAllText(file.FullName, macroNode.Contents);
        }
    }
}
