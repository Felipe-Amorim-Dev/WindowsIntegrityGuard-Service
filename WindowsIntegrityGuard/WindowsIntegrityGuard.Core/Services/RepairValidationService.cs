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
    public sealed class RepairValidationService : IRepairValidationService
    {
        private readonly IIntegrityScanner _integrityScanner;

        public RepairValidationService(IIntegrityScanner integrityScanner)
        {
            _integrityScanner = integrityScanner;
        }

        public async Task<RepairValidationResult> ValidateAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            IntegrityScanResult result = await _integrityScanner.ScanAsync(cancellationToken);

            bool isSuccessful = result.Status == IntegrityStatus.Healthy;

            string message = result.Status switch
            {
                IntegrityStatus.Healthy => "A validação pós-reparação confirmou a integridade do sistema.",
                IntegrityStatus.Corrupted => "A corrupção ainda está presente após a tentativa de reparação.",
                IntegrityStatus.Inconclusive => "Não foi possível confirmar a integridade após a reparação.",
                IntegrityStatus.Failed => "A validação pós-reparação apresentou uma falha.",
                _ => "A validação pós-reparação não apresentou um resultado definitivo."
            };

            return new RepairValidationResult
            {
                IsSuccessful = isSuccessful,
                IntegrityStatus = result.Status,
                Message = message,
                ValidatedAt = DateTimeOffset.UtcNow
            };
        }
    }
}
