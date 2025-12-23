using System;
using System.IO;

namespace FootballReport
{
    public static class ProjectPaths
    {
        public static string Root { get; } = LocateRoot();

        public static string ConfigDir => Path.Combine(Root, "config");
        public static string TemplatesDir => Path.Combine(Root, "templates");
        public static string OutDir => Path.Combine(Root, "out");

        public static string TemplateHtmlPath => Path.Combine(TemplatesDir, "report_template.html");
        public static string TemplateCssPath => Path.Combine(TemplatesDir, "report_styles.css");

        public static string FavoritesClubsPath => Path.Combine(ConfigDir, "favorites_clubs.json");
        public static string FavoritesCompetitionsPath => Path.Combine(ConfigDir, "favorites_competitions.json");
        public static string AliasesClubsPath => Path.Combine(ConfigDir, "aliases_clubs.json");
        public static string AliasesCompetitionsPath => Path.Combine(ConfigDir, "aliases_competitions.json");
        public static string BlacklistClubsPath => Path.Combine(ConfigDir, "blacklist_clubs.json");
        public static string BlacklistCompetitionsPath => Path.Combine(ConfigDir, "blacklist_competitions.json");
        public static string BlacklistTextPatternsPath => Path.Combine(ConfigDir, "blacklist_text_patterns.json");

        private static string LocateRoot()
        {
            var dir = Path.GetFullPath(AppContext.BaseDirectory);
            var last = string.Empty;

            while (!string.IsNullOrEmpty(dir) && dir != last)
            {
                if (Directory.Exists(Path.Combine(dir, "config")) &&
                    Directory.Exists(Path.Combine(dir, "templates")))
                {
                    return dir;
                }

                var parent = Directory.GetParent(dir);
                last = dir;
                dir = parent?.FullName ?? string.Empty;
            }

            return Path.GetFullPath(AppContext.BaseDirectory);
        }
    }
}
