Write-Host "Stopping AppHost process (if any)..."
Get-CimInstance Win32_Process |
    Where-Object { $_.Name -like "*AppHost*" -or $_.CommandLine -like "*HalcyonRecords.AppHost*" } |
    ForEach-Object {
        Write-Host "  Stopping PID $($_.ProcessId) ($($_.Name))"
        Stop-Process -Id $_.ProcessId -Force
    }

Write-Host "Stopping sql/meilisearch containers (if any)..."
$containers = docker ps --filter "name=sql-" --filter "name=meilisearch-" -q
if ($containers) {
    $containers | ForEach-Object { docker stop $_ }
} else {
    Write-Host "  None running."
}

Write-Host "Done."