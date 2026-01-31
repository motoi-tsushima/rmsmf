#Requires -Version 5.1
<#
.SYNOPSIS
    rmsmf と txprobe の結合テスト

.DESCRIPTION
    実際の exe ファイルを使用して、rmsmf と txprobe の連携動作をテストします。
    以下のシナリオをカバーします：
    1. txprobe で検索 → rmsmf で置換
    2. エンコーディング変換とBOM制御の連携
    3. 複数ファイルの一括処理
    4. CSV ファイルを使った複数文字列置換

.PARAMETER RmsmfPath
    rmsmf.exe のパス (デフォルト: .\rmsmf\bin\Debug\rmsmf.exe)

.PARAMETER TxprobePath
    txprobe.exe のパス (デフォルト: .\txprobe\bin\Debug\txprobe.exe)

.PARAMETER Configuration
    ビルド構成 (Debug または Release)

.EXAMPLE
    .\integration-test.ps1
    
.EXAMPLE
    .\integration-test.ps1 -Configuration Release
#>

param(
    [string]$RmsmfPath = "",
    [string]$TxprobePath = "",
    [ValidateSet("Debug", "Release")]
    [string]$Configuration = "Release"
)

$ErrorActionPreference = "Stop"
$OriginalEncoding = [Console]::OutputEncoding
[Console]::OutputEncoding = [System.Text.Encoding]::UTF8

# 実行結果の統計
$script:TestResults = @{
    Passed = 0
    Failed = 0
    Scenarios = @()
}

# exe のパスを設定
if (-not $RmsmfPath) {
    $RmsmfPath = Join-Path $PSScriptRoot "rmsmf\bin\$Configuration\rmsmf.exe"
}
if (-not $TxprobePath) {
    $TxprobePath = Join-Path $PSScriptRoot "txprobe\bin\$Configuration\txprobe.exe"
}

# GoogleDrive 同期の影響を避けるため、C:\_test 配下にテストディレクトリを作成
$testDir = "C:\_test\rmsmf-integration-test"

#region ヘルパー関数

function Write-TestHeader {
    param([string]$Message)
    Write-Host "`n$('=' * 70)" -ForegroundColor Cyan
    Write-Host "  $Message" -ForegroundColor Cyan
    Write-Host "$('=' * 70)" -ForegroundColor Cyan
}

function Write-TestStep {
    param([string]$Message)
    Write-Host "  → $Message" -ForegroundColor Gray
}

function Write-TestSuccess {
    param([string]$Message)
    Write-Host "  ✓ $Message" -ForegroundColor Green
    $script:TestResults.Passed++
}

function Write-TestFailure {
    param([string]$Message, [string]$Details = "")
    Write-Host "  ✗ $Message" -ForegroundColor Red
    if ($Details) {
        Write-Host "    詳細: $Details" -ForegroundColor Yellow
    }
    $script:TestResults.Failed++
}

function Invoke-TestScenario {
    param(
        [string]$Name,
        [scriptblock]$Test
    )
    
    Write-TestHeader $Name
    
    try {
        & $Test
        $script:TestResults.Scenarios += @{
            Name = $Name
            Result = "Success"
            Error = $null
        }
    }
    catch {
        Write-TestFailure "シナリオが失敗しました" $_.Exception.Message
        $script:TestResults.Scenarios += @{
            Name = $Name
            Result = "Failed"
            Error = $_.Exception.Message
        }
    }
}

function Assert-FileContains {
    param(
        [string]$FilePath,
        [string]$ExpectedContent,
        [string]$Message
    )
    
    if (-not (Test-Path $FilePath)) {
        throw "ファイルが見つかりません: $FilePath"
    }
    
    $actualContent = Get-Content $FilePath -Raw -Encoding UTF8
    if ($actualContent -notmatch [regex]::Escape($ExpectedContent)) {
        throw "$Message`n期待値: $ExpectedContent`n実際の値: $actualContent"
    }
    
    Write-TestSuccess $Message
}

function Assert-FileNotContains {
    param(
        [string]$FilePath,
        [string]$UnexpectedContent,
        [string]$Message
    )
    
    if (-not (Test-Path $FilePath)) {
        throw "ファイルが見つかりません: $FilePath"
    }
    
    $actualContent = Get-Content $FilePath -Raw -Encoding UTF8
    if ($actualContent -match [regex]::Escape($UnexpectedContent)) {
        throw "$Message`n期待しない文字列が含まれています: $UnexpectedContent"
    }
    
    Write-TestSuccess $Message
}

