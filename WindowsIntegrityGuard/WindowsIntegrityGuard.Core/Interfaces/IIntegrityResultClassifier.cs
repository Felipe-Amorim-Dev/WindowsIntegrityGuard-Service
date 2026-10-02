using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WindowsIntegrityGuard.Core.Models;

namespace WindowsIntegrityGuard.Core.Interfaces
{
    public interface IIntegrityResultClassifier
    {
        IntegrityAssessment Classify(IntegrityScanResult sfcResult, FileHashResult? hashResult = null, DigitalSignatureResult? signatureResult = null);
    }
}
