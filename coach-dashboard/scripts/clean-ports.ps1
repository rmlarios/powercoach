# clean-ports.ps1
# Script para limpiar procesos Node.js huérfanos que bloquean puertos

param(
    [int[]]$Ports = @(3000, 3001, 3002)
)

Write-Host "🧹 Limpiando puertos: $($Ports -join ', ')..." -ForegroundColor Cyan

foreach ($port in $Ports) {
    $connections = netstat -ano | findstr "LISTENING" | findstr ":$port "
    
    if ($connections) {
        foreach ($line in $connections -split "`n") {
            if ($line -match '\s+(\d+)\s*$') {
                $pid = $matches[1]
                if ($pid -and $pid -ne "0") {
                    try {
                        $process = Get-Process -Id $pid -ErrorAction SilentlyContinue
                        if ($process) {
                            Write-Host "  Terminando proceso $($process.ProcessName) (PID: $pid) en puerto $port" -ForegroundColor Yellow
                            Stop-Process -Id $pid -Force -ErrorAction SilentlyContinue
                        }
                    } catch {
                        # Proceso ya terminado
                    }
                }
            }
        }
    }
}

# Limpiar caché de Next.js si existe
$nextCache = Join-Path $PSScriptRoot "../.next"
if (Test-Path $nextCache) {
    Write-Host "🗑️  Limpiando caché de Next.js..." -ForegroundColor Cyan
    Remove-Item -Recurse -Force $nextCache -ErrorAction SilentlyContinue
}

Write-Host "✅ Limpieza completada" -ForegroundColor Green