function Assert-FileHasBOM {
    param(
        [string]$FilePath,
        [string]$Message
    )
    
    $fullPath = if ([System.IO.Path]::IsPathRooted($FilePath)) { $FilePath } else { Join-Path (Get-Location) $FilePath }
    $bytes = [System.IO.File]::ReadAllBytes($fullPath)
    if ($bytes.Length -lt 3 -or $bytes[0] -ne 0xEF -or $bytes[1] -ne 0xBB -or $bytes[2] -ne 0xBF) {
        throw "$Message`nBOMが見つかりませんでした"
    }
    
    Write-TestSuccess $Message
}

function Assert-FileNoBOM {
    param(
        [string]$FilePath,
        [string]$Message
    )
    
    $fullPath = if ([System.IO.Path]::IsPathRooted($FilePath)) { $FilePath } else { Join-Path (Get-Location) $FilePath }
    $bytes = [System.IO.File]::ReadAllBytes($fullPath)
    if ($bytes.Length -ge 3 -and $bytes[0] -eq 0xEF -and $bytes[1] -eq 0xBB -and $bytes[2] -eq 0xBF) {
        throw "$Message`nBOMが見つかりました（期待は BOM なし）"
    }
    
    Write-TestSuccess $Message
}

#endregion

#region セットアップとクリーンアップ

function Setup-TestEnvironment {
    Write-TestHeader "環境セットアップ"
    
    # exe の存在確認
    if (-not (Test-Path $RmsmfPath)) {
        throw "rmsmf.exe が見つかりません: $RmsmfPath`nビルドを実行してください: msbuild rmsmf.sln /p:Configuration=$Configuration"
    }
    Write-TestStep "rmsmf.exe を確認: $RmsmfPath"
    
    if (-not (Test-Path $TxprobePath)) {
        throw "txprobe.exe が見つかりません: $TxprobePath`nビルドを実行してください: msbuild rmsmf.sln /p:Configuration=$Configuration"
    }
    Write-TestStep "txprobe.exe を確認: $TxprobePath"
    
    # テストディレクトリの準備（C:\_test が存在しない場合は作成）
    $testRoot = "C:\_test"
    if (-not (Test-Path $testRoot)) {
        New-Item -ItemType Directory -Path $testRoot | Out-Null
        Write-TestStep "テストルートディレクトリを作成: $testRoot"
    }
    
    if (Test-Path $testDir) {
        Remove-Item -Recurse -Force $testDir
    }
    New-Item -ItemType Directory -Path $testDir | Out-Null
    Write-TestStep "テストディレクトリを作成: $testDir"
    
    # カレントディレクトリを test-workspace に変更
    Push-Location $testDir
    Write-TestStep "カレントディレクトリを変更: $testDir"
    
    Write-TestSuccess "環境セットアップ完了"
}

function Cleanup-TestEnvironment {
    Write-TestHeader "クリーンアップ"
    
    # 元のディレクトリに戻る
    Pop-Location
    
    # テスト失敗時はディレクトリを保持
    if ($script:TestResults.Failed -gt 0) {
        Write-Host "  ⚠ テストが失敗したため、テストディレクトリを保持します: $testDir" -ForegroundColor Yellow
        Write-Host "    デバッグ後、手動で削除してください。" -ForegroundColor Yellow
    }
    else {
        if (Test-Path $testDir) {
            Remove-Item -Recurse -Force $testDir
            Write-TestSuccess "テストディレクトリを削除しました"
        }
    }
}

#endregion

#region テストシナリオ

