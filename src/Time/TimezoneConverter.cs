using System;

namespace FootballReport.Time
{
    /// <summary>
    /// Конвертация времени строго от startDateTimeUtc (UTC).
    /// GMT+3/+4/+5 задаются фиксированными оффсетами.
    /// </summary>
    public static class TimezoneConverter
    {
        public static readonly TimeSpan GmtPlus3 = TimeSpan.FromHours(3);
        public static readonly TimeSpan GmtPlus4 = TimeSpan.FromHours(4);
        public static readonly TimeSpan GmtPlus5 = TimeSpan.FromHours(5);

        /// <summary>
        /// Преобразует UTC-время матча в локальное для указанного GMT-смещения.
        /// </summary>
        public static DateTimeOffset ConvertUtcToOffset(DateTimeOffset startDateTimeUtc, TimeSpan offset)
        {
            // startDateTimeUtc уже хранится как DateTimeOffset (ожидаем что это именно UTC).
            // Здесь делаем детерминированное преобразование к указанному смещению.
            return startDateTimeUtc.ToOffset(offset);
        }

        /// <summary>
        /// Формат вывода времени в отчёте: HH:mm (24h).
        /// </summary>
        public static string FormatTimeHm(DateTimeOffset localDateTime)
        {
            return localDateTime.ToString("HH:mm");
        }

        /// <summary>
        /// Формат даты для имени файла: dd.MM.yyyy
        /// </summary>
        public static string FormatDateForFilename(DateTimeOffset localDateTime)
        {
            return localDateTime.ToString("dd.MM.yyyy");
        }
    }
}
