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
    public sealed class DashboardStateService : IDashboardStateService
    {
        private readonly object _lock = new();
        private readonly ServiceStateManager _stateManager;

        private IntegrityStatus _integrityStatus = IntegrityStatus.NotStarted;
        private string _currentOperation = "Em espera";
        private DateTimeOffset? _lastScanAt;
        private double? _scanProgress;
        private int? _filesVerified;
        private int? _problemsFound;
        private int? _problemsRepaired;
        private TimeSpan? _lastScanDuration;

        public DashboardStateService(ServiceStateManager stateManager)
        {
            _stateManager = stateManager;
        }

        public DashboardSnapshot GetSnapshot()
        {
            lock (_lock)
            {
                return new DashboardSnapshot
                {
                    ServiceStatus = _stateManager.Status,
                    IntegrityStatus = _integrityStatus,
                    CurrentOperation = _currentOperation,
                    LastScanAt = _lastScanAt,
                    ScanProgress = _scanProgress,
                    FilesVerified = _filesVerified,
                    ProblemsFound = _problemsFound,
                    ProblemsRepaired = _problemsRepaired,
                    LastScanDuration = _lastScanDuration
                };
            }
        }

        public void MarkScanStarted()
        {
            lock (_lock)
            {
                _integrityStatus = IntegrityStatus.Scanning;
                _currentOperation = "Verificação em andamento";
                _scanProgress = null;
                _filesVerified = null;
                _problemsFound = null;
                _problemsRepaired = null;
            }
        }

        public void MarkScanCompleted(IntegrityScanResult result)
        {
            ArgumentNullException.ThrowIfNull(result);

            lock (_lock)
            {
                _integrityStatus = result.Status;
                _currentOperation = GetCompletedOperation(result.Status);
                _lastScanAt = result.FinishedAt;
                _lastScanDuration = result.FinishedAt - result.StartedAt;
                _scanProgress = null;
                _filesVerified = null;
                _problemsFound = result.Status == IntegrityStatus.Healthy ? 0 : null;
                _problemsRepaired = null;
            }
        }

        public void MarkScanFailed()
        {
            lock (_lock)
            {
                _integrityStatus = IntegrityStatus.Failed;
                _currentOperation = "Falha durante a verificação";
                _scanProgress = null;
            }
        }

        #region Metodos Privados
        private static string GetCompletedOperation(IntegrityStatus status)
        {
            return status switch
            {
                IntegrityStatus.Healthy => "Verificação concluída",
                IntegrityStatus.Corrupted => "Corrupção identificada",
                IntegrityStatus.Inconclusive => "Verificação inconclusiva",
                IntegrityStatus.Failed => "Falha durante a verificação",
                _ => "Operação concluída"
            };
        }
        #endregion
    }
}