function Test-Scenario1-SearchAndReplace {
    # シナリオ1: txprobe で検索 → rmsmf で置換
    
    Write-TestStep "テストファイルを作成"
    @"
using System;

public class Sample
{
    private string oldName = "OldValue";
    private int oldCount = 100;
    
    public void OldMethod()
    {
        Console.WriteLine("This is an old implementation.");
    }
}
"@ | Out-File -FilePath "Sample.cs" -Encoding UTF8
    
    # 検索単語ファイルを作成（カレントディレクトリに）
    "oldName" | Out-File -FilePath "search-words.txt" -Encoding UTF8
    
    # txprobe で "oldName" を検索
    Write-TestStep "txprobe で 'oldName' を検索"
    Write-Host "    実行コマンド: $TxprobePath /s:search-words.txt /d *.cs" -ForegroundColor DarkGray
    
    $searchResult = & $TxprobePath /s:"search-words.txt" /d "*.cs" 2>&1 | Out-String
    
    if ($LASTEXITCODE -ne 0) {
        throw "txprobe の実行に失敗しました (終了コード: $LASTEXITCODE):`n$searchResult"
    }
    
    if ($searchResult -notmatch "Sample.cs") {
        throw "期待されたファイルが検索結果に含まれていません。結果:`n$searchResult"
    }
    Write-TestSuccess "txprobe で対象ファイルを検出"
    
    # CSV 置換ファイルを作成
    Write-TestStep "CSV 置換ファイルを作成"
    "oldName,newName" | Out-File -FilePath "replace.csv" -Encoding UTF8
    
    # rmsmf で置換実行
    Write-TestStep "rmsmf で 'oldName' → 'newName' に置換"
    Write-Host "    実行コマンド: $RmsmfPath /d /r:replace.csv *.cs" -ForegroundColor DarkGray
    
    $replaceResult = & $RmsmfPath /d /r:"replace.csv" "*.cs" 2>&1 | Out-String
    
    if ($LASTEXITCODE -ne 0) {
        throw "rmsmf の実行に失敗しました (終了コード: $LASTEXITCODE):`n$replaceResult"
    }
    
    # 結果の検証
    Assert-FileContains -FilePath "Sample.cs" -ExpectedContent "newName" `
        -Message "置換後のファイルに 'newName' が含まれている"
    
    Assert-FileNotContains -FilePath "Sample.cs" -UnexpectedContent "oldName" `
        -Message "置換後のファイルに 'oldName' が含まれていない"
}

