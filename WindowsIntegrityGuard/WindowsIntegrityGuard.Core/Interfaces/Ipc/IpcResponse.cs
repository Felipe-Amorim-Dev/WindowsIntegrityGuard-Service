using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsIntegrityGuard.Core.Interfaces.Ipc
{
    public sealed class IpcResponse
    {
        public bool Success { get; init; }
        public string Message { get; init; } = string.Empty;
        public object? Data { get; init; }
        public DateTimeOffset Timestamp { get; init; } = DateTimeOffset.UtcNow;
    }
}
