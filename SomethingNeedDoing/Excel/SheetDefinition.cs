using System.Collections.Generic;
using System.Text.Json.Serialization;

#pragma warning disable CS8618

namespace SomethingNeedDoing.Excel;

// Don't believe Rider's lies, all JSON stuff *needs* to have setters or else the JSON deserializer won't assign them
// I wasted an hour with this, I hate you Microsoft
public class SheetDefinition
{
    [JsonPropertyName("sheet")] public string? Sheet { get; init; }
    [JsonPropertyName("defaultColumn")] public string? DefaultColumn { get; init; }
    [JsonPropertyName("definitions")] public ColumnDefinition[] Definitions { get; init; }

    private Dictionary<uint, ColumnDefinition?>? _columnCache;

    private uint ResolveDefinition(ColumnDefinition def, uint offset = 0)
    {
        // Index defaults to zero if there isn't one specified, BUT this might be a repeat or group definition
        var realOffset = def.Index == 0 ? offset : def.Index;

        if (def is RepeatColumnDefinition rcd)
        {
            var baseIdx = realOffset;

            for (var i = 0; i < rcd.Count; i++)
            {
                baseIdx += ResolveDefinition(rcd.Definition, baseIdx);
            }

            return baseIdx - realOffset;
        }

        if (def is GroupColumnDefinition gcd)
        {
            var baseIdx = realOffset;

            foreach (var member in gcd.Members)
            {
                baseIdx += ResolveDefinition(member, baseIdx);
            }

            return baseIdx - realOffset;
        }

        // Normal definition, just insert and move on
        if (!_columnCache!.ContainsKey(realOffset))
        {
            _columnCache[realOffset] = def;
        }

        return 1;
    }

    // can't put this in constructor, dunno why
    private void EnsureColumnCache()
    {
        if (_columnCache is null)
        {
            _columnCache = [];

            foreach (var def in Definitions)
            {
                ResolveDefinition(def);
            }
        }
    }

    private ColumnDefinition? GetDefinitionByIndex(uint index)
    {
        EnsureColumnCache();
        return _columnCache!.TryGetValue(index, out var retDef) ? retDef : null;
    }

    public string? GetNameForColumn(int index)
    {
        var def = GetDefinitionByIndex((uint)index);

        if (def is SingleColumnDefinition srd) return srd.Name;
        // TODO

        return null;
    }

    public int? GetColumnForName(string name)
    {
        EnsureColumnCache();
        foreach (var (key, value) in _columnCache!)
        {
            if (value is SingleColumnDefinition srd && srd.Name == name) return (int)key;
            // TODO
        }

        return null;
    }

    public ConverterDefinition? GetConverterForColumn(int index)
    {
        var def = GetDefinitionByIndex((uint)index);

        if (def is SingleColumnDefinition srd) return srd.Converter;
        // TODO

        return null;
    }
}