function Test-Scenario2-EncodingAndBOM {
# シナリオ2: エンコーディング変換とBOM制御の連携
    
Write-TestStep "Shift-JIS (BOMなし) のファイルを作成"
$content = "日本語のテストファイルです。`r`nShift-JIS エンコーディングで保存されています。"
$sjis = [System.Text.Encoding]::GetEncoding("shift_jis")
$filePath = Join-Path (Get-Location) "encoding-test.txt"
[System.IO.File]::WriteAllText($filePath, $content, $sjis)
    
# txprobe でエンコーディングを確認
Write-TestStep "txprobe でエンコーディングを確認"
Write-Host "    実行コマンド: $TxprobePath encoding-test.txt" -ForegroundColor DarkGray
    
$probeResult = & $TxprobePath "encoding-test.txt" 2>&1 | Out-String
    
    if ($LASTEXITCODE -ne 0) {
        throw "txprobe の実行に失敗しました (終了コード: $LASTEXITCODE):`n$probeResult"
    }
    
    # Shift-JIS が検出されることを確認
    if ($probeResult -notmatch "shift") {
        Write-Host "警告: Shift-JIS の検出結果が期待と異なる可能性があります:`n$probeResult" -ForegroundColor Yellow
    }
    else {
        Write-TestSuccess "txprobe が Shift-JIS エンコーディングを検出"
    }
    
    # rmsmf で UTF-8 (BOM付き) に変換 (/r オプションなし)
    Write-TestStep "rmsmf で UTF-8 (BOM付き) に変換"
    Write-Host "    実行コマンド: $RmsmfPath encoding-test.txt /c:shift_jis /w:utf-8 /b:true" -ForegroundColor DarkGray
    
    $convertResult = & $RmsmfPath "encoding-test.txt" /c:"shift_jis" /w:"utf-8" /b:"true" 2>&1 | Out-String
    
    if ($LASTEXITCODE -ne 0) {
        throw "rmsmf の実行に失敗しました (終了コード: $LASTEXITCODE):`n$convertResult"
    }
    
    # BOMの確認
    Assert-FileHasBOM -FilePath "encoding-test.txt" -Message "UTF-8 BOM が正しく追加されている"
    
    # 内容が保持されていることを確認
    Assert-FileContains -FilePath "encoding-test.txt" -ExpectedContent "日本語のテストファイル" `
        -Message "エンコーディング変換後も内容が保持されている"
}

function Test-Scenario3-MultipleFiles {
    # シナリオ3: 複数ファイルの一括処理
    
    Write-TestStep "複数のテストファイルを作成"
    
    # 3つのファイルを作成
    $files = @(
        @{ Name = "File1.txt"; Content = "This is file 1 with PLACEHOLDER text." }
        @{ Name = "File2.txt"; Content = "This is file 2 with PLACEHOLDER text." }
        @{ Name = "File3.txt"; Content = "This is file 3 with PLACEHOLDER text." }
    )
    
    foreach ($file in $files) {
        $file.Content | Out-File -FilePath $file.Name -Encoding UTF8
    }
    
    # 検索単語ファイルを作成
    "PLACEHOLDER" | Out-File -FilePath "search-placeholder.txt" -Encoding UTF8
    
    # txprobe で "PLACEHOLDER" を含むファイルを検索
    Write-TestStep "txprobe で 'PLACEHOLDER' を含むファイルを検索"
    Write-Host "    実行コマンド: $TxprobePath /s:search-placeholder.txt /d *.txt" -ForegroundColor DarkGray
    
    $searchResult = & $TxprobePath /s:"search-placeholder.txt" /d "*.txt" 2>&1 | Out-String
    
    if ($LASTEXITCODE -ne 0) {
        throw "txprobe の実行に失敗しました (終了コード: $LASTEXITCODE):`n$searchResult"
    }
    
    # 3つのファイルすべてが検出されることを確認
    $detectedCount = ([regex]::Matches($searchResult, "File\d\.txt")).Count
    if ($detectedCount -ne 3) {
        throw "期待される3つのファイルが検出されませんでした (検出数: $detectedCount)。結果:`n$searchResult"
    }
    Write-TestSuccess "txprobe が3つのファイルすべてを検出"
    
    # CSV 置換ファイルを作成
    Write-TestStep "CSV 置換ファイルを作成"
    "PLACEHOLDER,ACTUAL_VALUE" | Out-File -FilePath "replace-placeholder.csv" -Encoding UTF8
    
    # rmsmf で一括置換
    Write-TestStep "rmsmf で 'PLACEHOLDER' → 'ACTUAL_VALUE' に一括置換"
    Write-Host "    実行コマンド: $RmsmfPath /d /r:replace-placeholder.csv *.txt" -ForegroundColor DarkGray
    
    $replaceResult = & $RmsmfPath /d /r:"replace-placeholder.csv" "*.txt" 2>&1 | Out-String
    
    if ($LASTEXITCODE -ne 0) {
        throw "rmsmf の実行に失敗しました (終了コード: $LASTEXITCODE):`n$replaceResult"
    }
    
    # すべてのファイルで置換されたことを確認
    foreach ($file in $files) {
        Assert-FileContains -FilePath $file.Name -ExpectedContent "ACTUAL_VALUE" `
            -Message "$($file.Name) で置換が成功"
        
        Assert-FileNotContains -FilePath $file.Name -UnexpectedContent "PLACEHOLDER" `
            -Message "$($file.Name) に元の文字列が残っていない"
    }
}

