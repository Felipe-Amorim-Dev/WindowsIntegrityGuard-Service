using WindowsIntegrityGuard.Core.Enums;
using WindowsIntegrityGuard.Core.Interfaces;
using WindowsIntegrityGuard.Core.Models;
using WindowsIntegrityGuard.Core.Services;

namespace WindowsIntegrityGuard.Service;

public sealed class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IConfiguration _configuration;
    private readonly ServiceStateManager _stateManager;
    private readonly IIntegrityScanner _scanner;
    private readonly IDashboardStateService _dashboardStateService;

    public Worker(ILogger<Worker> logger, IConfiguration configuration, ServiceStateManager stateManager, IIntegrityScanner scanner, IDashboardStateService dashboardStateService)
    {
        _logger = logger;
        _configuration = configuration;
        _stateManager = stateManager;
        _scanner = scanner;
        _dashboardStateService = dashboardStateService;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Windows Integrity Guard iniciado em {DateTime}", DateTimeOffset.Now);
        _logger.LogInformation("Estado atual: {Status}", _stateManager.Status);

        try
        {
            bool enableStartupScan = _configuration.GetValue<bool>("WindowsIntegrityGuard:EnableStartupScan", false);

            if (enableStartupScan && _stateManager.TryTransition(ServiceStatus.Scanning))
            {
                _dashboardStateService.MarkScanStarted();

                _logger.LogInformation("Iniciando verificação de integridade com SFC.");

                IntegrityScanResult result = await _scanner.ScanAsync(stoppingToken);

                _dashboardStateService.MarkScanCompleted(result);

                if (result.Status == IntegrityStatus.Failed)
                {
                    _stateManager.TryTransition(ServiceStatus.Failed);
                    _logger.LogError("Verificação não concluída: {Message}", result.Message);
                }
                else
                {
                    _stateManager.TryTransition(ServiceStatus.Completed);
                    _logger.LogInformation("Resultado da verificação: {Status}. {Message}", result.Status, result.Message);
                }

                _logger.LogInformation("Duração da verificação: {Duration}", result.FinishedAt - result.StartedAt);
            }

            _logger.LogInformation("Worker aguardando operações. Estado: {Status}", _stateManager.Status);
            await Task.Delay(Timeout.InfiniteTimeSpan, stoppingToken);
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("Encerramento solicitado pelo sistema.");
        }
        catch (Exception ex)
        {
            _stateManager.TryTransition(ServiceStatus.Failed);
            _logger.LogError(ex, "Falha inesperada durante a execução do Worker.");
            throw;
        }
        finally
        {
            _logger.LogInformation("Windows Integrity Guard encerrado em {DateTime}", DateTimeOffset.Now);
        }
    }
}