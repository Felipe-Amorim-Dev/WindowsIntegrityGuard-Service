using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WindowsIntegrityGuard.Core.Enums;

namespace WindowsIntegrityGuard.Core.Models
{
    public sealed class RepairHistoryEntry
    {
        public Guid Id { get; init; } = Guid.NewGuid();
        public RepairStatus Status { get; init; }
        public string Message { get; init; } = string.Empty;
        public int? ExitCode { get; init; }
        public bool SfcExecuted { get; init; }
        public bool DismExecuted { get; init; }
        public bool ValidationSuccessful { get; init; }
        public bool RebootRequired { get; init; }
        public DateTimeOffset StartedAt { get; init; }
        public DateTimeOffset FinishedAt { get; init; }
    }
}
