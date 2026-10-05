using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WindowsIntegrityGuard.Core.Interfaces;
using WindowsIntegrityGuard.Core.Models;

namespace WindowsIntegrityGuard.Tests.Testes
{
    public sealed class FakeRepairHistoryService : IRepairHistoryService
    {
        public List<RepairResult> Results { get; } = [];

        public Task AddAsync(RepairResult repairResult, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            Results.Add(repairResult);

            return Task.CompletedTask;
        }

        public Task<IReadOnlyCollection<RepairHistoryEntry>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            return Task.FromResult<IReadOnlyCollection<RepairHistoryEntry>>([]);
        }
    }
}
