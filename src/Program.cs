using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FootballReport.Models;

namespace FootballReport
{
    internal static class Program
    {
        public static async Task<int> Main(string[] args)
        {
            try
            {
                var cts = new CancellationTokenSource();
                Console.CancelKeyPress += (_, e) =>
                {
                    e.Cancel = true;
                    cts.Cancel();
                };

                if (string.IsNullOrWhiteSpace(Secrets.ApiKey) || string.IsNullOrWhiteSpace(Secrets.BaseUrl))
                {
                    Console.WriteLine("Secrets.ApiKey / Secrets.BaseUrl are not configured.");
                    return 2;
                }

                var result = await ReportPipeline.RunAsync(
                    cts.Token,
                    progress: new Progress<ReportPipeline.ProgressUpdate>(p => Console.WriteLine($"{p.Step} -> {p.Message}")),
                    log: new Progress<string>(Console.WriteLine));

                if (!result.Success)
                {
                    Console.WriteLine($"Ошибка: {result.ErrorMessage}");
                    return 1;
                }

                if (result.Included.Count == 0)
                {
                    Console.WriteLine("Сегодня матчей нет");
                    return 0;
                }

                Console.WriteLine($"Готово. HTML файлов: {result.FilesRendered} (out/). Отобрано: {result.Included.Count}");
                return 0;
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("Операция отменена.");
                return 130;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка: " + ex.Message);
                Console.WriteLine(ex);
                return 1;
            }
        }

        internal static IReadOnlyList<Match> DedupByEventId(IReadOnlyList<Match> matches)
        {
            var result = new List<Match>(matches?.Count ?? 0);
            if (matches == null || matches.Count == 0)
                return result;

            var seen = new HashSet<string>(StringComparer.Ordinal);

            foreach (var m in matches)
            {
                if (m == null) continue;

                var id = (m.EventId ?? string.Empty).Trim();
                if (id.Length == 0) continue;

                if (seen.Add(id))
                    result.Add(m);
            }

            return result;
        }
    }
}
