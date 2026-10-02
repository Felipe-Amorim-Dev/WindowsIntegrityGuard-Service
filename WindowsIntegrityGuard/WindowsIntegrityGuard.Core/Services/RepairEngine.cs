using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WindowsIntegrityGuard.Core.Enums;
using WindowsIntegrityGuard.Core.Interfaces;
using WindowsIntegrityGuard.Core.Models;

namespace WindowsIntegrityGuard.Core.Services
{
    public sealed class RepairEngine : IRepairEngine
    {
        public Task<RepairResult> RepairAsync(RepairRequest request, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(request);
            cancellationToken.ThrowIfCancellationRequested();

            DateTimeOffset startedAt = DateTimeOffset.UtcNow;

            if (!request.Assessment.IsRepairCandidate)
            {
                return Task.FromResult(new RepairResult
                {
                    Status = RepairStatus.NotRequired,
                    Message = "Nenhuma corrupção confirmada exige reparação.",
                    StartedAt = startedAt,
                    FinishedAt = DateTimeOffset.UtcNow
                });
            }

            if (!request.AllowRepair)
            {
                return Task.FromResult(new RepairResult
                {
                    Status = RepairStatus.NotRequired,
                    Message = "Foi identificada corrupção, mas a reparação não está autorizada.",
                    StartedAt = startedAt,
                    FinishedAt = DateTimeOffset.UtcNow
                });
            }

            return Task.FromResult(new RepairResult
            {
                Status = RepairStatus.NotStarted,
                Message = "A reparação foi autorizada, mas o mecanismo de reparação ainda não foi integrado.",
                StartedAt = startedAt,
                FinishedAt = DateTimeOffset.UtcNow
            });
        }
    }
}
