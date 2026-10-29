using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WindowsIntegrityGuard.Core.Enums;
using WindowsIntegrityGuard.Core.Interfaces;
using WindowsIntegrityGuard.Core.Interfaces.Ipc;
using WindowsIntegrityGuard.Core.Models;
using WindowsIntegrityGuard.Core.Models.Ipc;
using WindowsIntegrityGuard.Core.Services;

namespace WindowsIntegrityGuard.Tests
{
    public sealed class IpcRequestHandlerTests
    {
        [Fact]
        public async Task HandleAsync_ShouldReturnSuccess_WhenCommandIsPing()
        {
            var stateManager = new ServiceStateManager();
            var dashboardStateService = new FakeDashboardStateService(stateManager);
            var scanCoordinator = new FakeScanCoordinator();
            var scanHistoryService = new FakeScanHistoryService();
            var handler = new IpcRequestHandler(stateManager, dashboardStateService, scanCoordinator, scanHistoryService);

            var request = new IpcRequest
            {
                Command = "ping"
            };

            IpcResponse response = await handler.HandleAsync(request);

            Assert.True(response.Success);
            Assert.Equal("Windows Integrity Guard Service disponível.", response.Message);
        }

        [Fact]
        public async Task HandleAsync_ShouldReturnCurrentStatus_WhenCommandIsStatus()
        {
            var stateManager = new ServiceStateManager();
            var dashboardStateService = new FakeDashboardStateService(stateManager);
            var scanCoordinator = new FakeScanCoordinator();
            var scanHistoryService = new FakeScanHistoryService();
            var handler = new IpcRequestHandler(stateManager, dashboardStateService, scanCoordinator, scanHistoryService);

            var request = new IpcRequest
            {
                Command = "status"
            };

            IpcResponse response = await handler.HandleAsync(request);

            Assert.True(response.Success);
            Assert.NotNull(response.Data);
        }

        [Fact]
        public async Task HandleAsync_ShouldReturnDashboard_WhenCommandIsDashboard()
        {
            var stateManager = new ServiceStateManager();
            var dashboardStateService = new FakeDashboardStateService(stateManager);
            var scanCoordinator = new FakeScanCoordinator();
            var scanHistoryService = new FakeScanHistoryService();
            var handler = new IpcRequestHandler(stateManager, dashboardStateService, scanCoordinator, scanHistoryService);

            var request = new IpcRequest
            {
                Command = "dashboard"
            };

            IpcResponse response = await handler.HandleAsync(request);

            Assert.True(response.Success);
            Assert.NotNull(response.Data);
        }

        [Fact]
        public async Task HandleAsync_ShouldStartScan_WhenCommandIsStartScan()
        {
            var stateManager = new ServiceStateManager();
            var dashboardStateService = new FakeDashboardStateService(stateManager);
            var scanCoordinator = new FakeScanCoordinator(true);
            var scanHistoryService = new FakeScanHistoryService();
            var handler = new IpcRequestHandler(stateManager, dashboardStateService, scanCoordinator, scanHistoryService);

            var request = new IpcRequest
            {
                Command = "start-scan"
            };

            IpcResponse response = await handler.HandleAsync(request);

            Assert.True(response.Success);
        }

        [Fact]
        public async Task HandleAsync_ShouldRejectScan_WhenScanCannotStart()
        {
            var stateManager = new ServiceStateManager();
            var dashboardStateService = new FakeDashboardStateService(stateManager);
            var scanCoordinator = new FakeScanCoordinator(false);
            var scanHistoryService = new FakeScanHistoryService();
            var handler = new IpcRequestHandler(stateManager, dashboardStateService, scanCoordinator, scanHistoryService);

            var request = new IpcRequest
            {
                Command = "start-scan"
            };

            IpcResponse response = await handler.HandleAsync(request);

            Assert.False(response.Success);
        }

        [Fact]
        public async Task HandleAsync_ShouldReturnError_WhenCommandIsUnknown()
        {
            var stateManager = new ServiceStateManager();
            var dashboardStateService = new FakeDashboardStateService(stateManager);
            var scanCoordinator = new FakeScanCoordinator();
            var scanHistoryService = new FakeScanHistoryService();
            var handler = new IpcRequestHandler(stateManager, dashboardStateService, scanCoordinator, scanHistoryService);

            var request = new IpcRequest
            {
                Command = "invalid-command"
            };

            IpcResponse response = await handler.HandleAsync(request);

            Assert.False(response.Success);
        }

        [Fact]
        public async Task HandleAsync_ShouldReturnError_WhenCommandIsEmpty()
        {
            var stateManager = new ServiceStateManager();
            var dashboardStateService = new FakeDashboardStateService(stateManager);
            var scanCoordinator = new FakeScanCoordinator();
            var scanHistoryService = new FakeScanHistoryService();
            var handler = new IpcRequestHandler(stateManager, dashboardStateService, scanCoordinator, scanHistoryService);

            var request = new IpcRequest
            {
                Command = string.Empty
            };

            IpcResponse response = await handler.HandleAsync(request);

            Assert.False(response.Success);
        }

        private sealed class FakeDashboardStateService : IDashboardStateService
        {
            private readonly ServiceStateManager _stateManager;

            public FakeDashboardStateService(ServiceStateManager stateManager)
            {
                _stateManager = stateManager;
            }

            public DashboardSnapshot GetSnapshot()
            {
                return new DashboardSnapshot
                {
                    ServiceStatus = _stateManager.Status,
                    IntegrityStatus = IntegrityStatus.NotStarted,
                    CurrentOperation = "Em espera"
                };
            }

            public void MarkScanStarted()
            {
            }

            public void MarkScanCompleted(IntegrityScanResult result)
            {
            }

            public void MarkScanFailed()
            {
            }
        }

        private sealed class FakeScanCoordinator : IScanCoordinator
        {
            private readonly bool _startResult;

            public FakeScanCoordinator(bool startResult = true)
            {
                _startResult = startResult;
            }

            public bool TryStartScan()
            {
                return _startResult;
            }
        }

        private sealed class FakeScanHistoryService : IScanHistoryService
        {
            public Task AddAsync(ScanHistoryEntry entry, CancellationToken cancellationToken = default)
            {
                return Task.CompletedTask;
            }

            public Task<IReadOnlyList<ScanHistoryEntry>> GetAllAsync(CancellationToken cancellationToken = default)
            {
                IReadOnlyList<ScanHistoryEntry> history = [];
                return Task.FromResult(history);
            }
        }
    }
}