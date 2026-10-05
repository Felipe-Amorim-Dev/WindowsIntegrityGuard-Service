using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WindowsIntegrityGuard.Core.Interfaces;
using WindowsIntegrityGuard.Core.Models;

namespace WindowsIntegrityGuard.Tests.Testes
{
    public sealed class FakeRepairValidationService : IRepairValidationService
    {
        private readonly Queue<RepairValidationResult> _results;

        public int ExecutionCount { get; private set; }

        public FakeRepairValidationService(params RepairValidationResult[] results)
        {
            _results = new Queue<RepairValidationResult>(results);
        }

        public Task<RepairValidationResult> ValidateAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ExecutionCount++;

            if (_results.Count == 0)
            {
                throw new InvalidOperationException("Nenhum resultado configurado para a validação fake.");
            }

            return Task.FromResult(_results.Dequeue());
        }
    }
}
