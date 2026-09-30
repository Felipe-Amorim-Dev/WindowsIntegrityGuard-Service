using WindowsIntegrityGuard.Core.Services;

namespace WindowsIntegrityGuard.Service;

public sealed class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly ServiceStateManager _stateManager;

    public Worker(ILogger<Worker> logger, ServiceStateManager stateManager)
    {
        _logger = logger;
        _stateManager = stateManager;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Windows Integrity Guard iniciado em: {DateTime}", DateTimeOffset.Now);
        _logger.LogInformation("Estado atual: {Status}", _stateManager.Status);

        try
        {
            await Task.Delay(Timeout.InfiniteTimeSpan, stoppingToken);
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("Solicitação de encerramento recebida.");
        }
        finally
        {
            _logger.LogInformation("Windows Integrity Guard encerrado em: {DateTime}", DateTimeOffset.Now);
        }
    }
}