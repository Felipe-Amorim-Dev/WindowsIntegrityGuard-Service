<h1 align="center">🛡️ Windows Integrity Guard — Service</h1>

<p align="center"><em>NET service for Windows integrity and controlled repair workflows.</em></p>

<p align="center">🇧🇷 <a href="README.md">Português</a> | 🇺🇸 English</p>

<p align="center"><img src="https://img.shields.io/badge/license-MIT-006bed?style=flat-square" alt="MIT" /> <img src="https://img.shields.io/github/last-commit/Felipe-Amorim-Dev/WindowsIntegrityGuard-Service?style=flat-square&color=006bed" alt="Last commit" /></p>

<p align="center"><a href="#sobre">About</a> • <a href="#tecnologias">Technologies</a> • <a href="#como-rodar">Running locally</a> • <a href="#licenca">License</a> • <a href="#autor">Author</a></p>

---

<a id="sobre"></a>
## 📖 About

Windows Integrity Guard service component built with .NET 9. Separates contracts and logic (Core), host execution (Service) and automated checks (Tests). Integrates Windows Service hosting and Serilog logging.

## ✨ Features

- Background service
- Integrity verification and controlled repair flow
- Automated Core tests

## 🛠️ Technologies

![C#](https://img.shields.io/badge/C%23-006bed?style=for-the-badge) ![.NET 9](https://img.shields.io/badge/.NET_9-006bed?style=for-the-badge) ![Windows Services](https://img.shields.io/badge/Windows_Services-006bed?style=for-the-badge) ![Serilog](https://img.shields.io/badge/Serilog-006bed?style=for-the-badge)

## 📦 Prerequisites

Windows and .NET 9 SDK (.NET desktop workload for WPF).

<a id="como-rodar"></a>
## 🚀 Running locally

```bash
git clone https://github.com/Felipe-Amorim-Dev/WindowsIntegrityGuard-Service.git
cd WindowsIntegrityGuard-Service
dotnet restore WindowsIntegrityGuard/WindowsIntegrityGuard.sln
dotnet build WindowsIntegrityGuard/WindowsIntegrityGuard.sln
dotnet test WindowsIntegrityGuard/WindowsIntegrityGuard.sln
```

## 🧪 Tests

```bash
dotnet test WindowsIntegrityGuard/WindowsIntegrityGuard.sln
```

## 🔗 Related repositories

- [WindowsIntegrityGuard](https://github.com/Felipe-Amorim-Dev/WindowsIntegrityGuard) — Repositório agregador
- [WindowsIntegrityGuard-UI](https://github.com/Felipe-Amorim-Dev/WindowsIntegrityGuard-UI) — Interface WPF

## 🏗️ Service architecture

| Layer | Responsibility |
| --- | --- |
| `WindowsIntegrityGuard.Core` | Contracts, integrity assessment, classification and repair orchestration |
| `WindowsIntegrityGuard.Service` | Windows Service host, integrity scanners, SFC/DISM adapters and IPC server |
| `WindowsIntegrityGuard.Tests` | Unit tests using isolated services and fakes |

The service host registers `Worker`, `NamedPipeServer`, `RepairEngine`, `SfcIntegrityScanner`, `DigitalSignatureService` and other adapters via dependency injection. Serilog writes daily rolling files under the Windows common application data directory.

### Integrity and repair workflow

1. A client communicates over the `WindowsIntegrityGuard` Named Pipe using JSON.
2. The request handler returns service information using the IPC response contract.
3. The integrity scanner and result classifier provide a structured assessment.
4. `RepairEngine` checks `IsRepairCandidate` and `AllowRepair` before invoking a repair.
5. Authorized repair follows SFC → validation → DISM if needed → second SFC → final validation.
6. Repair outcomes are stored through the repair history abstraction.

### Build and tests

```bash
dotnet restore WindowsIntegrityGuard/WindowsIntegrityGuard.sln
dotnet build WindowsIntegrityGuard/WindowsIntegrityGuard.sln
dotnet test WindowsIntegrityGuard/WindowsIntegrityGuard.sln
```

The desktop UI is maintained in [WindowsIntegrityGuard-UI](https://github.com/Felipe-Amorim-Dev/WindowsIntegrityGuard-UI).

<a id="licenca"></a>
## 📄 License

Distributed under the [MIT license](LICENSE).

<a id="autor"></a>
## 👤 Author

**Felipe Figueiredo Amorim** — Full Stack Developer.

[![LinkedIn](https://img.shields.io/badge/LinkedIn-Felipe%20Amorim-0A66C2?style=for-the-badge&logo=linkedin)](https://www.linkedin.com/in/felipe-amorim-dev/) [![E-mail](https://img.shields.io/badge/E--mail-contato-006bed?style=for-the-badge)](mailto:felipe.f.amorim@outlook.com)

<p align="center"><strong>Obrigado por visitar! / Thanks for visiting!</strong></p>
