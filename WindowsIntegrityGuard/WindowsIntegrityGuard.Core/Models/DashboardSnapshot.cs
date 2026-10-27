using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WindowsIntegrityGuard.Core.Enums;

namespace WindowsIntegrityGuard.Core.Models
{
    public sealed class DashboardSnapshot
    {
        public ServiceStatus ServiceStatus { get; init; }
        public IntegrityStatus IntegrityStatus { get; init; } = IntegrityStatus.NotStarted;
        public string CurrentOperation { get; init; } = "Em espera";
        public DateTimeOffset? LastScanAt { get; init; }
        public double? ScanProgress { get; init; }
        public int? FilesVerified { get; init; }
        public int? ProblemsFound { get; init; }
        public int? ProblemsRepaired { get; init; }
        public TimeSpan? LastScanDuration { get; init; }
    }
}
