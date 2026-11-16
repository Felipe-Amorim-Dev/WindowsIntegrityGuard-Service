using WindowsIntegrityGuard.Core.Enums;
using WindowsIntegrityGuard.Core.Interfaces;
using WindowsIntegrityGuard.Core.Models;
using WindowsIntegrityGuard.Core.Services;

namespace WindowsIntegrityGuard.Service;

public sealed class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IScanCoordinator _scanCoordinator;
    private readonly IServiceSettingsService _serviceSettingsService;

    public Worker(ILogger<Worker> logger, IScanCoordinator scanCoordinator, IServiceSettingsService serviceSettingsService)
    {
        _logger = logger;
        _scanCoordinator = scanCoordinator;
        _serviceSettingsService = serviceSettingsService;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Windows Integrity Guard iniciado em {DateTime}", DateTimeOffset.Now);

        try
        {
            _logger.LogInformation("Aguardando 2 minutos antes de avaliar a verificação automática.");

            await Task.Delay(TimeSpan.FromMinutes(2), stoppingToken);

            ServiceSettings settings = await _serviceSettingsService.LoadAsync(stoppingToken);

            if (settings.ScanOnStartup)
            {
                bool started = _scanCoordinator.TryStartScan();

                if (started)
                {
                    _logger.LogInformation("Verificação automática de inicialização solicitada.");
                }
                else
                {
                    _logger.LogWarning("Não foi possível iniciar a verificação automática porque existe outra operação em andamento.");
                }
            }
            else
            {
                _logger.LogInformation("Verificação automática após inicialização desabilitada.");
            }

            await Task.Delay(Timeout.InfiniteTimeSpan, stoppingToken);
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("Encerramento solicitado pelo sistema.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Falha inesperada durante a execução do Worker.");
            throw;
        }
        finally
        {
            _logger.LogInformation("Windows Integrity Guard encerrado em {DateTime}", DateTimeOffset.Now);
        }
    }
}
