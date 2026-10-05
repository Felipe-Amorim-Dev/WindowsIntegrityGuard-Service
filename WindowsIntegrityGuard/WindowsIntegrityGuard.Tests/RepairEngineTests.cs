using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WindowsIntegrityGuard.Core.Enums;
using WindowsIntegrityGuard.Core.Models;
using WindowsIntegrityGuard.Core.Services;
using WindowsIntegrityGuard.Tests.Testes;

namespace WindowsIntegrityGuard.Tests
{
    public sealed class RepairEngineTests
    {
        [Fact]
        public async Task RepairAsync_ShouldReturnNotRequired_WhenAssessmentIsNotRepairCandidate()
        {
            var sfc = new FakeSfcRepairService();
            var dism = new FakeDismRepairService(CreateSuccessCommandResult());
            var validation = new FakeRepairValidationService();
            var history = new FakeRepairHistoryService();

            var engine = new RepairEngine(NullLogger<RepairEngine>.Instance, sfc, dism, validation, history);

            var request = new RepairRequest
            {
                Assessment = CreateAssessment(IntegrityStatus.Healthy),
                AllowRepair = true
            };

            RepairResult result = await engine.RepairAsync(request);

            Assert.Equal(RepairStatus.NotRequired, result.Status);
            Assert.Equal(0, sfc.ExecutionCount);
            Assert.Equal(0, dism.ExecutionCount);
            Assert.Single(history.Results);
        }

        [Fact]
        public async Task RepairAsync_ShouldReturnNotRequired_WhenRepairIsNotAuthorized()
        {
            var sfc = new FakeSfcRepairService();
            var dism = new FakeDismRepairService(CreateSuccessCommandResult());
            var validation = new FakeRepairValidationService();
            var history = new FakeRepairHistoryService();

            var engine = new RepairEngine(NullLogger<RepairEngine>.Instance, sfc, dism, validation, history);

            var request = new RepairRequest
            {
                Assessment = CreateAssessment(IntegrityStatus.Corrupted),
                AllowRepair = false
            };

            RepairResult result = await engine.RepairAsync(request);

            Assert.Equal(RepairStatus.NotRequired, result.Status);
            Assert.Equal(0, sfc.ExecutionCount);
            Assert.Single(history.Results);
        }

        [Fact]
        public async Task RepairAsync_ShouldComplete_WhenFirstSfcRepairsSystem()
        {
            var sfc = new FakeSfcRepairService(CreateSuccessCommandResult());
            var dism = new FakeDismRepairService(CreateSuccessCommandResult());
            var validation = new FakeRepairValidationService(CreateValidationResult(true));
            var history = new FakeRepairHistoryService();

            var engine = new RepairEngine(NullLogger<RepairEngine>.Instance, sfc, dism, validation, history);

            var request = new RepairRequest
            {
                Assessment = CreateAssessment(IntegrityStatus.Corrupted),
                AllowRepair = true
            };

            RepairResult result = await engine.RepairAsync(request);

            Assert.Equal(RepairStatus.Completed, result.Status);
            Assert.True(result.SfcExecuted);
            Assert.False(result.DismExecuted);
            Assert.True(result.ValidationSuccessful);
            Assert.Equal(1, sfc.ExecutionCount);
            Assert.Equal(0, dism.ExecutionCount);
            Assert.Single(history.Results);
        }

        [Fact]
        public async Task RepairAsync_ShouldUseDism_WhenFirstValidationFails()
        {
            var sfc = new FakeSfcRepairService(CreateSuccessCommandResult(), CreateSuccessCommandResult());
            var dism = new FakeDismRepairService(CreateSuccessCommandResult());
            var validation = new FakeRepairValidationService(CreateValidationResult(false), CreateValidationResult(true));
            var history = new FakeRepairHistoryService();

            var engine = new RepairEngine(NullLogger<RepairEngine>.Instance, sfc, dism, validation, history);

            var request = new RepairRequest
            {
                Assessment = CreateAssessment(IntegrityStatus.Corrupted),
                AllowRepair = true
            };

            RepairResult result = await engine.RepairAsync(request);

            Assert.Equal(RepairStatus.Completed, result.Status);
            Assert.True(result.SfcExecuted);
            Assert.True(result.DismExecuted);
            Assert.True(result.ValidationSuccessful);
            Assert.Equal(2, sfc.ExecutionCount);
            Assert.Equal(1, dism.ExecutionCount);
            Assert.Equal(2, validation.ExecutionCount);
            Assert.Single(history.Results);
        }

