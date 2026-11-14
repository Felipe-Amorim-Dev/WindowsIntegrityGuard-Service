$ServiceName = "WindowsIntegrityGuard"

$CurrentPrincipal = New-Object Security.Principal.WindowsPrincipal([Security.Principal.WindowsIdentity]::GetCurrent())

if (-not $CurrentPrincipal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)) {
    Write-Error "Este script precisa ser executado como Administrador."
    exit 1
}

$Service = Get-Service -Name $ServiceName -ErrorAction SilentlyContinue

if ($null -eq $Service) {
    Write-Host "O serviço '$ServiceName' não está instalado."
    exit 0
}

if ($Service.Status -ne "Stopped") {
    Write-Host "Parando Windows Integrity Guard..."
    Stop-Service -Name $ServiceName -Force
}

Write-Host "Removendo Windows Integrity Guard..."

sc.exe delete $ServiceName | Out-Null

Write-Host "Windows Integrity Guard removido com sucesso."