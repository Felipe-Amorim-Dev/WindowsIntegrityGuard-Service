using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WindowsIntegrityGuard.Core.Models;

namespace WindowsIntegrityGuard.Core.Interfaces
{
    public interface IRepairHistoryService
    {
        Task AddAsync(RepairResult repairResult, CancellationToken cancellationToken = default);
        Task<IReadOnlyCollection<RepairHistoryEntry>> GetAllAsync(CancellationToken cancellationToken = default);
    }
}