        [Fact]
        public async Task RepairAsync_ShouldFail_WhenInitialSfcFails()
        {
            var sfc = new FakeSfcRepairService(CreateFailedCommandResult());
            var dism = new FakeDismRepairService(CreateSuccessCommandResult());
            var validation = new FakeRepairValidationService();
            var history = new FakeRepairHistoryService();

            var engine = new RepairEngine(NullLogger<RepairEngine>.Instance, sfc, dism, validation, history);

            var request = new RepairRequest
            {
                Assessment = CreateAssessment(IntegrityStatus.Corrupted),
                AllowRepair = true
            };

            RepairResult result = await engine.RepairAsync(request);

            Assert.Equal(RepairStatus.Failed, result.Status);
            Assert.Equal(1, sfc.ExecutionCount);
            Assert.Equal(0, dism.ExecutionCount);
            Assert.Single(history.Results);
        }

        [Fact]
        public async Task RepairAsync_ShouldFail_WhenDismFails()
        {
            var sfc = new FakeSfcRepairService(CreateSuccessCommandResult());
            var dism = new FakeDismRepairService(CreateFailedCommandResult());
            var validation = new FakeRepairValidationService(CreateValidationResult(false));
            var history = new FakeRepairHistoryService();

            var engine = new RepairEngine(NullLogger<RepairEngine>.Instance, sfc, dism, validation, history);

            var request = new RepairRequest
            {
                Assessment = CreateAssessment(IntegrityStatus.Corrupted),
                AllowRepair = true
            };

            RepairResult result = await engine.RepairAsync(request);

            Assert.Equal(RepairStatus.Failed, result.Status);
            Assert.True(result.DismExecuted);
            Assert.Equal(1, dism.ExecutionCount);
            Assert.Single(history.Results);
        }

        [Fact]
        public async Task RepairAsync_ShouldFail_WhenFinalValidationFails()
        {
            var sfc = new FakeSfcRepairService(CreateSuccessCommandResult(), CreateSuccessCommandResult());
            var dism = new FakeDismRepairService(CreateSuccessCommandResult());
            var validation = new FakeRepairValidationService(CreateValidationResult(false), CreateValidationResult(false));
            var history = new FakeRepairHistoryService();

            var engine = new RepairEngine(NullLogger<RepairEngine>.Instance, sfc, dism, validation, history);

            var request = new RepairRequest
            {
                Assessment = CreateAssessment(IntegrityStatus.Corrupted),
                AllowRepair = true
            };

            RepairResult result = await engine.RepairAsync(request);

            Assert.Equal(RepairStatus.Failed, result.Status);
            Assert.True(result.SfcExecuted);
            Assert.True(result.DismExecuted);
            Assert.False(result.ValidationSuccessful);
            Assert.Single(history.Results);
        }

        private static IntegrityAssessment CreateAssessment(IntegrityStatus status)
        {
            return new IntegrityAssessment
            {
                Status = status,
                AssessedAt = DateTimeOffset.UtcNow
            };
        }

        private static RepairCommandResult CreateSuccessCommandResult()
        {
            return new RepairCommandResult
            {
                Status = RepairCommandStatus.Success,
                ExitCode = 0,
                StartedAt = DateTimeOffset.UtcNow,
                FinishedAt = DateTimeOffset.UtcNow
            };
        }

        private static RepairCommandResult CreateFailedCommandResult()
        {
            return new RepairCommandResult
            {
                Status = RepairCommandStatus.Failed,
                ExitCode = 1,
                Message = "Falha simulada.",
                StartedAt = DateTimeOffset.UtcNow,
                FinishedAt = DateTimeOffset.UtcNow
            };
        }

        private static RepairValidationResult CreateValidationResult(bool isSuccessful)
        {
            return new RepairValidationResult
            {
                IsSuccessful = isSuccessful,
                IntegrityStatus = isSuccessful ? IntegrityStatus.Healthy : IntegrityStatus.Corrupted,
                ValidatedAt = DateTimeOffset.UtcNow
            };
        }
    }
}
