using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

using FootballReport.Models;

namespace FootballReport.Api
{
    /// <summary>
    /// Единственный источник матчей на Этапе 1:
    /// GET /api/flashscore/football/live
    ///
    /// sportdb.dev требует:
    /// - Base: https://api.sportdb.dev
    /// - Header: X-API-Key: <KEY>
    /// </summary>
    public sealed class FlashscoreClient : IDisposable
    {
        private const string Endpoint = "/api/flashscore/football/live";

        private readonly HttpClient _http;
        private readonly bool _disposeHttpClient;

        public FlashscoreClient(string apiKey, string baseUrl)
        {
            if (string.IsNullOrWhiteSpace(apiKey))
                throw new ArgumentException("apiKey is required.", nameof(apiKey));
            if (string.IsNullOrWhiteSpace(baseUrl))
                throw new ArgumentException("baseUrl is required.", nameof(baseUrl));

            _http = new HttpClient
            {
                BaseAddress = new Uri(EnsureTrailingSlash(baseUrl.Trim()), UriKind.Absolute),
                Timeout = TimeSpan.FromSeconds(20)
            };
            _disposeHttpClient = true;

            // sportdb.dev: X-API-Key
            _http.DefaultRequestHeaders.Remove("X-API-Key");
            _http.DefaultRequestHeaders.TryAddWithoutValidation("X-API-Key", apiKey.Trim());

            _http.DefaultRequestHeaders.Accept.Clear();
            _http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _http.DefaultRequestHeaders.UserAgent.ParseAdd("FootballReport/1.0");
        }

        public async Task<IReadOnlyList<Match>> GetTodayMatchesAsync(CancellationToken ct)
        {
            using var req = new HttpRequestMessage(HttpMethod.Get, Endpoint);

            using var resp = await _http.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, ct).ConfigureAwait(false);

            if (!resp.IsSuccessStatusCode)
            {
                var body = await SafeReadBodyAsync(resp, ct).ConfigureAwait(false);
                throw new InvalidOperationException(
                    $"Flashscore request failed: HTTP {(int)resp.StatusCode} {resp.ReasonPhrase}. Body: {TrimBody(body)}");
            }

            var json = await resp.Content.ReadAsStringAsync(ct).ConfigureAwait(false);
            if (string.IsNullOrWhiteSpace(json))
                return Array.Empty<Match>();

            return ParseMatchesFromJson(json);
        }

        private static IReadOnlyList<Match> ParseMatchesFromJson(string json)
        {
            using var doc = JsonDocument.Parse(json);

            var results = new List<Match>(256);
            Walk(doc.RootElement, results);

            return results;
        }

        private static void Walk(JsonElement el, List<Match> results)
        {
            switch (el.ValueKind)
            {
                case JsonValueKind.Object:
                    if (TryParseMatchObject(el, out var match))
                        results.Add(match);

                    foreach (var p in el.EnumerateObject())
                        Walk(p.Value, results);
                    break;

                case JsonValueKind.Array:
                    foreach (var item in el.EnumerateArray())
                        Walk(item, results);
                    break;
            }
        }

        private static bool TryParseMatchObject(JsonElement obj, out Match match)
        {
            match = null!;

            var eventId = GetString(obj, "eventId");
            var startUtcStr = GetString(obj, "startDateTimeUtc");
            var tournamentName = GetString(obj, "tournamentName");
            var homeName = GetString(obj, "homeName");
            var awayName = GetString(obj, "awayName");

            if (string.IsNullOrWhiteSpace(eventId) ||
                string.IsNullOrWhiteSpace(startUtcStr) ||
                string.IsNullOrWhiteSpace(tournamentName) ||
                string.IsNullOrWhiteSpace(homeName) ||
                string.IsNullOrWhiteSpace(awayName))
            {
                return false;
            }

            if (!TryParseUtc(startUtcStr!, out var startUtc))
                return false;

            var homeIds = GetStringArray(obj, "homeParticipantIds");
            var awayIds = GetStringArray(obj, "awayParticipantIds");

            var eventStage = GetString(obj, "eventStage");

            match = new Match(
                eventId: eventId!,
                startDateTimeUtc: startUtc,
                tournamentName: tournamentName!,
                homeName: homeName!,
                awayName: awayName!,
                homeParticipantIds: homeIds,
                awayParticipantIds: awayIds,
                eventStage: eventStage
            );

            return true;
        }

        private static bool TryParseUtc(string startDateTimeUtc, out DateTimeOffset utc)
        {
            utc = default;

            if (!DateTimeOffset.TryParse(startDateTimeUtc, out var parsed))
                return false;

            utc = parsed.ToUniversalTime();
            return true;
        }

        private static string? GetString(JsonElement obj, string propName)
        {
            if (!obj.TryGetProperty(propName, out var p))
                return null;

            if (p.ValueKind == JsonValueKind.String)
                return p.GetString();

            if (p.ValueKind == JsonValueKind.Number ||
                p.ValueKind == JsonValueKind.True ||
                p.ValueKind == JsonValueKind.False)
            {
                return p.ToString();
            }

            return null;
        }

        private static IReadOnlyList<string> GetStringArray(JsonElement obj, string propName)
        {
            if (!obj.TryGetProperty(propName, out var p))
                return Array.Empty<string>();

            if (p.ValueKind != JsonValueKind.Array)
                return Array.Empty<string>();

            var list = new List<string>();
            foreach (var item in p.EnumerateArray())
            {
                var s = item.ValueKind == JsonValueKind.String ? item.GetString() : item.ToString();
                if (!string.IsNullOrWhiteSpace(s))
                    list.Add(s.Trim());
            }

            return list;
        }

        private static async Task<string> SafeReadBodyAsync(HttpResponseMessage resp, CancellationToken ct)
        {
            try { return await resp.Content.ReadAsStringAsync(ct).ConfigureAwait(false); }
            catch { return string.Empty; }
        }

        private static string TrimBody(string body)
        {
            if (string.IsNullOrWhiteSpace(body))
                return string.Empty;

            body = body.Trim();
            return body.Length <= 500 ? body : body.Substring(0, 500) + "...";
        }

        private static string EnsureTrailingSlash(string url)
            => url.EndsWith("/", StringComparison.Ordinal) ? url : (url + "/");

        public void Dispose()
        {
            if (_disposeHttpClient)
                _http.Dispose();
        }
    }
}