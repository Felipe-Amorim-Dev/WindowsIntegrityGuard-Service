using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using WindowsIntegrityGuard.Core.Enums;
using WindowsIntegrityGuard.Core.Interfaces;
using WindowsIntegrityGuard.Core.Models;

namespace WindowsIntegrityGuard.Core.Services
{
    public sealed class FileHashService : IFileHashService
    {
        public async Task<string> CalculateHashAsync(string filePath, CancellationToken cancellationToken = default)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

            using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, 81920, FileOptions.Asynchronous | FileOptions.SequentialScan);

            byte[] hash = await SHA256.HashDataAsync(stream, cancellationToken);

            return Convert.ToHexString(hash);
        }

        public async Task<FileHashResult> VerifyHashAsync(string filePath, string expectedHash, CancellationToken cancellationToken = default)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(filePath);
            ArgumentException.ThrowIfNullOrWhiteSpace(expectedHash);

            if (expectedHash.Length != 64 || !expectedHash.All(Uri.IsHexDigit))
            {
                throw new ArgumentException("O hash esperado deve conter 64 caracteres hexadecimais.", nameof(expectedHash));
            }

            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                string currentHash = await CalculateHashAsync(filePath, cancellationToken);
                bool isMatch = string.Equals(currentHash, expectedHash, StringComparison.OrdinalIgnoreCase);

                return new FileHashResult
                {
                    FilePath = filePath,
                    ExpectedHash = expectedHash,
                    CurrentHash = currentHash,
                    Status = isMatch ? FileHashStatus.Match : FileHashStatus.Modified,
                    Message = isMatch ? "O arquivo corresponde ao hash de referência." : "O conteúdo do arquivo é diferente da referência.",
                    VerifiedAt = DateTimeOffset.UtcNow
                };
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                return new FileHashResult
                {
                    FilePath = filePath,
                    ExpectedHash = expectedHash,
                    Status = FileHashStatus.Failed,
                    Message = ex.Message,
                    VerifiedAt = DateTimeOffset.UtcNow
                };
            }
        }
    }
}
