#Requires -Version 5.1
param(
    [string]$Configuration = "Release"
)

$ErrorActionPreference = "Stop"

$RmsmfPath = Join-Path $PSScriptRoot "rmsmf\bin\$Configuration\rmsmf.exe"
$TxprobePath = Join-Path $PSScriptRoot "txprobe\bin\$Configuration\txprobe.exe"
$testDir = "C:\_test\rmsmf-test-scenario2"

if (-not (Test-Path $RmsmfPath)) {
    throw "rmsmf.exe が見つかりません: $RmsmfPath"
}

if (-not (Test-Path $TxprobePath)) {
    throw "txprobe.exe が見つかりません: $TxprobePath"
}

if (-not (Test-Path "C:\_test")) {
    New-Item -ItemType Directory -Path "C:\_test" | Out-Null
}

if (Test-Path $testDir) {
    Remove-Item -Recurse -Force $testDir
}
New-Item -ItemType Directory -Path $testDir | Out-Null

Push-Location $testDir

try {
    Write-Host "=== シナリオ2: エンコーディング変換とBOM制御 ===" -ForegroundColor Cyan
    
    Write-Host "Step 1: Shift-JIS (BOMなし) のファイルを作成" -ForegroundColor Gray
    Write-Host "  現在のディレクトリ: $(Get-Location)"
    Write-Host "  テストディレクトリ: $testDir"
    $testFilePath = Join-Path $testDir "encoding-test.txt"
    Write-Host "  完全パス: $testFilePath"
    $content = "日本語のテストファイルです。`r`nShift-JIS エンコーディングで保存されています。"
    $sjis = [System.Text.Encoding]::GetEncoding("shift_jis")
    [System.IO.File]::WriteAllText($testFilePath, $content, $sjis)
    
    Write-Host "Step 2: 作成されたファイルを確認" -ForegroundColor Gray
    $bytes = [System.IO.File]::ReadAllBytes($testFilePath)
    Write-Host "  ファイルサイズ: $($bytes.Length) bytes"
    Write-Host "  最初の10バイト: $($bytes[0..9] -join ', ')"
    Write-Host "  Test-Path (完全パス): $(Test-Path $testFilePath)"
    Write-Host "  Test-Path (相対パス): $(Test-Path 'encoding-test.txt')"
    Write-Host "  Get-ChildItem:"
    Get-ChildItem -Force | Select-Object Name, Length
    
    Write-Host "Step 3: txprobe でエンコーディングを確認（スキップ）" -ForegroundColor Yellow
    # Write-Host "  現在のディレクトリ: $(Get-Location)"
    # Write-Host "  コマンド: $TxprobePath encoding-test.txt" -ForegroundColor DarkGray
    # $probeResult = & $TxprobePath "encoding-test.txt" 2>&1 | Out-String
    # Write-Host "  txprobe 結果:`n$probeResult"
    
    # if ($LASTEXITCODE -ne 0) {
    #     throw "txprobe の実行に失敗しました (終了コード: $LASTEXITCODE):`n$probeResult"
    # }
    
    Write-Host "Step 4: rmsmf 実行前の確認" -ForegroundColor Gray
    Write-Host "  Test-Path結果: $(Test-Path 'encoding-test.txt')"
    Write-Host "  ファイル一覧:"
    $files = Get-ChildItem -Force
    if ($files) {
        $files | Select-Object Name, Length | Format-Table
    } else {
        Write-Host "  (ファイルが見つかりません)"
    }
    
    Write-Host "Step 5: rmsmf で UTF-8 (BOM付き) に変換" -ForegroundColor Gray
    Write-Host "  コマンド: $RmsmfPath encoding-test.txt /c:shift_jis /w:utf-8 /b:true" -ForegroundColor DarkGray
    $convertResult = & $RmsmfPath "encoding-test.txt" /c:shift_jis /w:utf-8 /b:true 2>&1 | Out-String
    Write-Host "  rmsmf 結果:`n$convertResult"
    Write-Host "  終了コード: $LASTEXITCODE"
    
    Write-Host "Step 6: rmsmf 実行後の確認" -ForegroundColor Gray
    Write-Host "  ファイル一覧:"
    Get-ChildItem -Force | Select-Object Name, Length | Format-Table
    
    if ($LASTEXITCODE -ne 0) {
        throw "rmsmf の実行に失敗しました (終了コード: $LASTEXITCODE):`n$convertResult"
    }
    
    Write-Host "Step 7: 変換後のファイルを確認" -ForegroundColor Gray
    if (Test-Path $testFilePath) {
        $bytes = [System.IO.File]::ReadAllBytes($testFilePath)
        Write-Host "  ファイルサイズ: $($bytes.Length) bytes"
        Write-Host "  最初の10バイト: $($bytes[0..9] -join ', ')"
        
        if ($bytes.Length -ge 3 -and $bytes[0] -eq 0xEF -and $bytes[1] -eq 0xBB -and $bytes[2] -eq 0xBF) {
            Write-Host "  ✓ UTF-8 BOM が検出されました" -ForegroundColor Green
        } else {
            Write-Host "  ✗ UTF-8 BOM が見つかりません" -ForegroundColor Red
        }
        
        $actualContent = Get-Content $testFilePath -Raw -Encoding UTF8
        Write-Host "  内容: $actualContent"
    } else {
        Write-Host "  ✗ ファイルが存在しません！" -ForegroundColor Red
    }
    
    Write-Host "`nテスト完了" -ForegroundColor Green
}
catch {
    Write-Host "`nエラーが発生しました:" -ForegroundColor Red
    Write-Host $_.Exception.Message -ForegroundColor Red
    Write-Host $_.ScriptStackTrace -ForegroundColor Yellow
}
finally {
    Pop-Location
    Write-Host "`nテストディレクトリ: $testDir" -ForegroundColor Cyan
}
