using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsIntegrityGuard.Core.Interfaces
{
    public interface IScanCoordinator
    {
        bool TryStartScan();
    }
}
