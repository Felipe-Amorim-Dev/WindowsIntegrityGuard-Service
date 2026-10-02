using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WindowsIntegrityGuard.Core.Enums;
using WindowsIntegrityGuard.Core.Interfaces;
using WindowsIntegrityGuard.Core.Models;

namespace WindowsIntegrityGuard.Service.Services
{
    public sealed class DismRepairService : IDismRepairService
    {
        private readonly TimeSpan _timeout;

        public DismRepairService(IConfiguration configuration)
        {
            int minutes = configuration.GetValue<int>("WindowsIntegrityGuard:RepairTimeoutMinutes", 60);
            _timeout = TimeSpan.FromMinutes(minutes > 0 ? minutes : 60);
        }

        public async Task<RepairCommandResult> RepairAsync(CancellationToken cancellationToken = default)
        {
            DateTimeOffset startedAt = DateTimeOffset.UtcNow;

            string windowsDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Windows);
            string executable = Path.Combine(windowsDirectory, "System32", "dism.exe");

            using var timeoutSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeoutSource.CancelAfter(_timeout);

            try
            {
                var processInfo = new ProcessStartInfo
                {
                    FileName = executable,
                    Arguments = "/Online /Cleanup-Image /RestoreHealth",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };

                using var process = new Process { StartInfo = processInfo };

                if (!process.Start())
                {
                    return CreateFailedResult("Não foi possível iniciar o DISM.", startedAt);
                }

                Task<string> outputTask = process.StandardOutput.ReadToEndAsync();
                Task<string> errorTask = process.StandardError.ReadToEndAsync();

                try
                {
                    await process.WaitForExitAsync(timeoutSource.Token);
                }
                catch (OperationCanceledException)
                {
                    if (!process.HasExited)
                    {
                        process.Kill(entireProcessTree: true);
                    }

                    await process.WaitForExitAsync();

                    if (cancellationToken.IsCancellationRequested)
                    {
                        throw;
                    }

                    return new RepairCommandResult
                    {
                        Status = RepairCommandStatus.TimedOut,
                        Message = "A execução do DISM excedeu o tempo limite.",
                        StartedAt = startedAt,
                        FinishedAt = DateTimeOffset.UtcNow
                    };
                }

                string output = await outputTask;
                string error = await errorTask;

                return new RepairCommandResult
                {
                    Status = process.ExitCode == 0 ? RepairCommandStatus.Success : RepairCommandStatus.Failed,
                    ExitCode = process.ExitCode,
                    Output = output,
                    ErrorOutput = error,
                    Message = process.ExitCode == 0 ? "DISM executado." : "O DISM retornou uma falha.",
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
                return CreateFailedResult(ex.Message, startedAt);
            }
        }

        private static RepairCommandResult CreateFailedResult(string message, DateTimeOffset startedAt)
        {
            return new RepairCommandResult
            {
                Status = RepairCommandStatus.Failed,
                Message = message,
                StartedAt = startedAt,
                FinishedAt = DateTimeOffset.UtcNow
            };
        }
    }
}
