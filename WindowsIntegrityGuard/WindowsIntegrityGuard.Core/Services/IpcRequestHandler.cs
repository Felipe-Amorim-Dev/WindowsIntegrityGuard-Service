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
        private readonly IServiceSettingsService _serviceSettingsService;

        public IpcRequestHandler(ServiceStateManager stateManager, IDashboardStateService dashboardStateService, IScanCoordinator scanCoordinator, IScanHistoryService scanHistoryService, IServiceSettingsService serviceSettingsService)
        {
            _stateManager = stateManager;
            _dashboardStateService = dashboardStateService;
            _scanCoordinator = scanCoordinator;
            _scanHistoryService = scanHistoryService;
            _serviceSettingsService = serviceSettingsService;
        }

        public async Task<IpcResponse> HandleAsync(IpcRequest request, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(request.Command))
            {
                return CreateErrorResponse("Comando inválido.");
            }

            string command = request.Command.Trim().ToLowerInvariant();

            if (request.Command.StartsWith("set-scan-on-startup:", StringComparison.OrdinalIgnoreCase))
            {
                return await SetScanOnStartupAsync(request.Command, cancellationToken);
            }

            return command switch
            {
                "ping" => CreatePingResponse(),
                "status" => CreateStatusResponse(),
                "dashboard" => CreateDashboardResponse(),
                "start-scan" => CreateStartScanResponse(),
                "scan-history" => await CreateScanHistoryResponseAsync(cancellationToken),
                "get-scan-on-startup" => await GetScanOnStartupAsync(cancellationToken),
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

        private async Task<IpcResponse> GetScanOnStartupAsync(CancellationToken cancellationToken)
        {
            ServiceSettings settings = await _serviceSettingsService.LoadAsync(cancellationToken);

            return new IpcResponse
            {
                Success = true,
                Message = "Configuração carregada.",
                Data = settings.ScanOnStartup,
                Timestamp = DateTimeOffset.UtcNow
            };
        }

        private async Task<IpcResponse> SetScanOnStartupAsync(string command, CancellationToken cancellationToken)
        {
            string value = command["set-scan-on-startup:".Length..];

            if (!bool.TryParse(value, out bool enabled))
            {
                return new IpcResponse
                {
                    Success = false,
                    Message = "Valor inválido para ScanOnStartup.",
                    Timestamp = DateTimeOffset.UtcNow
                };
            }

            await _serviceSettingsService.SetScanOnStartupAsync(enabled, cancellationToken);

            return new IpcResponse
            {
                Success = true,
                Message = enabled ? "Verificação automática habilitada." : "Verificação automática desabilitada.",
                Data = enabled,
                Timestamp = DateTimeOffset.UtcNow
            };
        }

        private sealed class FakeServiceSettingsService : IServiceSettingsService
        {
            private ServiceSettings _settings = new();

            public Task<ServiceSettings> LoadAsync(CancellationToken cancellationToken = default)
            {
                return Task.FromResult(_settings);
            }

            public Task SaveAsync(ServiceSettings settings, CancellationToken cancellationToken = default)
            {
                _settings = settings;
                return Task.CompletedTask;
            }

            public Task SetScanOnStartupAsync(bool enabled, CancellationToken cancellationToken = default)
            {
                _settings.ScanOnStartup = enabled;
                return Task.CompletedTask;
            }
        }
        #endregion
    }
}
