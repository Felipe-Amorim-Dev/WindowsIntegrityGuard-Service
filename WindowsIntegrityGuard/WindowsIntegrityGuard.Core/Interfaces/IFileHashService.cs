using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WindowsIntegrityGuard.Core.Models;

namespace WindowsIntegrityGuard.Core.Interfaces
{
    public interface IFileHashService
    {
        Task<string> CalculateHashAsync(string filePath, CancellationToken cancellationToken = default);
        Task<FileHashResult> VerifyHashAsync(string filePath, string expectedHash, CancellationToken cancellationToken = default);
    }
}
