using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using WindowsIntegrityGuard.Core.Interfaces;
using WindowsIntegrityGuard.Core.Models;

namespace WindowsIntegrityGuard.Service.Services
{
    public sealed class ScanHistoryService : IScanHistoryService
    {
        private readonly SemaphoreSlim _semaphore = new(1, 1);
        private readonly ILogger<ScanHistoryService> _logger;
        private readonly string _historyPath;

        public ScanHistoryService(ILogger<ScanHistoryService> logger)
        {
            _logger = logger;

            string dataDirectory = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                "WindowsIntegrityGuard",
                "Data");

            Directory.CreateDirectory(dataDirectory);

            _historyPath = Path.Combine(dataDirectory, "scan-history.json");
        }

        public async Task AddAsync(
            ScanHistoryEntry entry,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(entry);

            await _semaphore.WaitAsync(cancellationToken);

            try
            {
                List<ScanHistoryEntry> history = await ReadHistoryAsync(cancellationToken);

                history.Add(entry);

                string json = JsonSerializer.Serialize(
                    history,
                    new JsonSerializerOptions
                    {
                        WriteIndented = true
                    });

                await File.WriteAllTextAsync(
                    _historyPath,
                    json,
                    cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Falha ao salvar histórico de verificações.");
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task<IReadOnlyList<ScanHistoryEntry>> GetAllAsync(
            CancellationToken cancellationToken = default)
        {
            await _semaphore.WaitAsync(cancellationToken);

            try
            {
                List<ScanHistoryEntry> history =
                    await ReadHistoryAsync(cancellationToken);

                return history
                    .OrderByDescending(x => x.FinishedAt)
                    .ToList();
            }
            finally
            {
                _semaphore.Release();
            }
        }

        private async Task<List<ScanHistoryEntry>> ReadHistoryAsync(
            CancellationToken cancellationToken)
        {
            if (!File.Exists(_historyPath))
            {
                return [];
            }

            string json = await File.ReadAllTextAsync(
                _historyPath,
                cancellationToken);

            if (string.IsNullOrWhiteSpace(json))
            {
                return [];
            }

            return JsonSerializer.Deserialize<List<ScanHistoryEntry>>(json) ?? [];
        }
    }
}
