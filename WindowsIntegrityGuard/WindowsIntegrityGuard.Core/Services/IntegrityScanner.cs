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
    public sealed class IntegrityScanner : IIntegrityScanner
    {
        public Task<IntegrityScanResult> ScanAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            DateTimeOffset startedAt = DateTimeOffset.UtcNow;

            var result = new IntegrityScanResult
            {
                Status = IntegrityStatus.Inconclusive,
                Message = "O mecanismo de verificação ainda não foi integrado.",
                StartedAt = startedAt,
                FinishedAt = DateTimeOffset.UtcNow
            };

            return Task.FromResult(result);
        }
    }
}
