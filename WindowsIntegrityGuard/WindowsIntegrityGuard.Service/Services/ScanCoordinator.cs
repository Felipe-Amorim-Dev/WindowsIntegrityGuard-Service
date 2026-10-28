using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WindowsIntegrityGuard.Core.Enums;
using WindowsIntegrityGuard.Core.Interfaces;
using WindowsIntegrityGuard.Core.Models;
using WindowsIntegrityGuard.Core.Services;

namespace WindowsIntegrityGuard.Service.Services
{
    public sealed class ScanCoordinator : IScanCoordinator
    {
        private readonly object _lock = new();
        private readonly ILogger<ScanCoordinator> _logger;
        private readonly IIntegrityScanner _scanner;
        private readonly ServiceStateManager _stateManager;
        private readonly IDashboardStateService _dashboardStateService;
        private readonly IHostApplicationLifetime _applicationLifetime;
        private Task? _activeScan;
        private readonly IScanHistoryService _scanHistoryService;

        public ScanCoordinator(ILogger<ScanCoordinator> logger, IIntegrityScanner scanner, ServiceStateManager stateManager, IDashboardStateService dashboardStateService, IHostApplicationLifetime applicationLifetime, IScanHistoryService scanHistoryService)
        {
            _logger = logger;
            _scanner = scanner;
            _stateManager = stateManager;
            _dashboardStateService = dashboardStateService;
            _applicationLifetime = applicationLifetime;
            _scanHistoryService = scanHistoryService;
        }

        public bool TryStartScan()
        {
            lock (_lock)
            {
                if (_activeScan is { IsCompleted: false })
                {
                    return false;
                }

                if (_stateManager.Status is ServiceStatus.Scanning or ServiceStatus.Repairing)
                {
                    return false;
                }

                if (_stateManager.Status is ServiceStatus.Completed or ServiceStatus.Failed)
                {
                    if (!_stateManager.TryTransition(ServiceStatus.Idle))
                    {
                        return false;
                    }
                }

                if (!_stateManager.TryTransition(ServiceStatus.Scanning))
                {
                    return false;
                }

                _dashboardStateService.MarkScanStarted();
                _activeScan = ExecuteScanAsync(_applicationLifetime.ApplicationStopping);

                return true;
            }
        }

        private async Task ExecuteScanAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Iniciando verificação de integridade.");

            try
            {
                IntegrityScanResult result = await _scanner.ScanAsync(cancellationToken);

                _dashboardStateService.MarkScanCompleted(result);

                ScanHistoryEntry historyEntry = new()
                {
                    StartedAt = result.StartedAt,
                    FinishedAt = result.FinishedAt,
                    Duration = result.FinishedAt - result.StartedAt,
                    Status = result.Status,
                    Message = result.Message,
                    ExitCode = result.ExitCode,
                    RequiresRepairReview = result.RequiresRepairReview
                };

                await _scanHistoryService.AddAsync(
                    historyEntry,
                    cancellationToken);

                if (result.Status == IntegrityStatus.Failed)
                {
                    _stateManager.TryTransition(ServiceStatus.Failed);
                    _logger.LogError("A verificação de integridade falhou. {Message}", result.Message);
                    return;
                }

                _stateManager.TryTransition(ServiceStatus.Completed);

                _logger.LogInformation("Verificação concluída. Resultado: {Status}. {Message}", result.Status, result.Message);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                _logger.LogInformation("A verificação foi cancelada devido ao encerramento do Service.");

                _dashboardStateService.MarkScanFailed();
                _stateManager.TryTransition(ServiceStatus.Failed);
            }
            catch (Exception ex)
            {
                _dashboardStateService.MarkScanFailed();
                _stateManager.TryTransition(ServiceStatus.Failed);

                _logger.LogError(ex, "Falha inesperada durante a verificação de integridade.");
            }
        }
    }
}
