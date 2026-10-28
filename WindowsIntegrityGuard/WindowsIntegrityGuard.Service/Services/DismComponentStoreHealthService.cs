using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WindowsIntegrityGuard.Core.Enums;
using WindowsIntegrityGuard.Core.Interfaces;
using WindowsIntegrityGuard.Core.Models;
using WindowsIntegrityGuard.Service.Native;

namespace WindowsIntegrityGuard.Service.Services
{
    public sealed class DismComponentStoreHealthService : IComponentStoreHealthService
    {
        private readonly ILogger<DismComponentStoreHealthService> _logger;
        private readonly SemaphoreSlim _semaphore = new(1, 1);

        public DismComponentStoreHealthService(ILogger<DismComponentStoreHealthService> logger)
        {
            _logger = logger;
        }

        public async Task<ComponentStoreHealthResult> CheckHealthAsync(CancellationToken cancellationToken = default)
        {
            await _semaphore.WaitAsync(cancellationToken);

            try
            {
                return await Task.Run(() => CheckHealth(cancellationToken), cancellationToken);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        #region Metodos Privados
        private ComponentStoreHealthResult CheckHealth(CancellationToken cancellationToken)
        {
            uint session = DismNativeMethods.DefaultSession;
            bool initialized = false;
            bool sessionOpened = false;

            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                int hr = DismNativeMethods.DismInitialize(DismLogLevel.ErrorsWarningsInfo, null, null);

                if (hr < 0)
                {
                    return CreateUnknown("Não foi possível inicializar a DISM API.", hr);
                }

                initialized = true;

                hr = DismNativeMethods.DismOpenSession(DismNativeMethods.OnlineImage, null, null, out session);

                if (hr < 0)
                {
                    return CreateUnknown("Não foi possível abrir a sessão DISM do Windows online.", hr);
                }

                sessionOpened = true;
                cancellationToken.ThrowIfCancellationRequested();

                hr = DismNativeMethods.DismCheckImageHealth(session, true, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, out DismImageHealthState healthState);

                if (hr < 0)
                {
                    return CreateUnknown("Não foi possível determinar a saúde do Component Store.", hr);
                }

                return new ComponentStoreHealthResult
                {
                    Status = MapStatus(healthState),
                    Message = GetMessage(healthState),
                    HResult = hr,
                    CheckedAt = DateTimeOffset.UtcNow
                };
            }
            catch (DllNotFoundException ex)
            {
                _logger.LogWarning(ex, "DismApi.dll não está disponível.");
                return CreateUnknown("A DISM API não está disponível neste sistema.");
            }
            catch (EntryPointNotFoundException ex)
            {
                _logger.LogWarning(ex, "A função necessária da DISM API não está disponível.");
                return CreateUnknown("A versão disponível da DISM API não oferece a função necessária.");
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Falha inesperada ao consultar a DISM API.");
                return CreateUnknown("Falha inesperada ao consultar a saúde do Component Store.");
            }
            finally
            {
                if (sessionOpened)
                {
                    int closeHr = DismNativeMethods.DismCloseSession(session);

                    if (closeHr < 0)
                    {
                        _logger.LogWarning("DismCloseSession retornou HRESULT 0x{HResult:X8}.", closeHr);
                    }
                }

                if (initialized)
                {
                    int shutdownHr = DismNativeMethods.DismShutdown();

                    if (shutdownHr < 0)
                    {
                        _logger.LogWarning("DismShutdown retornou HRESULT 0x{HResult:X8}.", shutdownHr);
                    }
                }
            }
        }

        private static ComponentStoreHealthStatus MapStatus(DismImageHealthState healthState)
        {
            return healthState switch
            {
                DismImageHealthState.Healthy => ComponentStoreHealthStatus.Healthy,
                DismImageHealthState.Repairable => ComponentStoreHealthStatus.Repairable,
                DismImageHealthState.NonRepairable => ComponentStoreHealthStatus.NonRepairable,
                _ => ComponentStoreHealthStatus.Unknown
            };
        }

        private static string GetMessage(DismImageHealthState healthState)
        {
            return healthState switch
            {
                DismImageHealthState.Healthy => "O Component Store do Windows está íntegro.",
                DismImageHealthState.Repairable => "O Component Store possui corrupção reparável.",
                DismImageHealthState.NonRepairable => "O Component Store possui corrupção não reparável.",
                _ => "Não foi possível determinar a saúde do Component Store."
            };
        }

        private static ComponentStoreHealthResult CreateUnknown(string message, int? hResult = null)
        {
            return new ComponentStoreHealthResult
            {
                Status = ComponentStoreHealthStatus.Unknown,
                Message = message,
                HResult = hResult,
                CheckedAt = DateTimeOffset.UtcNow
            };
        }
        #endregion
    }
}
