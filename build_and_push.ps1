# Build & Push to itch.io
# Unity 2022.3.62f2 | ypikaeigames/i-vs-blocks:windows

$UNITY     = "C:\Program Files\Unity\Hub\Editor\2022.3.62f2\Editor\Unity.exe"
$PROJECT   = "C:\Users\Marat\Documents\Test\Tutorial_Education"
$BUILD_DIR = "$PROJECT\Build\Windows"
$BUILD_EXE = "$BUILD_DIR\i-vs-blocks.exe"
$LOG_FILE  = "$PROJECT\Build\build.log"
$BUTLER    = "C:\Users\Администратор\Documents\Butler\butler.exe"
$ITCH_GAME = "ypikaeigames/i-vs-blocks:windows"

Write-Host "=== Building Unity project ===" -ForegroundColor Cyan

New-Item -ItemType Directory -Force -Path $BUILD_DIR | Out-Null

& $UNITY `
    -batchmode `
    -quit `
    -projectPath $PROJECT `
    -buildWindows64Player $BUILD_EXE `
    -logFile $LOG_FILE

if ($LASTEXITCODE -ne 0) {
    Write-Host "Build FAILED. See log: $LOG_FILE" -ForegroundColor Red
    exit 1
}

Write-Host "Build SUCCESS" -ForegroundColor Green
Write-Host ""
Write-Host "=== Pushing to itch.io ===" -ForegroundColor Cyan

& $BUTLER push $BUILD_DIR $ITCH_GAME

if ($LASTEXITCODE -ne 0) {
    Write-Host "Push FAILED" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "Done! https://ypikaeigames.itch.io/i-vs-blocks" -ForegroundColor Green
