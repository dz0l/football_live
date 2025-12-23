using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using FootballReport;

namespace FootballReport.Ui.UI.Services;

internal enum ConfigListKey
{
    FavoritesClubs,
    FavoritesCompetitions,
    BlacklistClubs,
    BlacklistCompetitions,
    BlacklistPatterns
}

internal enum ConfigAliasKey
{
    AliasClubs,
    AliasCompetitions
}

internal static class ConfigFilesService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    internal static Dictionary<ConfigListKey, List<string>> LoadLists()
    {
        var dict = new Dictionary<ConfigListKey, List<string>>();

        foreach (ConfigListKey key in Enum.GetValues(typeof(ConfigListKey)))
        {
            dict[key] = ReadList(PathFor(key));
        }

        return dict;
    }

    internal static Dictionary<ConfigAliasKey, Dictionary<string, string>> LoadAliases()
    {
        var dict = new Dictionary<ConfigAliasKey, Dictionary<string, string>>();

        foreach (ConfigAliasKey key in Enum.GetValues(typeof(ConfigAliasKey)))
        {
            dict[key] = ReadMap(PathFor(key));
        }

        return dict;
    }

    internal static void SaveList(ConfigListKey key, IEnumerable<string> values)
    {
        var cleaned = CleanList(values);
        var json = JsonSerializer.Serialize(cleaned, JsonOptions);
        File.WriteAllText(PathFor(key), json, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
    }

    internal static void SaveAlias(ConfigAliasKey key, IDictionary<string, string> map)
    {
        var cleaned = CleanMap(map);
        var json = JsonSerializer.Serialize(cleaned, JsonOptions);
        File.WriteAllText(PathFor(key), json, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
    }

    private static List<string> ReadList(string path)
    {
        if (!File.Exists(path))
            return new List<string>();

        var json = File.ReadAllText(path, Encoding.UTF8);
        var list = JsonSerializer.Deserialize<List<string>>(json) ?? new List<string>();
        return CleanList(list);
    }

    private static Dictionary<string, string> ReadMap(string path)
    {
        if (!File.Exists(path))
            return new Dictionary<string, string>(StringComparer.Ordinal);

        var json = File.ReadAllText(path, Encoding.UTF8);
        var map = JsonSerializer.Deserialize<Dictionary<string, string>>(json) ??
                  new Dictionary<string, string>(StringComparer.Ordinal);

        return CleanMap(map);
    }

    private static List<string> CleanList(IEnumerable<string>? input)
    {
        var result = new List<string>();
        if (input == null) return result;

        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var raw in input)
        {
            var value = (raw ?? string.Empty).Trim();
            if (value.Length == 0) continue;
            if (seen.Add(value))
                result.Add(value);
        }

        return result;
    }

    private static Dictionary<string, string> CleanMap(IDictionary<string, string>? input)
    {
        var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        if (input == null) return result;

        foreach (var kv in input)
        {
            var key = (kv.Key ?? string.Empty).Trim();
            var val = (kv.Value ?? string.Empty).Trim();
            if (key.Length == 0 || val.Length == 0) continue;

            if (!result.ContainsKey(key))
                result[key] = val;
        }

        return result;
    }

    private static string PathFor(ConfigListKey key) => key switch
    {
        ConfigListKey.FavoritesClubs => ProjectPaths.FavoritesClubsPath,
        ConfigListKey.FavoritesCompetitions => ProjectPaths.FavoritesCompetitionsPath,
        ConfigListKey.BlacklistClubs => ProjectPaths.BlacklistClubsPath,
        ConfigListKey.BlacklistCompetitions => ProjectPaths.BlacklistCompetitionsPath,
        ConfigListKey.BlacklistPatterns => ProjectPaths.BlacklistTextPatternsPath,
        _ => throw new ArgumentOutOfRangeException(nameof(key), key, "Unknown list key")
    };

    private static string PathFor(ConfigAliasKey key) => key switch
    {
        ConfigAliasKey.AliasClubs => ProjectPaths.AliasesClubsPath,
        ConfigAliasKey.AliasCompetitions => ProjectPaths.AliasesCompetitionsPath,
        _ => throw new ArgumentOutOfRangeException(nameof(key), key, "Unknown alias key")
    };
}
