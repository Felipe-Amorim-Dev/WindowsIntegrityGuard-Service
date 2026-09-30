using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WindowsIntegrityGuard.Core.Enums;

namespace WindowsIntegrityGuard.Core.Models
{
    public sealed class IntegrityScanResult
    {
        public IntegrityStatus Status { get; init; } = IntegrityStatus.NotStarted;
        public string Message { get; init; } = string.Empty;
        public DateTimeOffset StartedAt { get; init; }
        public DateTimeOffset FinishedAt { get; init; }
        public bool RequiresRepair => Status == IntegrityStatus.Corrupted;
    }
}
