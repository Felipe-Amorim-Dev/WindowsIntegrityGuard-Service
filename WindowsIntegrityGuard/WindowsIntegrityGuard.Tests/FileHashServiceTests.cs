using System.Security.Cryptography;
using System.Text;
using WindowsIntegrityGuard.Core.Enums;
using WindowsIntegrityGuard.Core.Services;

namespace WindowsIntegrityGuard.Tests;

public class FileHashServiceTests
{
    [Fact]
    public async Task Compare_ShouldDetectMatchAndModification()
    {
        string path = Path.GetTempFileName();
        try
        {
            await File.WriteAllTextAsync(path, "baseline");
            var service = new FileHashService();
            string expected = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes("baseline")));
            Assert.Equal(FileHashStatus.Match, (await service.VerifyHashAsync(path, expected)).Status);
            await File.WriteAllTextAsync(path, "changed");
            Assert.Equal(FileHashStatus.Modified, (await service.VerifyHashAsync(path, expected)).Status);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public async Task InvalidReference_ShouldBeRejected()
    {
        await Assert.ThrowsAsync<ArgumentException>(() => new FileHashService().VerifyHashAsync("example.txt", "invalid"));
    }
}
