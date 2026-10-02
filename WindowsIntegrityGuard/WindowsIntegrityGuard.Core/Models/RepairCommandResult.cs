using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WindowsIntegrityGuard.Core.Enums;

namespace WindowsIntegrityGuard.Core.Models
{
    public sealed class RepairCommandResult
    {
        public RepairCommandStatus Status { get; init; }
        public int? ExitCode { get; init; }
        public string Output { get; init; } = string.Empty;
        public string ErrorOutput { get; init; } = string.Empty;
        public string Message { get; init; } = string.Empty;
        public DateTimeOffset StartedAt { get; init; }
        public DateTimeOffset FinishedAt { get; init; }

        public bool IsSuccessful => Status == RepairCommandStatus.Success;
    }
}
