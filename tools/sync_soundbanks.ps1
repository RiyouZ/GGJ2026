<#
sync_soundbanks.ps1
用途：将 Wwise 输出的 GeneratedSoundBanks 同步到 Unity 的 StreamingAssets 目录（保留目录结构）。
默认源：C:\Project\ggj2026\ggj2026\ggj2026_WwiseProject\GeneratedSoundBanks\Windows
默认目标：C:\Project\ggj2026\ggj2026\Assets\StreamingAssets\Audio\GeneratedSoundBanks\Windows
用法：在项目根目录或任意位置运行：
    powershell -ExecutionPolicy Bypass -File .\tools\sync_soundbanks.ps1
或指定源/目标：
    powershell -ExecutionPolicy Bypass -File .\tools\sync_soundbanks.ps1 -Source "C:\path\to\GeneratedSoundBanks\Windows" -Destination "C:\path\to\Unity\Assets\StreamingAssets\Audio\GeneratedSoundBanks\Windows"
#>
param(
    [string]$Source = "C:\Project\ggj2026\ggj2026\ggj2026_WwiseProject\GeneratedSoundBanks\Windows",
    [string]$Destination = "C:\Project\ggj2026\ggj2026\Assets\StreamingAssets\Audio\GeneratedSoundBanks\Windows",
    [switch]$WhatIf
)

function Write-Info($msg){ Write-Host "[info] $msg" -ForegroundColor Cyan }
function Write-ErrorAndExit($msg){ Write-Host "[error] $msg" -ForegroundColor Red; exit 1 }

Write-Info "Source: $Source"
Write-Info "Destination: $Destination"

if($WhatIf){ Write-Info "Running in WhatIf mode - no files will be copied." }

if(-not (Test-Path $Source)){
    Write-ErrorAndExit "Source path not found: $Source" 
}

# Ensure destination exists
if(-not (Test-Path $Destination)){
    if($WhatIf){ Write-Info "Would create destination: $Destination" } else { New-Item -ItemType Directory -Force -Path $Destination | Out-Null; Write-Info "Created destination: $Destination" }
}

# Prefer robocopy for robust mirroring
if(-not $WhatIf -and (Get-Command robocopy -ErrorAction SilentlyContinue)){
    Write-Info "Using robocopy to mirror files..."
    $args = @($Source, $Destination, '/MIR')
    $proc = Start-Process -FilePath robocopy -ArgumentList $args -NoNewWindow -Wait -PassThru
n    if($proc.ExitCode -ge 8){ Write-ErrorAndExit "robocopy failed with exit code $($proc.ExitCode)" }
    Write-Info "robocopy finished with exit code $($proc.ExitCode)"
}
else{
    if($WhatIf){
        Write-Info "WhatIf: Copy-Item -Path (Join-Path $Source '*') -Destination $Destination -Recurse -Force"
    } else {
        Write-Info "Copying files with Copy-Item..."
        Copy-Item -Path (Join-Path $Source '*') -Destination $Destination -Recurse -Force
        Write-Info "Copy complete."
    }
}

Write-Info "Sync complete. Please open Unity and select Assets/StreamingAssets/Audio/GeneratedSoundBanks then right-click → Reimport.`nAlternatively run Assets → Reimport All in Unity.`n"
