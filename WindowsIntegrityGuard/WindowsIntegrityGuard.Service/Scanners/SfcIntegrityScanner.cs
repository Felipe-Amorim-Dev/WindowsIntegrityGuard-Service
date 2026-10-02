using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WindowsIntegrityGuard.Core.Enums;
using WindowsIntegrityGuard.Core.Interfaces;
using WindowsIntegrityGuard.Core.Models;

namespace WindowsIntegrityGuard.Service.Scanners
{
    public sealed class SfcIntegrityScanner : IIntegrityScanner
    {
        private readonly TimeSpan _timeout;

        public SfcIntegrityScanner(IConfiguration configuration)
        {
            int minutes = configuration.GetValue<int>("WindowsIntegrityGuard:ScanTimeoutMinutes", 60);
            _timeout = TimeSpan.FromMinutes(minutes > 0 ? minutes : 60);
        }

        public async Task<IntegrityScanResult> ScanAsync(CancellationToken cancellationToken = default)
        {
            DateTimeOffset startedAt = DateTimeOffset.UtcNow;
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                string windowsDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Windows);
                string executable = Path.Combine(windowsDirectory, "System32", "sfc.exe");

                var processInfo = new ProcessStartInfo
                {
                    FileName = executable,
                    Arguments = "/verifyonly",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    StandardOutputEncoding = Encoding.GetEncoding(CultureInfo.CurrentCulture.TextInfo.OEMCodePage),
                    StandardErrorEncoding = Encoding.GetEncoding(CultureInfo.CurrentCulture.TextInfo.OEMCodePage)
                };

                using var process = new Process { StartInfo = processInfo };
                using var timeoutSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

                timeoutSource.CancelAfter(_timeout);

                if (!process.Start())
                {
                    throw new InvalidOperationException("Não foi possível iniciar o SFC.");
                }

                Task<string> outputTask = process.StandardOutput.ReadToEndAsync();
                Task<string> errorTask = process.StandardError.ReadToEndAsync();

                try
                {
                    await process.WaitForExitAsync(timeoutSource.Token);
                }
                catch (OperationCanceledException)
                {
                    try
                    {
                        if (!process.HasExited)
                        {
                            process.Kill(entireProcessTree: true);
                        }
                    }
                    catch (InvalidOperationException)
                    {
                        // O processo pode ter terminado entre a verificação e o Kill.
                    }

                    await process.WaitForExitAsync();
                    await Task.WhenAll(outputTask, errorTask);

                    if (cancellationToken.IsCancellationRequested)
                    {
                        throw new OperationCanceledException(cancellationToken);
                    }

                    return CreateResult(IntegrityStatus.Failed, "A verificação excedeu o tempo limite.", startedAt);
                }

                string output = await outputTask;
                string error = await errorTask;
                string fullOutput = string.IsNullOrWhiteSpace(error) ? output : $"{output}{Environment.NewLine}{error}";
                IntegrityStatus status = SfcResultParser.Classify(fullOutput, process.ExitCode);

                return new IntegrityScanResult
                {
                    Status = status,
                    Message = GetMessage(status),
                    Output = fullOutput,
                    ExitCode = process.ExitCode,
                    StartedAt = startedAt,
                    FinishedAt = DateTimeOffset.UtcNow
                };
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex)
            {
                return CreateResult(IntegrityStatus.Failed, ex.Message, startedAt);
            }
        }

        private static string GetMessage(IntegrityStatus status)
        {
            return status switch
            {
                IntegrityStatus.Healthy => "Nenhuma violação de integridade identificada.",
                IntegrityStatus.Corrupted => "O SFC identificou violações de integridade.",
                IntegrityStatus.Inconclusive => "Não foi possível determinar a integridade do sistema com segurança.",
                _ => "Falha durante a verificação de integridade."
            };
        }

        private static IntegrityScanResult CreateResult(IntegrityStatus status, string message, DateTimeOffset startedAt)
        {
            return new IntegrityScanResult
            {
                Status = status,
                Message = message,
                StartedAt = startedAt,
                FinishedAt = DateTimeOffset.UtcNow
            };
        }
    }
}
