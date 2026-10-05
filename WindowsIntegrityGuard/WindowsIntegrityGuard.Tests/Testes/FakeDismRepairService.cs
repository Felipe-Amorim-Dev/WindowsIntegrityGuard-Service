using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WindowsIntegrityGuard.Core.Interfaces;
using WindowsIntegrityGuard.Core.Models;

namespace WindowsIntegrityGuard.Tests.Testes
{
    public sealed class FakeDismRepairService : IDismRepairService
    {
        private readonly RepairCommandResult _result;

        public int ExecutionCount { get; private set; }

        public FakeDismRepairService(RepairCommandResult result)
        {
            _result = result;
        }

        public Task<RepairCommandResult> RepairAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ExecutionCount++;

            return Task.FromResult(_result);
        }
    }
}
