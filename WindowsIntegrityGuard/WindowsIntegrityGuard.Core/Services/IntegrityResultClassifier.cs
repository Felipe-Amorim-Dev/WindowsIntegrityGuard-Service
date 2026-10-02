using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WindowsIntegrityGuard.Core.Enums;
using WindowsIntegrityGuard.Core.Interfaces;
using WindowsIntegrityGuard.Core.Models;

namespace WindowsIntegrityGuard.Core.Services
{
    public sealed class IntegrityResultClassifier : IIntegrityResultClassifier
    {
        public IntegrityAssessment Classify(IntegrityScanResult sfcResult, FileHashResult? hashResult = null, DigitalSignatureResult? signatureResult = null)
        {
            ArgumentNullException.ThrowIfNull(sfcResult);

            IntegrityStatus status = sfcResult.Status switch
            {
                IntegrityStatus.Healthy => IntegrityStatus.Healthy,
                IntegrityStatus.Corrupted => IntegrityStatus.Corrupted,
                IntegrityStatus.Failed => IntegrityStatus.Failed,
                _ => IntegrityStatus.Inconclusive
            };

            bool hashModified = hashResult?.Status == FileHashStatus.Modified;
            bool hashFailed = hashResult?.Status == FileHashStatus.Failed;

            bool signatureWarning = signatureResult?.Status is DigitalSignatureStatus.Untrusted
                or DigitalSignatureStatus.Unsigned
                or DigitalSignatureStatus.Inconclusive
                or DigitalSignatureStatus.Failed;

            string message = status switch
            {
                IntegrityStatus.Healthy => "O SFC não identificou violações nos arquivos protegidos do Windows.",
                IntegrityStatus.Corrupted => "O SFC identificou violações de integridade. É necessária avaliação para reparação.",
                IntegrityStatus.Failed => "A verificação do SFC apresentou uma falha operacional.",
                _ => "Não foi possível determinar a integridade dos arquivos protegidos."
            };

            if (hashModified)
            {
                message += " Foi identificada uma diferença no hash do arquivo monitorado.";
            }

            if (hashFailed)
            {
                message += " Não foi possível concluir a verificação SHA-256.";
            }

            if (signatureWarning)
            {
                message += " A validação da assinatura digital requer análise adicional.";
            }

            return new IntegrityAssessment
            {
                Status = status,
                HashStatus = hashResult?.Status,
                SignatureStatus = signatureResult?.Status,
                Message = message,
                AssessedAt = DateTimeOffset.UtcNow
            };
        }
    }
}
