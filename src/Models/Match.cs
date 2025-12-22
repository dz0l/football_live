using System;
using System.Collections.Generic;

namespace FootballReport.Models
{
    /// <summary>
    /// Внутренняя доменная модель матча.
    /// Строится ТОЛЬКО из подтверждённых полей /api/flashscore/football/live.
    /// </summary>
    public sealed class Match
    {
        /// <summary>
        /// Уникальный идентификатор матча (используется для дедупликации).
        /// </summary>
        public string EventId { get; }

        /// <summary>
        /// Абсолютное время начала матча (UTC), источник истины для всех таймзон.
        /// </summary>
        public DateTimeOffset StartDateTimeUtc { get; }

        /// <summary>
        /// Название турнира (как пришло из API; нормализация и алиасы применяются отдельно).
        /// </summary>
        public string TournamentName { get; }

        public string HomeName { get; }
        public string AwayName { get; }

        /// <summary>
        /// Participant IDs (для будущих расширений; в Этапе 1 не используется для фильтрации).
        /// </summary>
        public IReadOnlyList<string> HomeParticipantIds { get; }
        public IReadOnlyList<string> AwayParticipantIds { get; }

        /// <summary>
        /// Опционально. Если поле присутствует в API - сохраняем.
        /// </summary>
        public string? EventStage { get; }

        public Match(
            string eventId,
            DateTimeOffset startDateTimeUtc,
            string tournamentName,
            string homeName,
            string awayName,
            IReadOnlyList<string> homeParticipantIds,
            IReadOnlyList<string> awayParticipantIds,
            string? eventStage = null)
        {
            if (string.IsNullOrWhiteSpace(eventId))
                throw new ArgumentException("eventId is required.", nameof(eventId));
            if (string.IsNullOrWhiteSpace(tournamentName))
                throw new ArgumentException("tournamentName is required.", nameof(tournamentName));
            if (string.IsNullOrWhiteSpace(homeName))
                throw new ArgumentException("homeName is required.", nameof(homeName));
            if (string.IsNullOrWhiteSpace(awayName))
                throw new ArgumentException("awayName is required.", nameof(awayName));

            EventId = eventId.Trim();
            StartDateTimeUtc = startDateTimeUtc;

            TournamentName = tournamentName.Trim();
            HomeName = homeName.Trim();
            AwayName = awayName.Trim();

            HomeParticipantIds = homeParticipantIds ?? Array.Empty<string>();
            AwayParticipantIds = awayParticipantIds ?? Array.Empty<string>();

            EventStage = string.IsNullOrWhiteSpace(eventStage) ? null : eventStage.Trim();
        }

        public override string ToString()
            => $"{StartDateTimeUtc:O} | {TournamentName} | {HomeName} vs {AwayName} (EventId={EventId})";
    }
}
