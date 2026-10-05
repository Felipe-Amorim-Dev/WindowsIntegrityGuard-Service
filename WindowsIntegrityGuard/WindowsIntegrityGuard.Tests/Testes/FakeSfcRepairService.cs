using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WindowsIntegrityGuard.Core.Interfaces;
using WindowsIntegrityGuard.Core.Models;

namespace WindowsIntegrityGuard.Tests.Testes
{
    public sealed class FakeSfcRepairService : ISfcRepairService
    {
        private readonly Queue<RepairCommandResult> _results;

        public int ExecutionCount { get; private set; }

        public FakeSfcRepairService(params RepairCommandResult[] results)
        {
            _results = new Queue<RepairCommandResult>(results);
        }

        public Task<RepairCommandResult> RepairAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ExecutionCount++;

            if (_results.Count == 0)
            {
                throw new InvalidOperationException("Nenhum resultado configurado para o SFC fake.");
            }

            return Task.FromResult(_results.Dequeue());
        }
    }
}
