# Windows Integrity Guard — Service

**Status: prototype / Em desenvolvimento.** .NET service and core functionality for controlled Windows integrity verification and repair. Part of the [main project](https://github.com/Felipe-Amorim-Dev/WindowsIntegrityGuard).

## Structure
- `WindowsIntegrityGuard/WindowsIntegrityGuard.Core` — core contracts and repair logic
- `WindowsIntegrityGuard/WindowsIntegrityGuard.Service` — service host
- `WindowsIntegrityGuard/WindowsIntegrityGuard.Tests` — existing source tests and fakes/mocks
- `WindowsIntegrityGuard/WindowsIntegrityGuard.sln` — solution

## Development and verification
```bash
dotnet restore WindowsIntegrityGuard/WindowsIntegrityGuard.sln
dotnet build WindowsIntegrityGuard/WindowsIntegrityGuard.sln
dotnet test WindowsIntegrityGuard/WindowsIntegrityGuard.sln
```

Windows-specific API usage may require a Windows environment. Tests should isolate system-repair commands; never execute destructive or privileged repair operations in a CI job.

**Safety:** not production ready. Validate permissions, command invocation, user consent, repair outcomes and recovery paths in a disposable Windows VM before deployment.

**PT-BR:** Serviço experimental de análise e reparo. Testes existentes não substituem validação operacional no Windows real.
