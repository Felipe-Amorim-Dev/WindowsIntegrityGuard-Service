using WindowsIntegrityGuard.Core.Enums;
using WindowsIntegrityGuard.Core.Interfaces;
using WindowsIntegrityGuard.Core.Models;
using WindowsIntegrityGuard.Core.Services;

namespace WindowsIntegrityGuard.Service;

public sealed class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IConfiguration _configuration;
    private readonly IScanCoordinator _scanCoordinator;

    public Worker(ILogger<Worker> logger, IConfiguration configuration, IScanCoordinator scanCoordinator)
    {
        _logger = logger;
        _configuration = configuration;
        _scanCoordinator = scanCoordinator;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Windows Integrity Guard iniciado em {DateTime}", DateTimeOffset.Now);

        try
        {
            bool enableStartupScan = _configuration.GetValue<bool>("WindowsIntegrityGuard:EnableStartupScan", false);

            if (enableStartupScan)
            {
                bool started = _scanCoordinator.TryStartScan();

                if (started)
                {
                    _logger.LogInformation("Verificação automática de inicialização solicitada.");
                }
                else
                {
                    _logger.LogWarning("Não foi possível iniciar a verificação automática.");
                }
            }

            await Task.Delay(Timeout.InfiniteTimeSpan, stoppingToken);
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("Encerramento solicitado pelo sistema.");
        }
        finally
        {
            _logger.LogInformation("Windows Integrity Guard encerrado em {DateTime}", DateTimeOffset.Now);
        }
    }
}
