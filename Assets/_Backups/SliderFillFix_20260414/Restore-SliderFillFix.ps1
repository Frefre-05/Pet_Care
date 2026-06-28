$ErrorActionPreference = 'Stop'
$backupDir = Split-Path -Parent $MyInvocation.MyCommand.Path
Copy-Item -LiteralPath (Join-Path $backupDir 'House.unity.before-slider-fill-fix') -Destination 'Assets\Pixel Adventure 1\Scenes 1\House.unity' -Force
Copy-Item -LiteralPath (Join-Path $backupDir 'Hospital.unity.before-slider-fill-fix') -Destination 'Assets\Pixel Adventure 1\Scenes 1\Hospital.unity' -Force
Write-Output 'Slider fill fix restored from backup.'
