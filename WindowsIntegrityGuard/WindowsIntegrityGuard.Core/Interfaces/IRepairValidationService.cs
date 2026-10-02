using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WindowsIntegrityGuard.Core.Models;

namespace WindowsIntegrityGuard.Core.Interfaces
{
    public interface IRepairValidationService
    {
        Task<RepairValidationResult> ValidateAsync(CancellationToken cancellationToken = default);
    }
}
