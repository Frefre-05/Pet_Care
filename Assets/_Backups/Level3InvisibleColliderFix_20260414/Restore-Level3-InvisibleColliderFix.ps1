$ErrorActionPreference = 'Stop'

$backupDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$backupScene = Join-Path $backupDir 'Level 3.unity.before-invisible-collider-fix'
$targetScene = 'Assets\Pixel Adventure 1\Scenes 1\Level 3.unity'

Copy-Item -LiteralPath $backupScene -Destination $targetScene -Force
Write-Output 'Level 3 invisible collider fix restored from backup.'
