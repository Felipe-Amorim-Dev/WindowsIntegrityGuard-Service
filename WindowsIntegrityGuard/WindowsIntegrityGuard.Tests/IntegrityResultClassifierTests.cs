using WindowsIntegrityGuard.Core.Enums;
using WindowsIntegrityGuard.Core.Models;
using WindowsIntegrityGuard.Core.Services;

namespace WindowsIntegrityGuard.Tests;

public class IntegrityResultClassifierTests
{
    [Fact]
    public void ModifiedHash_DoesNotOverrideHealthySfc()
    {
        var result = new IntegrityResultClassifier().Classify(new IntegrityScanResult { Status = IntegrityStatus.Healthy }, new FileHashResult { Status = FileHashStatus.Modified });
        Assert.Equal(IntegrityStatus.Healthy, result.Status);
        Assert.True(result.RequiresReview);
        Assert.False(result.IsRepairCandidate);
    }

    [Fact]
    public void UntrustedSignature_DoesNotCreateRepairCandidate()
    {
        var result = new IntegrityResultClassifier().Classify(new IntegrityScanResult { Status = IntegrityStatus.Inconclusive }, signatureResult: new DigitalSignatureResult { Status = DigitalSignatureStatus.Untrusted });
        Assert.True(result.RequiresReview);
        Assert.False(result.IsRepairCandidate);
    }

    [Fact]
    public void SfcCorruption_IsOnlyAReviewCandidate()
    {
        var result = new IntegrityResultClassifier().Classify(new IntegrityScanResult { Status = IntegrityStatus.Corrupted });
        Assert.True(result.IsRepairCandidate);
        Assert.Equal(IntegrityStatus.Corrupted, result.Status);
    }
}
