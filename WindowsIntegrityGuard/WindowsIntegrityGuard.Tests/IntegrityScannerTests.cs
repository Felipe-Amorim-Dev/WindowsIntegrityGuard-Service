using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WindowsIntegrityGuard.Core.Enums;
using WindowsIntegrityGuard.Core.Services;

namespace WindowsIntegrityGuard.Tests
{
    public class IntegrityScannerTests
    {
        [Fact]
        public async Task ScanAsync_ShouldReturnInconclusive_WhenScannerIsNotImplemented()
        {
            var scanner = new IntegrityScanner();

            var result = await scanner.ScanAsync();

            Assert.Equal(IntegrityStatus.Inconclusive, result.Status);
            Assert.False(result.RequiresRepair);
            Assert.NotEmpty(result.Message);
        }

        [Fact]
        public async Task ScanAsync_ShouldReturnValidExecutionDates()
        {
            var scanner = new IntegrityScanner();

            var result = await scanner.ScanAsync();

            Assert.True(result.FinishedAt >= result.StartedAt);
        }

        [Fact]
        public async Task ScanAsync_ShouldRespectCancellation()
        {
            var scanner = new IntegrityScanner();
            using var cancellationTokenSource = new CancellationTokenSource();

            cancellationTokenSource.Cancel();

            await Assert.ThrowsAnyAsync<OperationCanceledException>(() => scanner.ScanAsync(cancellationTokenSource.Token));
        }

        [Fact]
        public void CorruptedResult_ShouldRequireRepair()
        {
            var result = new WindowsIntegrityGuard.Core.Models.IntegrityScanResult
            {
                Status = IntegrityStatus.Corrupted
            };

            Assert.True(result.RequiresRepair);
        }
    }
}