function Test-Scenario4-CSVMultipleReplace {
    # シナリオ4: CSV ファイルを使った複数文字列置換
    
    Write-TestStep "テストファイルを作成"
    @"
<configuration>
    <database>
        <server>localhost</server>
        <port>5432</port>
        <database>testdb</database>
        <username>admin</username>
        <password>password123</password>
    </database>
    <environment>development</environment>
</configuration>
"@ | Out-File -FilePath "Config.xml" -Encoding UTF8
    
    # CSV 置換定義ファイルを作成
    Write-TestStep "CSV 置換定義ファイルを作成"
    @"
localhost,production-server.example.com
testdb,productiondb
development,production
admin,prod_user
password123,SecureP@ssw0rd!
"@ | Out-File -FilePath "replace.csv" -Encoding UTF8
    
    # 検索単語ファイルを作成
    "localhost" | Out-File -FilePath "search-localhost.txt" -Encoding UTF8
    
    # txprobe で現在の設定値を確認
    Write-TestStep "txprobe で現在の設定を確認"
    Write-Host "    実行コマンド: $TxprobePath /s:search-localhost.txt /d *.xml" -ForegroundColor DarkGray
    
    $probeResult = & $TxprobePath /s:"search-localhost.txt" /d "*.xml" 2>&1 | Out-String
    
    if ($LASTEXITCODE -ne 0) {
        throw "txprobe の実行に失敗しました (終了コード: $LASTEXITCODE):`n$probeResult"
    }
    
    if ($probeResult -match "Config.xml") {
        Write-TestSuccess "txprobe が設定ファイルを検出"
    }
    
    # rmsmf で CSV を使って一括置換
    Write-TestStep "rmsmf で CSV を使って複数の設定値を置換"
    Write-Host "    実行コマンド: $RmsmfPath /d /r:replace.csv *.xml" -ForegroundColor DarkGray
    
    $replaceResult = & $RmsmfPath /d /r:"replace.csv" "*.xml" 2>&1 | Out-String
    
    if ($LASTEXITCODE -ne 0) {
        throw "rmsmf の実行に失敗しました (終了コード: $LASTEXITCODE):`n$replaceResult"
    }
    
    # すべての置換が正しく行われたことを確認
    Assert-FileContains -FilePath "Config.xml" -ExpectedContent "production-server.example.com" `
        -Message "サーバー名が置換されている"
    
    Assert-FileContains -FilePath "Config.xml" -ExpectedContent "productiondb" `
        -Message "データベース名が置換されている"
    
    Assert-FileContains -FilePath "Config.xml" -ExpectedContent "production" `
        -Message "環境名が置換されている"
    
    Assert-FileContains -FilePath "Config.xml" -ExpectedContent "prod_user" `
        -Message "ユーザー名が置換されている"
    
    Assert-FileContains -FilePath "Config.xml" -ExpectedContent "SecureP@ssw0rd!" `
        -Message "パスワードが置換されている"
    
    # 元の値が残っていないことを確認
    Assert-FileNotContains -FilePath "Config.xml" -UnexpectedContent "localhost" `
        -Message "元のサーバー名が残っていない"
    
    Assert-FileNotContains -FilePath "Config.xml" -UnexpectedContent "testdb" `
        -Message "元のデータベース名が残っていない"
}

function Test-Scenario5-BOMControl {
# シナリオ5: BOM の追加と削除
    
Write-TestStep "UTF-8 (BOM なし) のファイルを作成"
$utf8NoBom = New-Object System.Text.UTF8Encoding $false
$filePath = Join-Path (Get-Location) "bom-test.txt"
[System.IO.File]::WriteAllText($filePath, "UTF-8 without BOM", $utf8NoBom)
    
    # BOM がないことを確認
    Assert-FileNoBOM -FilePath "bom-test.txt" -Message "初期状態で BOM がない"
    
    # rmsmf で BOM を追加 (/r オプションなし)
    Write-TestStep "rmsmf で BOM を追加"
    Write-Host "    実行コマンド: $RmsmfPath bom-test.txt /w:utf-8 /b:true" -ForegroundColor DarkGray
    
    $addBomResult = & $RmsmfPath "bom-test.txt" /w:"utf-8" /b:"true" 2>&1 | Out-String
    
    if ($LASTEXITCODE -ne 0) {
        throw "rmsmf の実行に失敗しました (終了コード: $LASTEXITCODE):`n$addBomResult"
    }
    
    Assert-FileHasBOM -FilePath "bom-test.txt" -Message "BOM が正しく追加されている"
    
    # rmsmf で BOM を削除
    Write-TestStep "rmsmf で BOM を削除"
    Write-Host "    実行コマンド: $RmsmfPath bom-test.txt /w:utf-8 /b:false" -ForegroundColor DarkGray
    
    $removeBomResult = & $RmsmfPath "bom-test.txt" /w:"utf-8" /b:"false" 2>&1 | Out-String
    
    if ($LASTEXITCODE -ne 0) {
        throw "rmsmf の実行に失敗しました (終了コード: $LASTEXITCODE):`n$removeBomResult"
    }
    
    Assert-FileNoBOM -FilePath "bom-test.txt" -Message "BOM が正しく削除されている"
    
    # 内容が保持されていることを確認
    Assert-FileContains -FilePath "bom-test.txt" -ExpectedContent "UTF-8 without BOM" `
        -Message "BOM 操作後も内容が保持されている"
}

#endregion

#region メイン実行

function Show-Summary {
    Write-Host "`n$('=' * 70)" -ForegroundColor Cyan
    Write-Host "  テスト実行結果サマリー" -ForegroundColor Cyan
    Write-Host "$('=' * 70)" -ForegroundColor Cyan
    
    $totalTests = $script:TestResults.Passed + $script:TestResults.Failed
    $successRate = if ($totalTests -gt 0) { 
        [math]::Round(($script:TestResults.Passed / $totalTests) * 100, 2) 
    } else { 
        0 
    }
    
    Write-Host ""
    Write-Host "  合計アサーション数: $totalTests" -ForegroundColor White
    Write-Host "  成功: $($script:TestResults.Passed)" -ForegroundColor Green
    Write-Host "  失敗: $($script:TestResults.Failed)" -ForegroundColor $(if ($script:TestResults.Failed -gt 0) { "Red" } else { "Green" })
    Write-Host "  成功率: $successRate%" -ForegroundColor $(if ($successRate -eq 100) { "Green" } else { "Yellow" })
    Write-Host ""
    
    Write-Host "  シナリオ別結果:" -ForegroundColor White
    foreach ($scenario in $script:TestResults.Scenarios) {
        if ($scenario.Result -eq "Skipped") {
            $icon = "○"
            $color = "Yellow"
        }
        elseif ($scenario.Result -eq "Success") {
            $icon = "✓"
            $color = "Green"
        }
        else {
            $icon = "✗"
            $color = "Red"
        }
        
        Write-Host "    $icon $($scenario.Name)" -ForegroundColor $color
        
        if ($scenario.Error) {
            if ($scenario.Result -eq "Skipped") {
                Write-Host "      理由: $($scenario.Error)" -ForegroundColor Yellow
            }
            else {
                Write-Host "      エラー: $($scenario.Error)" -ForegroundColor Yellow
            }
        }
    }
    
    Write-Host ""
    Write-Host "$('=' * 70)" -ForegroundColor Cyan
    
    if ($script:TestResults.Failed -eq 0) {
        Write-Host "  すべてのテストが成功しました！ 🎉" -ForegroundColor Green
    }
    else {
        Write-Host "  一部のテストが失敗しました。" -ForegroundColor Red
    }
    Write-Host "$('=' * 70)" -ForegroundColor Cyan
}

