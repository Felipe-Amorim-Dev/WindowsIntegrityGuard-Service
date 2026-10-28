using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WindowsIntegrityGuard.Core.Enums;

namespace WindowsIntegrityGuard.Core.Models
{
    public sealed class ComponentStoreHealthResult
    {
        public ComponentStoreHealthStatus Status { get; init; }
        public string Message { get; init; } = string.Empty;
        public int? HResult { get; init; }
        public DateTimeOffset CheckedAt { get; init; }
    }
}
