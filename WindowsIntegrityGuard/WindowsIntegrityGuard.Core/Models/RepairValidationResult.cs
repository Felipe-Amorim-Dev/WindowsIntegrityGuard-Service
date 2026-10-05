using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WindowsIntegrityGuard.Core.Enums;

namespace WindowsIntegrityGuard.Core.Models
{
    public sealed class RepairValidationResult
    {
        public bool IsSuccessful { get; init; }
        public IntegrityStatus IntegrityStatus { get; init; }
        public string Message { get; init; } = string.Empty;
        public DateTimeOffset ValidatedAt { get; init; }
    }
}
