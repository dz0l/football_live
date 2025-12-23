using System.Collections.Generic;

namespace FootballReport.Models
{
    /// <summary>
    /// Список избранных сущностей (клубы, турниры).
    /// JSON: массив строк
    /// </summary>
    public sealed class FavoritesConfig
    {
        public IReadOnlyList<string> Items { get; }

        public FavoritesConfig(IReadOnlyList<string> items)
        {
            Items = items ?? new List<string>();
        }
    }

    /// <summary>
    /// Алиасы для нормализации названий.
    /// JSON: { "rawName": "canonicalName" }
    /// </summary>
    public sealed class AliasesConfig
    {
        public IReadOnlyDictionary<string, string> Map { get; }

        public AliasesConfig(IReadOnlyDictionary<string, string> map)
        {
            Map = map ?? new Dictionary<string, string>();
        }
    }

    /// <summary>
    /// Чёрные списки (клубы, турниры, текстовые паттерны).
    /// JSON: массив строк
    /// </summary>
    public sealed class BlacklistConfig
    {
        public IReadOnlyList<string> Items { get; }

        public BlacklistConfig(IReadOnlyList<string> items)
        {
            Items = items ?? new List<string>();
        }
    }

    /// <summary>
    /// Агрегированный конфиг приложения.
    /// Используется как единая точка доступа к данным config/.
    /// </summary>
    public sealed class AppConfig
    {
        public FavoritesConfig FavoriteClubs { get; }
        public FavoritesConfig FavoriteCompetitions { get; }

        public AliasesConfig ClubAliases { get; }
        public AliasesConfig CompetitionAliases { get; }

        public BlacklistConfig BlacklistedClubs { get; }
        public BlacklistConfig BlacklistedCompetitions { get; }
        public BlacklistConfig BlacklistedTextPatterns { get; }

        public AppConfig(
            FavoritesConfig favoriteClubs,
            FavoritesConfig favoriteCompetitions,
            AliasesConfig clubAliases,
            AliasesConfig competitionAliases,
            BlacklistConfig blacklistedClubs,
            BlacklistConfig blacklistedCompetitions,
            BlacklistConfig blacklistedTextPatterns)
        {
            FavoriteClubs = favoriteClubs;
            FavoriteCompetitions = favoriteCompetitions;
            ClubAliases = clubAliases;
            CompetitionAliases = competitionAliases;
            BlacklistedClubs = blacklistedClubs;
            BlacklistedCompetitions = blacklistedCompetitions;
            BlacklistedTextPatterns = blacklistedTextPatterns;
        }
    }
}
