using System.IO.Pipes;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;
using System.Text.Json;
using WindowsIntegrityGuard.Core.Interfaces;
using WindowsIntegrityGuard.Core.Interfaces.Ipc;
using WindowsIntegrityGuard.Core.Models.Ipc;

namespace WindowsIntegrityGuard.Service.Communication
{
    public sealed class NamedPipeServer : BackgroundService
    {
        private const string PipeName = "WindowsIntegrityGuard";
        private readonly ILogger<NamedPipeServer> _logger;
        private readonly IIpcRequestHandler _requestHandler;
        private readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web);

        public NamedPipeServer(ILogger<NamedPipeServer> logger, IIpcRequestHandler requestHandler)
        {
            _logger = logger;
            _requestHandler = requestHandler;
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

                    try
                    {
                        await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
                    }
                    catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                    {
                        break;
                    }
                }
            }

            _logger.LogInformation("Servidor IPC encerrado.");
        }

        #region Metodos Privados

        private async Task WaitForConnectionAsync(CancellationToken cancellationToken)
        {
            PipeSecurity pipeSecurity = CreatePipeSecurity();

            await using var pipe = NamedPipeServerStreamAcl.Create(
                PipeName,
                PipeDirection.InOut,
                1,
                PipeTransmissionMode.Byte,
                PipeOptions.Asynchronous,
                0,
                0,
                pipeSecurity);

            await pipe.WaitForConnectionAsync(cancellationToken);

            _logger.LogDebug("Cliente conectado ao IPC.");

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

            IpcResponse response = await _requestHandler.HandleAsync(request, cancellationToken);

            await WriteResponseAsync(writer, response);
        }

        private static PipeSecurity CreatePipeSecurity()
        {
            PipeSecurity pipeSecurity = new();

            pipeSecurity.SetAccessRuleProtection(true, false);

            SecurityIdentifier system = new(WellKnownSidType.LocalSystemSid, null);
            SecurityIdentifier administrators = new(WellKnownSidType.BuiltinAdministratorsSid, null);
            SecurityIdentifier users = new(WellKnownSidType.BuiltinUsersSid, null);

            pipeSecurity.AddAccessRule(new PipeAccessRule(system, PipeAccessRights.FullControl, AccessControlType.Allow));
            pipeSecurity.AddAccessRule(new PipeAccessRule(administrators, PipeAccessRights.FullControl, AccessControlType.Allow));
            pipeSecurity.AddAccessRule(new PipeAccessRule(users, PipeAccessRights.ReadWrite, AccessControlType.Allow));

            return pipeSecurity;
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

        #endregion
    }
}