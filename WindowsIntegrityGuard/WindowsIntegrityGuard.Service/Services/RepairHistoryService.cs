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
    public sealed class RepairHistoryService : IRepairHistoryService
    {
        private readonly string _filePath;
        private readonly SemaphoreSlim _semaphore = new(1, 1);
        private readonly JsonSerializerOptions _serializerOptions = new() { WriteIndented = true };

        public RepairHistoryService()
        {
            string dataDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "WindowsIntegrityGuard", "Data");

            Directory.CreateDirectory(dataDirectory);

            _filePath = Path.Combine(dataDirectory, "repair-history.json");
        }

        public async Task AddAsync(RepairResult repairResult, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(repairResult);

            await _semaphore.WaitAsync(cancellationToken);

            try
            {
                List<RepairHistoryEntry> history = await ReadHistoryAsync(cancellationToken);

                history.Add(new RepairHistoryEntry
                {
                    Status = repairResult.Status,
                    Message = repairResult.Message,
                    ExitCode = repairResult.ExitCode,
                    SfcExecuted = repairResult.SfcExecuted,
                    DismExecuted = repairResult.DismExecuted,
                    ValidationSuccessful = repairResult.ValidationSuccessful,
                    RebootRequired = repairResult.RebootRequired,
                    StartedAt = repairResult.StartedAt,
                    FinishedAt = repairResult.FinishedAt
                });

                string json = JsonSerializer.Serialize(history, _serializerOptions);

                await File.WriteAllTextAsync(_filePath, json, cancellationToken);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task<IReadOnlyCollection<RepairHistoryEntry>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            await _semaphore.WaitAsync(cancellationToken);

            try
            {
                return await ReadHistoryAsync(cancellationToken);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        private async Task<List<RepairHistoryEntry>> ReadHistoryAsync(CancellationToken cancellationToken)
        {
            if (!File.Exists(_filePath))
            {
                return [];
            }

            string json = await File.ReadAllTextAsync(_filePath, cancellationToken);

            if (string.IsNullOrWhiteSpace(json))
            {
                return [];
            }

            return JsonSerializer.Deserialize<List<RepairHistoryEntry>>(json, _serializerOptions) ?? [];
        }
    }
}
