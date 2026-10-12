using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WindowsIntegrityGuard.Core.Enums;
using WindowsIntegrityGuard.Core.Interfaces.Ipc;
using WindowsIntegrityGuard.Core.Models.Ipc;
using WindowsIntegrityGuard.Core.Services;

namespace WindowsIntegrityGuard.Tests
{
    public sealed class IpcRequestHandlerTests
    {
        [Fact]
        public void Handle_ShouldReturnSuccess_WhenCommandIsPing()
        {
            var stateManager = new ServiceStateManager();
            var handler = new IpcRequestHandler(stateManager);

            var request = new IpcRequest
            {
                Command = "ping"
            };

            IpcResponse response = handler.Handle(request);

            Assert.True(response.Success);
            Assert.Equal("Windows Integrity Guard Service disponível.", response.Message);
        }

        [Fact]
        public void Handle_ShouldReturnCurrentStatus_WhenCommandIsStatus()
        {
            var stateManager = new ServiceStateManager();
            var handler = new IpcRequestHandler(stateManager);

            var request = new IpcRequest
            {
                Command = "status"
            };

            IpcResponse response = handler.Handle(request);

            Assert.True(response.Success);

            ServiceStatusSnapshot status = Assert.IsType<ServiceStatusSnapshot>(response.Data);

            Assert.Equal(ServiceStatus.Idle, status.Status);
        }

        [Fact]
        public void Handle_ShouldReturnError_WhenCommandIsUnknown()
        {
            var stateManager = new ServiceStateManager();
            var handler = new IpcRequestHandler(stateManager);

            var request = new IpcRequest
            {
                Command = "repair"
            };

            IpcResponse response = handler.Handle(request);

            Assert.False(response.Success);
            Assert.Equal("Comando não reconhecido.", response.Message);
        }

        [Fact]
        public void Handle_ShouldReturnError_WhenCommandIsEmpty()
        {
            var stateManager = new ServiceStateManager();
            var handler = new IpcRequestHandler(stateManager);

            var request = new IpcRequest
            {
                Command = string.Empty
            };

            IpcResponse response = handler.Handle(request);

            Assert.False(response.Success);
            Assert.Equal("Comando não informado.", response.Message);
        }

        [Fact]
        public void Handle_StatusCommand_ShouldNotChangeServiceStatus()
        {
            var stateManager = new ServiceStateManager();
            var handler = new IpcRequestHandler(stateManager);

            Assert.True(stateManager.TryTransition(ServiceStatus.Scanning));

            var request = new IpcRequest
            {
                Command = "status"
            };

            IpcResponse response = handler.Handle(request);

            ServiceStatusSnapshot status = Assert.IsType<ServiceStatusSnapshot>(response.Data);

            Assert.Equal(ServiceStatus.Scanning, status.Status);
            Assert.Equal(ServiceStatus.Scanning, stateManager.Status);
        }
    }
}
