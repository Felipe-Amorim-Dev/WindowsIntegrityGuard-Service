using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using WindowsIntegrityGuard.Core.Interfaces;
using WindowsIntegrityGuard.Core.Models;

namespace WindowsIntegrityGuard.Service.Services
{
    public sealed class ServiceSettingsService : IServiceSettingsService
    {
        private readonly ILogger<ServiceSettingsService> _logger;
        private readonly string _settingsPath;
        private readonly SemaphoreSlim _semaphore = new(1, 1);
        private readonly JsonSerializerOptions _jsonOptions = new() { WriteIndented = true };

        public ServiceSettingsService(ILogger<ServiceSettingsService> logger)
        {
            _logger = logger;

            string directory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "WindowsIntegrityGuard", "Data");
            Directory.CreateDirectory(directory);
            _settingsPath = Path.Combine(directory, "settings.json");
        }

        public async Task<ServiceSettings> LoadAsync(CancellationToken cancellationToken = default)
        {
            await _semaphore.WaitAsync(cancellationToken);

            try
            {
                if (!File.Exists(_settingsPath))
                {
                    return new ServiceSettings();
                }

                string json = await File.ReadAllTextAsync(_settingsPath, cancellationToken);
                return JsonSerializer.Deserialize<ServiceSettings>(json) ?? new ServiceSettings();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Falha ao carregar as configurações do Windows Integrity Guard.");
                return new ServiceSettings();
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task SaveAsync(ServiceSettings settings, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(settings);
            await _semaphore.WaitAsync(cancellationToken);

            try
            {
                string json = JsonSerializer.Serialize(settings, _jsonOptions);
                await File.WriteAllTextAsync(_settingsPath, json, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Falha ao salvar as configurações do Windows Integrity Guard.");
                throw;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task SetScanOnStartupAsync(bool enabled, CancellationToken cancellationToken = default)
        {
            ServiceSettings settings = await LoadAsync(cancellationToken);
            settings.ScanOnStartup = enabled;
            await SaveAsync(settings, cancellationToken);
        }
    }
}
