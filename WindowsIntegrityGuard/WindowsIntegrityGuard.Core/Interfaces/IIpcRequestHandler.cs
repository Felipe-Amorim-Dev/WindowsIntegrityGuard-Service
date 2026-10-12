using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WindowsIntegrityGuard.Core.Interfaces.Ipc;
using WindowsIntegrityGuard.Core.Models.Ipc;

namespace WindowsIntegrityGuard.Core.Interfaces
{
    public interface IIpcRequestHandler
    {
        IpcResponse Handle(IpcRequest request);
    }
}
