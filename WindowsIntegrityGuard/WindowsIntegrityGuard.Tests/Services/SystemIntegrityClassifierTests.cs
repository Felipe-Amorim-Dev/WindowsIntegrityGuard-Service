using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WindowsIntegrityGuard.Core.Enums;
using WindowsIntegrityGuard.Core.Services;

namespace WindowsIntegrityGuard.Tests.Services
{
    public sealed class SystemIntegrityClassifierTests
    {
        private readonly SystemIntegrityClassifier _classifier = new();

        [Fact]
        public void Classify_WhenSfcHealthyAndDismHealthy_ReturnsHealthy()
        {
            IntegrityStatus result = _classifier.Classify(IntegrityStatus.Healthy, ComponentStoreHealthStatus.Healthy);

            Assert.Equal(IntegrityStatus.Healthy, result);
        }

        [Fact]
        public void Classify_WhenSfcHealthyAndDismRepairable_ReturnsCorrupted()
        {
            IntegrityStatus result = _classifier.Classify(IntegrityStatus.Healthy, ComponentStoreHealthStatus.Repairable);

            Assert.Equal(IntegrityStatus.Corrupted, result);
        }

        [Fact]
        public void Classify_WhenSfcHealthyAndDismNonRepairable_ReturnsCorrupted()
        {
            IntegrityStatus result = _classifier.Classify(IntegrityStatus.Healthy, ComponentStoreHealthStatus.NonRepairable);

            Assert.Equal(IntegrityStatus.Corrupted, result);
        }

        [Fact]
        public void Classify_WhenSfcHealthyAndDismUnknown_ReturnsInconclusive()
        {
            IntegrityStatus result = _classifier.Classify(IntegrityStatus.Healthy, ComponentStoreHealthStatus.Unknown);

            Assert.Equal(IntegrityStatus.Inconclusive, result);
        }

        [Fact]
        public void Classify_WhenSfcCorruptedAndDismHealthy_ReturnsCorrupted()
        {
            IntegrityStatus result = _classifier.Classify(IntegrityStatus.Corrupted, ComponentStoreHealthStatus.Healthy);

            Assert.Equal(IntegrityStatus.Corrupted, result);
        }

        [Fact]
        public void Classify_WhenSfcCorruptedAndDismRepairable_ReturnsCorrupted()
        {
            IntegrityStatus result = _classifier.Classify(IntegrityStatus.Corrupted, ComponentStoreHealthStatus.Repairable);

            Assert.Equal(IntegrityStatus.Corrupted, result);
        }

        [Fact]
        public void Classify_WhenSfcCorruptedAndDismUnknown_ReturnsCorrupted()
        {
            IntegrityStatus result = _classifier.Classify(IntegrityStatus.Corrupted, ComponentStoreHealthStatus.Unknown);

            Assert.Equal(IntegrityStatus.Corrupted, result);
        }

        [Fact]
        public void Classify_WhenSfcInconclusiveAndDismHealthy_ReturnsInconclusive()
        {
            IntegrityStatus result = _classifier.Classify(IntegrityStatus.Inconclusive, ComponentStoreHealthStatus.Healthy);

            Assert.Equal(IntegrityStatus.Inconclusive, result);
        }

        [Fact]
        public void Classify_WhenSfcInconclusiveAndDismRepairable_ReturnsCorrupted()
        {
            IntegrityStatus result = _classifier.Classify(IntegrityStatus.Inconclusive, ComponentStoreHealthStatus.Repairable);

            Assert.Equal(IntegrityStatus.Corrupted, result);
        }

        [Fact]
        public void Classify_WhenSfcInconclusiveAndDismUnknown_ReturnsInconclusive()
        {
            IntegrityStatus result = _classifier.Classify(IntegrityStatus.Inconclusive, ComponentStoreHealthStatus.Unknown);

            Assert.Equal(IntegrityStatus.Inconclusive, result);
        }

        [Fact]
        public void Classify_WhenSfcFailedAndDismHealthy_ReturnsFailed()
        {
            IntegrityStatus result = _classifier.Classify(IntegrityStatus.Failed, ComponentStoreHealthStatus.Healthy);

            Assert.Equal(IntegrityStatus.Failed, result);
        }
    }
}
