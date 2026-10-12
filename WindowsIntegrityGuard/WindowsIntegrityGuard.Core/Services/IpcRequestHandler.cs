using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WindowsIntegrityGuard.Core.Interfaces.Ipc;
using WindowsIntegrityGuard.Core.Interfaces;
using WindowsIntegrityGuard.Core.Models.Ipc;

namespace WindowsIntegrityGuard.Core.Services
{
    public sealed class IpcRequestHandler : IIpcRequestHandler
    {
        private readonly ServiceStateManager _stateManager;

        public IpcRequestHandler(ServiceStateManager stateManager)
        {
            _stateManager = stateManager;
        }

        public IpcResponse Handle(IpcRequest request)
        {
            ArgumentNullException.ThrowIfNull(request);

            if (string.IsNullOrWhiteSpace(request.Command))
            {
                return CreateErrorResponse("Comando não informado.");
            }

            string command = request.Command.Trim().ToLowerInvariant();

            return command switch
            {
                "ping" => CreatePingResponse(),
                "status" => CreateStatusResponse(),
                _ => CreateErrorResponse("Comando não reconhecido.")
            };
        }

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
    }
}
