using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WindowsIntegrityGuard.Core.Enums;
using WindowsIntegrityGuard.Core.Interfaces;
using WindowsIntegrityGuard.Core.Models;

namespace WindowsIntegrityGuard.Core.Services
{
    public sealed class RepairEngine : IRepairEngine
    {
        private readonly ISfcRepairService _sfcRepairService;
        private readonly IDismRepairService _dismRepairService;
        private readonly IRepairValidationService _validationService;

        public RepairEngine(ISfcRepairService sfcRepairService, IDismRepairService dismRepairService, IRepairValidationService validationService)
        {
            _sfcRepairService = sfcRepairService;
            _dismRepairService = dismRepairService;
            _validationService = validationService;
        }

        public async Task<RepairResult> RepairAsync(RepairRequest request, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(request);
            cancellationToken.ThrowIfCancellationRequested();

            DateTimeOffset startedAt = DateTimeOffset.UtcNow;

            if (!request.Assessment.IsRepairCandidate)
            {
                return CreateResult(RepairStatus.NotRequired, "Nenhuma corrupção confirmada exige reparação.", startedAt);
            }

            if (!request.AllowRepair)
            {
                return CreateResult(RepairStatus.NotRequired, "A corrupção foi identificada, mas a reparação não está autorizada.", startedAt);
            }

            RepairCommandResult sfcResult = await _sfcRepairService.RepairAsync(cancellationToken);

            if (!sfcResult.IsSuccessful)
            {
                return CreateResult(RepairStatus.Failed, $"O SFC não foi concluído com sucesso. {sfcResult.Message}", startedAt, true, false, false, sfcResult.ExitCode);
            }

            RepairValidationResult validationResult = await _validationService.ValidateAsync(cancellationToken);

            if (validationResult.IsSuccessful)
            {
                return CreateResult(RepairStatus.Completed, "O SFC reparou o sistema e a validação confirmou a integridade.", startedAt, true, false, true, sfcResult.ExitCode);
            }

            RepairCommandResult dismResult = await _dismRepairService.RepairAsync(cancellationToken);

            if (!dismResult.IsSuccessful)
            {
                return CreateResult(RepairStatus.Failed, $"O DISM não foi concluído com sucesso. {dismResult.Message}", startedAt, true, true, false, dismResult.ExitCode);
            }

            RepairCommandResult secondSfcResult = await _sfcRepairService.RepairAsync(cancellationToken);

            if (!secondSfcResult.IsSuccessful)
            {
                return CreateResult(RepairStatus.Failed, $"O segundo SFC não foi concluído com sucesso. {secondSfcResult.Message}", startedAt, true, true, false, secondSfcResult.ExitCode);
            }

            RepairValidationResult finalValidation = await _validationService.ValidateAsync(cancellationToken);

            if (!finalValidation.IsSuccessful)
            {
                return CreateResult(RepairStatus.Failed, "O sistema ainda apresenta problemas após SFC e DISM.", startedAt, true, true, false, secondSfcResult.ExitCode);
            }

            return CreateResult(RepairStatus.Completed, "A integridade do sistema foi restaurada com sucesso.", startedAt, true, true, true, secondSfcResult.ExitCode);
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