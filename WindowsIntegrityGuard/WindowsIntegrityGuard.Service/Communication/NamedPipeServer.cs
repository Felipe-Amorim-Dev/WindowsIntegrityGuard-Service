using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using WindowsIntegrityGuard.Core.Interfaces.Ipc;
using WindowsIntegrityGuard.Core.Models.Ipc;
using WindowsIntegrityGuard.Core.Services;

namespace WindowsIntegrityGuard.Service.Communication
{
    public sealed class NamedPipeServer : BackgroundService
    {
        private const string PipeName = "WindowsIntegrityGuard";
        private readonly ILogger<NamedPipeServer> _logger;
        private readonly ServiceStateManager _stateManager;
        private readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web);

        public NamedPipeServer(ILogger<NamedPipeServer> logger, ServiceStateManager stateManager)
        {
            _logger = logger;
            _stateManager = stateManager;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Servidor IPC iniciado. Pipe: {PipeName}", PipeName);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await WaitForConnectionAsync(stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Falha durante a comunicação IPC.");
                    await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
                }
            }

            _logger.LogInformation("Servidor IPC encerrado.");
        }

        private async Task WaitForConnectionAsync(CancellationToken cancellationToken)
        {
            await using var pipe = new NamedPipeServerStream(PipeName, PipeDirection.InOut, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);

            await pipe.WaitForConnectionAsync(cancellationToken);

            _logger.LogInformation("Cliente conectado ao IPC.");

            using var reader = new StreamReader(pipe, Encoding.UTF8, false, 1024, true);
            using var writer = new StreamWriter(pipe, new UTF8Encoding(false), 1024, true) { AutoFlush = true };

            string? requestJson = await reader.ReadLineAsync(cancellationToken);

            if (string.IsNullOrWhiteSpace(requestJson))
            {
                await WriteResponseAsync(writer, CreateErrorResponse("Requisição vazia."));
                return;
            }

            IpcRequest? request;

            try
            {
                request = JsonSerializer.Deserialize<IpcRequest>(requestJson, _jsonOptions);
            }
            catch (JsonException)
            {
                await WriteResponseAsync(writer, CreateErrorResponse("JSON inválido."));
                return;
            }

            if (request is null || string.IsNullOrWhiteSpace(request.Command))
            {
                await WriteResponseAsync(writer, CreateErrorResponse("Comando não informado."));
                return;
            }

            IpcResponse response = ProcessRequest(request);

            await WriteResponseAsync(writer, response);
        }

        private IpcResponse ProcessRequest(IpcRequest request)
        {
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

        private async Task WriteResponseAsync(StreamWriter writer, IpcResponse response)
        {
            string json = JsonSerializer.Serialize(response, _jsonOptions);
            await writer.WriteLineAsync(json);
        }
    }
}
