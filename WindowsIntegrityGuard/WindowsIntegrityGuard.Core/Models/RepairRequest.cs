using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsIntegrityGuard.Core.Models
{
    public sealed class RepairRequest
    {
        public IntegrityAssessment Assessment { get; init; } = new();
        public bool AllowRepair { get; init; }
    }
}
