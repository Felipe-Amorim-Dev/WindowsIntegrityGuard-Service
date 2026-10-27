<h1 align="center">🛡️ Windows Integrity Guard — Service</h1>

<p align="center"><em>Serviço .NET para rotinas de integridade e reparo controlado do Windows.</em></p>

<p align="center">🇧🇷 Português | 🇺🇸 <a href="README.en.md">English</a></p>

<p align="center"><img src="https://img.shields.io/badge/licen%C3%A7a-MIT-006bed?style=flat-square" alt="MIT" /> <img src="https://img.shields.io/github/last-commit/Felipe-Amorim-Dev/WindowsIntegrityGuard-Service?style=flat-square&color=006bed" alt="Last commit" /></p>

<p align="center"><a href="#sobre">Sobre</a> • <a href="#tecnologias">Tecnologias</a> • <a href="#como-rodar">Como rodar</a> • <a href="#licenca">Licença</a> • <a href="#autor">Autor</a></p>

---

<a id="sobre"></a>
## 📖 Sobre

Componente de serviço do Windows Integrity Guard em .NET 9. Divide contratos e lógica no Core, execução no Service e verificações automatizadas no projeto Tests. Integra hospedagem como Windows Service e logs Serilog.

## ✨ Funcionalidades

- Serviço em segundo plano
- Verificação de integridade e fluxo de reparo controlado
- Testes automatizados do Core

## 🛠️ Tecnologias

![C#](https://img.shields.io/badge/C%23-006bed?style=for-the-badge) ![.NET 9](https://img.shields.io/badge/.NET_9-006bed?style=for-the-badge) ![Windows Services](https://img.shields.io/badge/Windows_Services-006bed?style=for-the-badge) ![Serilog](https://img.shields.io/badge/Serilog-006bed?style=for-the-badge)

## 📦 Pré-requisitos

Windows e SDK .NET 9 (componentes de desktop .NET para WPF).

<a id="como-rodar"></a>
## 🚀 Como rodar

```bash
git clone https://github.com/Felipe-Amorim-Dev/WindowsIntegrityGuard-Service.git
cd WindowsIntegrityGuard-Service
dotnet restore WindowsIntegrityGuard/WindowsIntegrityGuard.sln
dotnet build WindowsIntegrityGuard/WindowsIntegrityGuard.sln
dotnet test WindowsIntegrityGuard/WindowsIntegrityGuard.sln
```

## 🧪 Testes

```bash
dotnet test WindowsIntegrityGuard/WindowsIntegrityGuard.sln
```

## 🔗 Repositórios relacionados

- [WindowsIntegrityGuard](https://github.com/Felipe-Amorim-Dev/WindowsIntegrityGuard) — Repositório agregador
- [WindowsIntegrityGuard-UI](https://github.com/Felipe-Amorim-Dev/WindowsIntegrityGuard-UI) — Interface WPF

## 🏗️ Arquitetura e componentes

- WindowsIntegrityGuard.Core define interfaces de varredura, classificação, serviços SFC/DISM e RepairEngine.
- WindowsIntegrityGuard.Service hospeda Worker, NamedPipeServer, scanners e implementações Windows.
- WindowsIntegrityGuard.Tests contém testes de componentes e simulações (fakes).
- Serilog grava logs em ProgramData/WindowsIntegrityGuard/Logs, com rotação diária.

### Fluxo técnico

1. O IPC utiliza Named Pipe WindowsIntegrityGuard e mensagens JSON.
2. RepairEngine exige avaliação de corrupção e autorização de reparo.
3. Sequência: SFC → validação → DISM quando necessário → novo SFC → validação final.
4. Histórico e resultados são registrados pelas abstrações de serviço.

<a id="licenca"></a>
## 📄 Licença

Distribuído sob a [licença MIT](LICENSE).

<a id="autor"></a>
## 👤 Autor

**Felipe Figueiredo Amorim** — Desenvolvedor Full Stack.

[![LinkedIn](https://img.shields.io/badge/LinkedIn-Felipe%20Amorim-0A66C2?style=for-the-badge&logo=linkedin)](https://www.linkedin.com/in/felipe-amorim-dev/) [![E-mail](https://img.shields.io/badge/E--mail-contato-006bed?style=for-the-badge)](mailto:felipe.f.amorim@outlook.com)

<p align="center"><strong>Obrigado por visitar! / Thanks for visiting!</strong></p>
