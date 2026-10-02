using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WindowsIntegrityGuard.Core.Enums;

namespace WindowsIntegrityGuard.Core.Models
{
    public sealed class RepairResult
    {
        public RepairStatus Status { get; init; } = RepairStatus.NotStarted;
        public string Message { get; init; } = string.Empty;
        public int? ExitCode { get; init; }
        public DateTimeOffset StartedAt { get; init; }
        public DateTimeOffset FinishedAt { get; init; }
        public bool RebootRequired { get; init; }
    }
}
