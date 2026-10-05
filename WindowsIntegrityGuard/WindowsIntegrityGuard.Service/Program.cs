using Serilog;
using Serilog.Events;
using WindowsIntegrityGuard.Core.Interfaces;
using WindowsIntegrityGuard.Service.Scanners;
using WindowsIntegrityGuard.Service.Services;
using WindowsIntegrityGuard.Core.Services;
using WindowsIntegrityGuard.Service;

var builder = Host.CreateApplicationBuilder(args);

string logDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "WindowsIntegrityGuard", "Logs");
string logFilePath = Path.Combine(logDirectory, "service-.log");

Directory.CreateDirectory(logDirectory);

builder.Services.AddWindowsService(options =>
{
    options.ServiceName = "Windows Integrity Guard";
});

builder.Services.AddSerilog((services, configuration) => configuration
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File(logFilePath, rollingInterval: RollingInterval.Day, retainedFileCountLimit: 14, shared: true));

builder.Services.AddSingleton<ServiceStateManager>();
builder.Services.AddSingleton<IIntegrityScanner, SfcIntegrityScanner>();
builder.Services.AddSingleton<IFileHashService, FileHashService>();
builder.Services.AddSingleton<IDigitalSignatureService, DigitalSignatureService>();
builder.Services.AddSingleton<IIntegrityResultClassifier, IntegrityResultClassifier>();
builder.Services.AddSingleton<ISfcRepairService, SfcRepairService>();
builder.Services.AddSingleton<IDismRepairService, DismRepairService>();
builder.Services.AddSingleton<IRepairValidationService, RepairValidationService>();
builder.Services.AddSingleton<IRepairHistoryService, RepairHistoryService>();
builder.Services.AddSingleton<IRepairEngine, RepairEngine>();

builder.Services.AddHostedService<Worker>();

builder.Services.AddHostedService<Worker>();

var host = builder.Build();

await host.RunAsync();