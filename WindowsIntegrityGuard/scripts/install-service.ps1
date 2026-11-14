param(
    [string]$PublishPath = "$PSScriptRoot\..\publish"
)

$ServiceName = "WindowsIntegrityGuard"
$DisplayName = "Windows Integrity Guard"
$Description = "Monitora e verifica a integridade de arquivos e componentes críticos do Windows."

$CurrentPrincipal = New-Object Security.Principal.WindowsPrincipal([Security.Principal.WindowsIdentity]::GetCurrent())

if (-not $CurrentPrincipal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)) {
    Write-Error "Este script precisa ser executado como Administrador."
    exit 1
}

$PublishPath = [System.IO.Path]::GetFullPath($PublishPath)
$ExecutablePath = Join-Path $PublishPath "WindowsIntegrityGuard.Service.exe"

if (-not (Test-Path $ExecutablePath)) {
    Write-Error "Executável não encontrado em: $ExecutablePath"
    Write-Host "Publique o Service antes de executar a instalação."
    exit 1
}

$ExistingService = Get-Service -Name $ServiceName -ErrorAction SilentlyContinue

if ($null -ne $ExistingService) {
    Write-Error "O serviço '$ServiceName' já está instalado."
    exit 1
}

Write-Host "Instalando Windows Integrity Guard..."

New-Service -Name $ServiceName -BinaryPathName "`"$ExecutablePath`"" -DisplayName $DisplayName -Description $Description -StartupType Automatic

Write-Host "Iniciando serviço..."

Start-Service -Name $ServiceName

$Service = Get-Service -Name $ServiceName

Write-Host ""
Write-Host "Windows Integrity Guard instalado com sucesso."
Write-Host "Status: $($Service.Status)"
Write-Host "Executável: $ExecutablePath"