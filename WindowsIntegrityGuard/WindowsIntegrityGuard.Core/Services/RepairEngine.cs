using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using WindowsIntegrityGuard.Core.Enums;
using WindowsIntegrityGuard.Core.Interfaces;
using WindowsIntegrityGuard.Core.Models;

namespace WindowsIntegrityGuard.Core.Services
{
    public sealed class RepairEngine : IRepairEngine
    {
        private readonly ILogger<RepairEngine> _logger;
        private readonly ISfcRepairService _sfcRepairService;
        private readonly IDismRepairService _dismRepairService;
        private readonly IRepairValidationService _validationService;
        private readonly IRepairHistoryService _historyService;

        public RepairEngine(ILogger<RepairEngine> logger, ISfcRepairService sfcRepairService, IDismRepairService dismRepairService, IRepairValidationService validationService, IRepairHistoryService historyService)
        {
            _logger = logger;
            _sfcRepairService = sfcRepairService;
            _dismRepairService = dismRepairService;
            _validationService = validationService;
            _historyService = historyService;
        }

        public async Task<RepairResult> RepairAsync(RepairRequest request, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(request);
            cancellationToken.ThrowIfCancellationRequested();

            DateTimeOffset startedAt = DateTimeOffset.UtcNow;

            _logger.LogInformation("Iniciando avaliação para reparação.");

            try
            {
                if (!request.Assessment.IsRepairCandidate)
                {
                    RepairResult result = CreateResult(RepairStatus.NotRequired, "Nenhuma corrupção confirmada exige reparação.", startedAt);

                    _logger.LogInformation("Reparação não necessária.");
                    await SaveHistoryAsync(result, cancellationToken);

                    return result;
                }

                if (!request.AllowRepair)
                {
                    RepairResult result = CreateResult(RepairStatus.NotRequired, "A corrupção foi identificada, mas a reparação não está autorizada.", startedAt);

                    _logger.LogWarning("Foi identificada corrupção, porém a reparação não está autorizada.");
                    await SaveHistoryAsync(result, cancellationToken);

                    return result;
                }

                _logger.LogInformation("Executando SFC para tentativa inicial de reparação.");

                RepairCommandResult sfcResult = await _sfcRepairService.RepairAsync(cancellationToken);

                if (!sfcResult.IsSuccessful)
                {
                    RepairResult result = CreateResult(RepairStatus.Failed, $"O SFC não foi concluído com sucesso. {sfcResult.Message}", startedAt, true, false, false, sfcResult.ExitCode);

                    _logger.LogError("Falha na execução inicial do SFC. ExitCode: {ExitCode}", sfcResult.ExitCode);
                    await SaveHistoryAsync(result, cancellationToken);

                    return result;
                }

                _logger.LogInformation("SFC concluído. Iniciando validação pós-reparo.");

                RepairValidationResult validationResult = await _validationService.ValidateAsync(cancellationToken);

                if (validationResult.IsSuccessful)
                {
                    RepairResult result = CreateResult(RepairStatus.Completed, "O SFC reparou o sistema e a validação confirmou a integridade.", startedAt, true, false, true, sfcResult.ExitCode);

                    _logger.LogInformation("A validação confirmou a integridade após o SFC.");
                    await SaveHistoryAsync(result, cancellationToken);

                    return result;
                }

                _logger.LogWarning("A validação após o SFC não confirmou a integridade. Iniciando DISM.");

                RepairCommandResult dismResult = await _dismRepairService.RepairAsync(cancellationToken);

                if (!dismResult.IsSuccessful)
                {
                    RepairResult result = CreateResult(RepairStatus.Failed, $"O DISM não foi concluído com sucesso. {dismResult.Message}", startedAt, true, true, false, dismResult.ExitCode);

                    _logger.LogError("Falha na execução do DISM. ExitCode: {ExitCode}", dismResult.ExitCode);
                    await SaveHistoryAsync(result, cancellationToken);

                    return result;
                }

                _logger.LogInformation("DISM concluído. Executando nova verificação e reparação com SFC.");

                RepairCommandResult secondSfcResult = await _sfcRepairService.RepairAsync(cancellationToken);

                if (!secondSfcResult.IsSuccessful)
                {
                    RepairResult result = CreateResult(RepairStatus.Failed, $"O segundo SFC não foi concluído com sucesso. {secondSfcResult.Message}", startedAt, true, true, false, secondSfcResult.ExitCode);

                    _logger.LogError("Falha na segunda execução do SFC. ExitCode: {ExitCode}", secondSfcResult.ExitCode);
                    await SaveHistoryAsync(result, cancellationToken);

                    return result;
                }

                _logger.LogInformation("Segundo SFC concluído. Executando validação final.");

                RepairValidationResult finalValidation = await _validationService.ValidateAsync(cancellationToken);

                if (!finalValidation.IsSuccessful)
                {
                    RepairResult result = CreateResult(RepairStatus.Failed, "O sistema ainda apresenta problemas após SFC e DISM.", startedAt, true, true, false, secondSfcResult.ExitCode);

                    _logger.LogError("A validação final não confirmou a integridade do sistema.");
                    await SaveHistoryAsync(result, cancellationToken);

                    return result;
                }

                RepairResult completedResult = CreateResult(RepairStatus.Completed, "A integridade do sistema foi restaurada com sucesso.", startedAt, true, true, true, secondSfcResult.ExitCode);

                _logger.LogInformation("Fluxo de reparação concluído com sucesso.");
                await SaveHistoryAsync(completedResult, cancellationToken);

                return completedResult;
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                _logger.LogWarning("A operação de reparação foi cancelada.");
                throw;
            }
            catch (Exception ex)
            {
                RepairResult result = CreateResult(RepairStatus.Failed, "Ocorreu uma falha inesperada durante o processo de reparação.", startedAt);

                _logger.LogError(ex, "Falha inesperada durante o Repair Engine.");

                try
                {
                    await SaveHistoryAsync(result, CancellationToken.None);
                }
                catch (Exception historyException)
                {
                    _logger.LogError(historyException, "Não foi possível registrar a falha no histórico de reparações.");
                }

                return result;
            }
        }

        private async Task SaveHistoryAsync(RepairResult result, CancellationToken cancellationToken)
        {
            try
            {
                await _historyService.AddAsync(result, cancellationToken);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Não foi possível salvar o resultado no histórico de reparações.");
            }
        }

        private static RepairResult CreateResult(RepairStatus status, string message, DateTimeOffset startedAt, bool sfcExecuted = false, bool dismExecuted = false, bool validationSuccessful = false, int? exitCode = null)
        {
            return new RepairResult
            {
                Status = status,
                Message = message,
                ExitCode = exitCode,
                StartedAt = startedAt,
                FinishedAt = DateTimeOffset.UtcNow,
                SfcExecuted = sfcExecuted,
                DismExecuted = dismExecuted,
                ValidationSuccessful = validationSuccessful
            };
        }
    }
}