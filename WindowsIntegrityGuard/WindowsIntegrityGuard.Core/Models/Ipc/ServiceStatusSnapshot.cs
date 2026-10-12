using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WindowsIntegrityGuard.Core.Enums;

namespace WindowsIntegrityGuard.Core.Models.Ipc
{
    public sealed class ServiceStatusSnapshot
    {
        public ServiceStatus Status { get; init; }
        public DateTimeOffset Timestamp { get; init; }
    }
}
