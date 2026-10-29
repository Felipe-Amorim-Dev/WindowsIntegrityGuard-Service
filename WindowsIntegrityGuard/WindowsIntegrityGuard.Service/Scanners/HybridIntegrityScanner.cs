using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WindowsIntegrityGuard.Core.Enums;
using WindowsIntegrityGuard.Core.Interfaces;
using WindowsIntegrityGuard.Core.Models;

namespace WindowsIntegrityGuard.Service.Scanners
{
    public sealed class HybridIntegrityScanner : IIntegrityScanner
    {
        private readonly SfcIntegrityScanner _sfcScanner;
        private readonly IComponentStoreHealthService _componentStoreHealthService;
        private readonly ISystemIntegrityClassifier _classifier;
        private readonly ILogger<HybridIntegrityScanner> _logger;

        public HybridIntegrityScanner(SfcIntegrityScanner sfcScanner, IComponentStoreHealthService componentStoreHealthService, ISystemIntegrityClassifier classifier, ILogger<HybridIntegrityScanner> logger)
        {
            _sfcScanner = sfcScanner;
            _componentStoreHealthService = componentStoreHealthService;
            _classifier = classifier;
            _logger = logger;
        }

        public async Task<IntegrityScanResult> ScanAsync(CancellationToken cancellationToken = default)
        {
            DateTimeOffset startedAt = DateTimeOffset.UtcNow;

            try
            {
                _logger.LogInformation("Iniciando verificação híbrida de integridade.");

                IntegrityScanResult sfcResult = await _sfcScanner.ScanAsync(cancellationToken);

                _logger.LogInformation("Resultado SFC: {Status}.", sfcResult.Status);

                if (sfcResult.Status == IntegrityStatus.Failed)
                {
                    return CreateResult(IntegrityStatus.Failed, "A verificação SFC falhou.", sfcResult, startedAt);
                }

                ComponentStoreHealthResult dismResult = await _componentStoreHealthService.CheckHealthAsync(cancellationToken);

                _logger.LogInformation("Resultado do Component Store: {Status}.", dismResult.Status);

                IntegrityStatus finalStatus = _classifier.Classify(sfcResult.Status, dismResult.Status);
                string message = GetMessage(finalStatus, sfcResult.Status, dismResult.Status);

                _logger.LogInformation("Classificação final de integridade: {Status}.", finalStatus);

                return CreateResult(finalStatus, message, sfcResult, startedAt);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Falha inesperada durante a verificação híbrida de integridade.");

                return new IntegrityScanResult
                {
                    Status = IntegrityStatus.Failed,
                    Message = "Falha inesperada durante a verificação de integridade.",
                    StartedAt = startedAt,
                    FinishedAt = DateTimeOffset.UtcNow
                };
            }
        }

        #region Metodos Privados

        private static IntegrityScanResult CreateResult(IntegrityStatus status, string message, IntegrityScanResult sfcResult, DateTimeOffset startedAt)
        {
            return new IntegrityScanResult
            {
                Status = status,
                Message = message,
                Output = sfcResult.Output,
                ExitCode = sfcResult.ExitCode,
                StartedAt = startedAt,
                FinishedAt = DateTimeOffset.UtcNow
            };
        }

        private static string GetMessage(IntegrityStatus finalStatus, IntegrityStatus sfcStatus, ComponentStoreHealthStatus componentStoreStatus)
        {
            if (finalStatus == IntegrityStatus.Healthy)
            {
                return "Os arquivos protegidos do Windows e o Component Store estão íntegros.";
            }

            if (sfcStatus == IntegrityStatus.Corrupted)
            {
                return "O SFC identificou violações de integridade nos arquivos protegidos do Windows.";
            }

            if (componentStoreStatus == ComponentStoreHealthStatus.Repairable)
            {
                return "O Component Store do Windows possui corrupção reparável.";
            }

            if (componentStoreStatus == ComponentStoreHealthStatus.NonRepairable)
            {
                return "O Component Store do Windows possui corrupção não reparável.";
            }

            if (finalStatus == IntegrityStatus.Failed)
            {
                return "A verificação de integridade não pôde ser concluída.";
            }

            return "Não foi possível determinar a integridade completa do sistema com segurança.";
        }

        #endregion
    }
}
