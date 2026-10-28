using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WindowsIntegrityGuard.Core.Interfaces.Ipc;
using WindowsIntegrityGuard.Core.Interfaces;
using WindowsIntegrityGuard.Core.Models.Ipc;
using WindowsIntegrityGuard.Core.Models;

namespace WindowsIntegrityGuard.Core.Services
{
    public sealed class IpcRequestHandler : IIpcRequestHandler
    {
        private readonly ServiceStateManager _stateManager;
        private readonly IDashboardStateService _dashboardStateService;
        private readonly IScanCoordinator _scanCoordinator;
        private readonly IScanHistoryService _scanHistoryService;

        public IpcRequestHandler(ServiceStateManager stateManager, IDashboardStateService dashboardStateService, IScanCoordinator scanCoordinator, IScanHistoryService scanHistoryService)
        {
            _stateManager = stateManager;
            _dashboardStateService = dashboardStateService;
            _scanCoordinator = scanCoordinator;
            _scanHistoryService = scanHistoryService;
        }

        public async Task<IpcResponse> HandleAsync(IpcRequest request, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(request.Command))
            {
                return CreateErrorResponse("Comando inválido.");
            }

            string command = request.Command.Trim().ToLowerInvariant();

            return command switch
            {
                "ping" => CreatePingResponse(),
                "status" => CreateStatusResponse(),
                "dashboard" => CreateDashboardResponse(),
                "start-scan" => CreateStartScanResponse(),
                "scan-history" => await CreateScanHistoryResponseAsync(cancellationToken),
                _ => CreateErrorResponse("Comando não reconhecido.")
            };
        }

        #region Metodos Privados
        private static IpcResponse CreatePingResponse()
        {
            return new IpcResponse
            {
                Success = true,
                Message = "Windows Integrity Guard Service disponível."
            };
        }

        private IpcResponse CreateStatusResponse()
        {
            var status = new ServiceStatusSnapshot
            {
                Status = _stateManager.Status,
                Timestamp = DateTimeOffset.UtcNow
            };

            return new IpcResponse
            {
                Success = true,
                Message = "Status obtido com sucesso.",
                Data = status
            };
        }

        private static IpcResponse CreateErrorResponse(string message)
        {
            return new IpcResponse
            {
                Success = false,
                Message = message
            };
        }

        private IpcResponse CreateDashboardResponse()
        {
            DashboardSnapshot snapshot = _dashboardStateService.GetSnapshot();

            return new IpcResponse
            {
                Success = true,
                Message = "Dashboard obtido com sucesso.",
                Data = snapshot
            };
        }

        private IpcResponse CreateStartScanResponse()
        {
            bool started = _scanCoordinator.TryStartScan();

            if (!started)
            {
                return new IpcResponse
                {
                    Success = false,
                    Message = "Não foi possível iniciar a verificação. Existe uma operação em andamento."
                };
            }

            return new IpcResponse
            {
                Success = true,
                Message = "Verificação iniciada com sucesso."
            };
        }

        private async Task<IpcResponse> CreateScanHistoryResponseAsync(CancellationToken cancellationToken)
        {
            IReadOnlyList<ScanHistoryEntry> history = await _scanHistoryService.GetAllAsync(cancellationToken);

            return new IpcResponse
            {
                Success = true,
                Message = "Histórico obtido com sucesso.",
                Data = history
            };
        }
        #endregion
    }
}
