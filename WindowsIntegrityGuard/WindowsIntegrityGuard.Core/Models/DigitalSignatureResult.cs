using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WindowsIntegrityGuard.Core.Enums;

namespace WindowsIntegrityGuard.Core.Models
{
    public sealed class DigitalSignatureResult
    {
        public string FilePath { get; init; } = string.Empty;
        public DigitalSignatureStatus Status { get; init; }
        public string Message { get; init; } = string.Empty;
        public uint? ErrorCode { get; init; }
        public DateTimeOffset VerifiedAt { get; init; }
    }
}
