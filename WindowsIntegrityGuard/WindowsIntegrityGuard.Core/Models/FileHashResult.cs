using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WindowsIntegrityGuard.Core.Enums;

namespace WindowsIntegrityGuard.Core.Models
{
    public sealed class FileHashResult
    {
        public string FilePath { get; init; } = string.Empty;
        public string ExpectedHash { get; init; } = string.Empty;
        public string CurrentHash { get; init; } = string.Empty;
        public FileHashStatus Status { get; init; }
        public string Message { get; init; } = string.Empty;
        public DateTimeOffset VerifiedAt { get; init; }
    }
}
