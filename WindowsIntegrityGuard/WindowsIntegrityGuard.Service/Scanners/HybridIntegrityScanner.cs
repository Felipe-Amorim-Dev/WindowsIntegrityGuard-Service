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
        private readonly IDashboardStateService _dashboardStateService;

        public HybridIntegrityScanner(SfcIntegrityScanner sfcScanner, IComponentStoreHealthService componentStoreHealthService, ISystemIntegrityClassifier classifier, ILogger<HybridIntegrityScanner> logger, IDashboardStateService dashboardStateService)
        {
            _sfcScanner = sfcScanner;
            _componentStoreHealthService = componentStoreHealthService;
            _classifier = classifier;
            _logger = logger;
            _dashboardStateService = dashboardStateService;
        }

        public async Task<IntegrityScanResult> ScanAsync(CancellationToken cancellationToken = default)
        {
            DateTimeOffset startedAt = DateTimeOffset.UtcNow;

            try
            {
                _logger.LogInformation("Iniciando verificação híbrida de integridade.");

                _dashboardStateService.UpdateOperation("Preparando verificação de arquivos protegidos...", 10);

                IntegrityScanResult sfcResult = await ExecuteSfcAsync(cancellationToken);

                _logger.LogInformation("Resultado SFC: {Status}.", sfcResult.Status);

                if (sfcResult.Status == IntegrityStatus.Failed)
                {
                    _dashboardStateService.UpdateOperation("Falha durante a verificação SFC.", 100);
                    return CreateResult(IntegrityStatus.Failed, "A verificação SFC falhou.", sfcResult, startedAt);
                }

                ComponentStoreHealthResult dismResult = await ExecuteComponentStoreScanAsync(cancellationToken);

                _logger.LogInformation("Resultado do Component Store: {Status}.", dismResult.Status);

                _dashboardStateService.UpdateOperation("Classificando resultado da verificação...", 90);

                IntegrityStatus finalStatus = _classifier.Classify(sfcResult.Status, dismResult.Status);
                string message = GetMessage(finalStatus, sfcResult.Status, dismResult.Status);

                _logger.LogInformation("Classificação final de integridade: {Status}.", finalStatus);

                _dashboardStateService.UpdateOperation("Finalizando verificação...", 95);

                return CreateResult(finalStatus, message, sfcResult, startedAt);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Falha inesperada durante a verificação híbrida de integridade.");
                _dashboardStateService.UpdateOperation("Falha inesperada durante a verificação.", null);

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

        private async Task<IntegrityScanResult> ExecuteSfcAsync(CancellationToken cancellationToken)
        {
            _dashboardStateService.UpdateOperation("Verificando arquivos protegidos do Windows...", 15);

            Task<IntegrityScanResult> scanTask = _sfcScanner.ScanAsync(cancellationToken);
            Task progressTask = MonitorProgressAsync(scanTask, 15, 60, "Verificando arquivos protegidos do Windows...", cancellationToken);

            IntegrityScanResult result = await scanTask;
            await progressTask;

            _dashboardStateService.UpdateOperation("Verificação dos arquivos protegidos concluída.", 60);

            return result;
        }

        private async Task<ComponentStoreHealthResult> ExecuteComponentStoreScanAsync(CancellationToken cancellationToken)
        {
            _dashboardStateService.UpdateOperation("Preparando verificação do Component Store...", 65);

            Task<ComponentStoreHealthResult> scanTask = _componentStoreHealthService.CheckHealthAsync(cancellationToken);
            Task progressTask = MonitorProgressAsync(scanTask, 65, 88, "Verificando Component Store do Windows...", cancellationToken);

            ComponentStoreHealthResult result = await scanTask;
            await progressTask;

            _dashboardStateService.UpdateOperation("Verificação do Component Store concluída.", 88);

            return result;
        }

        private async Task MonitorProgressAsync(Task operationTask, double startProgress, double maximumProgress, string operation, CancellationToken cancellationToken)
        {
            double progress = startProgress;

            while (!operationTask.IsCompleted)
            {
                cancellationToken.ThrowIfCancellationRequested();

                _dashboardStateService.UpdateOperation(operation, progress);

                await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);

                progress += 1;

                if (progress > maximumProgress)
                {
                    progress = maximumProgress;
                }
            }
        }

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
