namespace WindowsIntegrityGuard.Service;

public sealed class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;

    public Worker(ILogger<Worker> logger)
    {
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Windows Integrity Guard iniciado em: {DateTime}", DateTimeOffset.Now);
        _logger.LogInformation("Serviço inicializado e aguardando operações.");

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