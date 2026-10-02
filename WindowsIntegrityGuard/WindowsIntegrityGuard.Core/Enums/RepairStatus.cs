using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsIntegrityGuard.Core.Enums
{
    public enum RepairStatus
    {
        NotStarted,
        NotRequired,
        Repairing,
        Completed,
        Failed,
        Cancelled
    }
}