# メイン実行
try {
    Write-Host @"

╔═══════════════════════════════════════════════════════════════════╗
║                                                                   ║
║   rmsmf & txprobe 結合テストスイート                              ║
║                                                                   ║
╚═══════════════════════════════════════════════════════════════════╝

"@ -ForegroundColor Cyan

    Setup-TestEnvironment
    
    # 各シナリオを実行
    Invoke-TestScenario -Name "シナリオ1: txprobe 検索 → rmsmf 置換" `
        -Test { Test-Scenario1-SearchAndReplace }
    
    Invoke-TestScenario -Name "シナリオ2: エンコーディング変換とBOM制御" `
        -Test { Test-Scenario2-EncodingAndBOM }
    
    Invoke-TestScenario -Name "シナリオ3: 複数ファイルの一括処理" `
        -Test { Test-Scenario3-MultipleFiles }
    
    Invoke-TestScenario -Name "シナリオ4: CSV ファイルを使った複数文字列置換" `
        -Test { Test-Scenario4-CSVMultipleReplace }
    
    Invoke-TestScenario -Name "シナリオ5: BOM の追加と削除" `
        -Test { Test-Scenario5-BOMControl }
    
    Show-Summary
    
    exit $(if ($script:TestResults.Failed -eq 0) { 0 } else { 1 })
}
catch {
    Write-Host "`n致命的エラーが発生しました:" -ForegroundColor Red
    Write-Host $_.Exception.Message -ForegroundColor Red
    Write-Host $_.ScriptStackTrace -ForegroundColor Yellow
    exit 1
}
finally {
    Cleanup-TestEnvironment
    [Console]::OutputEncoding = $OriginalEncoding
}

#endregion
