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
        private readonly ServiceStateManager _stateManager;

        public DashboardStateService(ServiceStateManager stateManager)
        {
            _stateManager = stateManager;
        }

        public DashboardSnapshot GetSnapshot()
        {
            return new DashboardSnapshot
            {
                ServiceStatus = _stateManager.Status,
                IntegrityStatus = IntegrityStatus.NotStarted,
                CurrentOperation = GetCurrentOperation(_stateManager.Status),
                LastScanAt = null,
                ScanProgress = null,
                FilesVerified = null,
                ProblemsFound = null,
                ProblemsRepaired = null,
                LastScanDuration = null
            };
        }

        #region Metodos Privados
        private static string GetCurrentOperation(ServiceStatus status)
        {
            return status switch
            {
                ServiceStatus.Idle => "Em espera",
                ServiceStatus.Scanning => "Verificação em andamento",
                ServiceStatus.Repairing => "Reparação em andamento",
                ServiceStatus.Completed => "Operação concluída",
                ServiceStatus.Failed => "Falha na operação",
                _ => "Estado desconhecido"
            };
        }
        #endregion
    }
}
