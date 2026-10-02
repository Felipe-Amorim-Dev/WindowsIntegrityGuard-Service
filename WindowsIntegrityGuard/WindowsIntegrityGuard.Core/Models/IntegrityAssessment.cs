using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WindowsIntegrityGuard.Core.Enums;

namespace WindowsIntegrityGuard.Core.Models
{
    public sealed class IntegrityAssessment
    {
        public IntegrityStatus Status { get; init; } = IntegrityStatus.Inconclusive;
        public FileHashStatus? HashStatus { get; init; }
        public DigitalSignatureStatus? SignatureStatus { get; init; }
        public string Message { get; init; } = string.Empty;
        public DateTimeOffset AssessedAt { get; init; }
        public bool HasFileModification => HashStatus == FileHashStatus.Modified;
        public bool HasSignatureWarning => SignatureStatus is DigitalSignatureStatus.Untrusted or DigitalSignatureStatus.Unsigned or DigitalSignatureStatus.Inconclusive or DigitalSignatureStatus.Failed;
        public bool RequiresReview => Status != IntegrityStatus.Healthy || HasFileModification || HashStatus == FileHashStatus.Failed || HasSignatureWarning;
        public bool IsRepairCandidate => Status == IntegrityStatus.Corrupted;
    }
}
