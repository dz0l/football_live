using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using FootballReport.Models;

namespace FootballReport
{
    internal static class AppConfigLoader
    {
        internal static AppConfig Load()
        {
            var favClubs = new FavoritesConfig(LoadStringArray(ProjectPaths.FavoritesClubsPath));
            var favComps = new FavoritesConfig(LoadStringArray(ProjectPaths.FavoritesCompetitionsPath));

            var aliasClubs = new AliasesConfig(LoadStringMap(ProjectPaths.AliasesClubsPath));
            var aliasComps = new AliasesConfig(LoadStringMap(ProjectPaths.AliasesCompetitionsPath));

            var blClubs = new BlacklistConfig(LoadStringArray(ProjectPaths.BlacklistClubsPath));
            var blComps = new BlacklistConfig(LoadStringArray(ProjectPaths.BlacklistCompetitionsPath));
            var blText = new BlacklistConfig(LoadStringArray(ProjectPaths.BlacklistTextPatternsPath));

            return new AppConfig(
                favoriteClubs: favClubs,
                favoriteCompetitions: favComps,
                clubAliases: aliasClubs,
                competitionAliases: aliasComps,
                blacklistedClubs: blClubs,
                blacklistedCompetitions: blComps,
                blacklistedTextPatterns: blText
            );
        }

        private static IReadOnlyList<string> LoadStringArray(string path)
        {
            if (!File.Exists(path))
                throw new FileNotFoundException($"Config file not found: {path}");

            var json = File.ReadAllText(path, Encoding.UTF8);

            var arr = JsonSerializer.Deserialize<List<string>>(json, new JsonSerializerOptions
            {
                ReadCommentHandling = JsonCommentHandling.Disallow,
                AllowTrailingCommas = false
            });

            return arr ?? new List<string>();
        }

        private static IReadOnlyDictionary<string, string> LoadStringMap(string path)
        {
            if (!File.Exists(path))
                throw new FileNotFoundException($"Config file not found: {path}");

            var json = File.ReadAllText(path, Encoding.UTF8);

            var dict = JsonSerializer.Deserialize<Dictionary<string, string>>(json, new JsonSerializerOptions
            {
                ReadCommentHandling = JsonCommentHandling.Disallow,
                AllowTrailingCommas = false
            });

            return dict ?? new Dictionary<string, string>();
        }
    }
}
