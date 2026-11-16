using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsIntegrityGuard.Core.Models.Ipc
{
    public sealed class IpcRequest
    {
        public string Command { get; init; } = string.Empty;
    }
}
