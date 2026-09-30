using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WindowsIntegrityGuard.Core.Models;

namespace WindowsIntegrityGuard.Core.Interfaces
{
    public interface IDigitalSignatureService
    {
        Task<DigitalSignatureResult> VerifyAsync(string filePath, CancellationToken cancellationToken = default);
    }
}
