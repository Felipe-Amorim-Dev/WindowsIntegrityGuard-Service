using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WindowsIntegrityGuard.Core.Enums;

namespace WindowsIntegrityGuard.Service.Scanners
{
    public static class SfcCbsResultParser
    {
        private static readonly string[] CorruptionMarkers =
        [
            "Cannot repair member file",
            "Repairing corrupted file",
            "Repaired file"
        ];

        public static IntegrityStatus Classify(string cbsContent)
        {
            if (string.IsNullOrWhiteSpace(cbsContent))
            {
                return IntegrityStatus.Inconclusive;
            }

            string[] sfcLines = cbsContent.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries).Where(line => line.Contains("[SR]", StringComparison.OrdinalIgnoreCase)).ToArray();

            if (sfcLines.Length == 0)
            {
                return IntegrityStatus.Inconclusive;
            }

            bool hasCorruption = sfcLines.Any(line => CorruptionMarkers.Any(marker => line.Contains(marker, StringComparison.OrdinalIgnoreCase)));

            if (hasCorruption)
            {
                return IntegrityStatus.Corrupted;
            }

            bool verificationCompleted = sfcLines.Any(line => line.Contains("Verify complete", StringComparison.OrdinalIgnoreCase));

            if (verificationCompleted)
            {
                return IntegrityStatus.Healthy;
            }

            return IntegrityStatus.Inconclusive;
        }
    }
}

