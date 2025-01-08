using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SomethingNeedDoing.Macros;
using SomethingNeedDoing.Macros.Exceptions;
using System;
using System.Collections.Generic;
using System.IO;

namespace SomethingNeedDoing;

public interface INode
{
    public string Name { get; set; }
    public string FilePath { get; set; }

    public bool IsFileOrInFolder(string path)
    {
        if (File.Exists(path)) return path == FilePath;
        else if (Directory.Exists(path))
            foreach (var d in Directory.GetDirectories(path))
                if (IsFileOrInFolder(d)) return true;
        return false;
    }

    public string FileName => Path.GetFileNameWithoutExtension(FilePath);
    public string Extension => Path.GetExtension(FilePath);
    public string DirectoryName => Path.GetDirectoryName(FilePath) ?? string.Empty;
    public string RelativePath => Path.GetRelativePath(Service.Configuration.RootFolderPath, FilePath);
    public void Rename(string name) => File.Move(FilePath, Path.Combine(DirectoryName, name + Extension));
    public bool Exists => File.Exists(FilePath);
}

public enum Language
{
    Native,
    Lua,
    //CSharp,
    //Python,
}

public class MacroNode : INode
{
    public string Name { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string Gist { get; set; } = string.Empty;
    public string Contents { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether this macro should loop automatically.
    /// </summary>
    public bool CraftingLoop { get; set; } = false;

    /// <summary>
    /// Gets or sets a value indicating how many loops this macro should run if looping is enabled.
    /// </summary>
    public int CraftLoopCount { get; set; } = 0;
    public Language Language { get; set; } = Language.Native;

    public MacroNode() { }
    public MacroNode(MacroFile file)
    {
        Name = file.Name;
        FilePath = file.Path;
        Contents = file.Contents;
        Language = file.Language;
    }

    public void RunMacro()
    {
        try
        {
            Service.MacroManager.EnqueueMacro(this);
        }
        catch (MacroSyntaxError ex)
        {
            Service.ChatManager.PrintError($"{ex.Message}");
        }
        catch (Exception ex)
        {
            Service.ChatManager.PrintError($"Unexpected error");
            Svc.Log.Error(ex, "Unexpected error");
        }
    }
}

public class FolderNode : INode
{
    public string Name { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string Gist { get; set; } = string.Empty;

    [JsonProperty(ItemConverterType = typeof(ConcreteNodeConverter))]
    public List<INode> Children { get; } = [];
}

/// <summary>
/// Converts INodes to MacroNodes or FolderNodes.
/// </summary>
public class ConcreteNodeConverter : JsonConverter
{
    public override bool CanRead => true;
    public override bool CanWrite => false;

    public override bool CanConvert(Type objectType) => objectType == typeof(INode);

    public override object ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
    {
        var jObject = JObject.Load(reader);
        var jType = jObject["$type"]?.Value<string>();

        if (jType == SimpleName(typeof(MacroNode)))
        {
            var obj = new MacroNode();
            serializer.Populate(jObject.CreateReader(), obj);
            return obj;
        }
        else if (jType == SimpleName(typeof(FolderNode)))
        {
            var obj = new FolderNode();
            serializer.Populate(jObject.CreateReader(), obj);
            return obj;
        }
        else
        {
            throw new NotSupportedException($"Node type \"{jType}\" is not supported.");
        }
    }

    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer) => throw new NotImplementedException();
    private string SimpleName(Type type) => $"{type.FullName}, {type.Assembly.GetName().Name}";
}
