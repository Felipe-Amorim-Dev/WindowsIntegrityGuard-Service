using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WindowsIntegrityGuard.Core.Enums;

namespace WindowsIntegrityGuard.Core.Models
{
    public sealed class ScanHistoryEntry
    {
        public DateTimeOffset StartedAt { get; init; }
        public DateTimeOffset FinishedAt { get; init; }
        public TimeSpan Duration { get; init; }
        public IntegrityStatus Status { get; init; }
        public string Message { get; init; } = string.Empty;
        public int? ExitCode { get; init; }
        public bool RequiresRepairReview { get; init; }
    }
}
