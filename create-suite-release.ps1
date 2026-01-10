# リリースパッケージ作成スクリプト（統合版）
# 使い方: .\create-suite-release.ps1 -Version "1.0.0"

param(
    [Parameter(Mandatory=$true)]
    [string]$Version,
    
    [Parameter(Mandatory=$false)]
    [string]$Configuration = "Release"
)

$ErrorActionPreference = "Stop"

Write-Host "=== rmsmf Suite リリースパッケージ作成 ===" -ForegroundColor Green
Write-Host "バージョン: $Version" -ForegroundColor Cyan
Write-Host "ビルド構成: $Configuration" -ForegroundColor Cyan

# パス設定
$suiteName = "rmsmf-suite-v$Version"
$releaseDir = Join-Path $PSScriptRoot $suiteName
$binDir = Join-Path $releaseDir "bin"
$zipPath = Join-Path $PSScriptRoot "$suiteName.zip"

$rmsmfExe = Join-Path $PSScriptRoot "rmsmf\bin\$Configuration\rmsmf.exe"
$txprobeExe = Join-Path $PSScriptRoot "txprobe\bin\$Configuration\txprobe.exe"
$readmePath = Join-Path $PSScriptRoot "README.md"
$licensePath = Join-Path $PSScriptRoot "LICENSE.txt"
$changelogPath = Join-Path $PSScriptRoot "CHANGELOG.md"

# ビルド済みファイルの存在確認
Write-Host "`n[1/6] ビルド済みファイルの確認..." -ForegroundColor Yellow

$missingFiles = @()
if (-not (Test-Path $rmsmfExe)) { $missingFiles += "rmsmf.exe" }
if (-not (Test-Path $txprobeExe)) { $missingFiles += "txprobe.exe" }

if ($missingFiles.Count -gt 0) {
    Write-Host "エラー: 以下のファイルが見つかりません" -ForegroundColor Red
    $missingFiles | ForEach-Object { Write-Host "  - $_" -ForegroundColor Red }
    Write-Host "`n先に $Configuration ビルドを実行してください。" -ForegroundColor Yellow
    Write-Host "Visual Studioで:" -ForegroundColor Yellow
    Write-Host "  1. ソリューション構成を '$Configuration' に変更" -ForegroundColor White
    Write-Host "  2. ビルド → ソリューションのリビルド" -ForegroundColor White
    exit 1
}

Write-Host "? ビルド済みファイルを確認しました" -ForegroundColor Green

# バージョン確認
Write-Host "`n[2/6] バージョン番号の確認..." -ForegroundColor Yellow

$rmsmfVersion = (Get-Item $rmsmfExe).VersionInfo.FileVersion
$txprobeVersion = (Get-Item $txprobeExe).VersionInfo.FileVersion

Write-Host "  rmsmf.exe: $rmsmfVersion" -ForegroundColor Cyan
Write-Host "  txprobe.exe: $txprobeVersion" -ForegroundColor Cyan

# リリースフォルダーを作成
Write-Host "`n[3/6] リリースフォルダーの作成..." -ForegroundColor Yellow

if (Test-Path $releaseDir) {
    Write-Host "既存のフォルダーを削除します: $releaseDir" -ForegroundColor Yellow
    Remove-Item $releaseDir -Recurse -Force
}

New-Item -ItemType Directory -Force -Path $releaseDir | Out-Null
New-Item -ItemType Directory -Force -Path $binDir | Out-Null
Write-Host "? フォルダーを作成しました: $releaseDir" -ForegroundColor Green

# ファイルをコピー
Write-Host "`n[4/6] ファイルのコピー..." -ForegroundColor Yellow

# EXEファイル
Copy-Item $rmsmfExe -Destination $binDir
Write-Host "  ? bin/rmsmf.exe" -ForegroundColor Green

Copy-Item $txprobeExe -Destination $binDir
Write-Host "  ? bin/txprobe.exe" -ForegroundColor Green

# ドキュメント
if (Test-Path $readmePath) {
    Copy-Item $readmePath -Destination $releaseDir
    Write-Host "  ? README.md" -ForegroundColor Green
}

if (Test-Path $licensePath) {
    Copy-Item $licensePath -Destination $releaseDir
    Write-Host "  ? LICENSE.txt" -ForegroundColor Green
}

if (Test-Path $changelogPath) {
    Copy-Item $changelogPath -Destination $releaseDir
    Write-Host "  ? CHANGELOG.md" -ForegroundColor Green
}

# インストール手順を作成
Write-Host "`n[5/6] インストール手順の作成..." -ForegroundColor Yellow

$installGuide = @"
# rmsmf Suite インストール方法

## クイックスタート

1. このフォルダーを任意の場所に配置してください
   例: C:\Tools\rmsmf-suite

2. bin フォルダーへのパスを PATH 環境変数に追加してください
   例: C:\Tools\rmsmf-suite\bin

## PATH環境変数の設定方法（Windows 10/11）

1. スタートメニューで「環境変数」を検索
2. 「システム環境変数の編集」をクリック
3. 「環境変数」ボタンをクリック
4. ユーザー環境変数の「Path」を選択して「編集」
5. 「新規」をクリックして、bin フォルダーのフルパスを追加
6. 「OK」をクリックしてすべてのダイアログを閉じる
7. 新しいコマンドプロンプトまたはPowerShellを開いて確認

## 確認方法

PowerShellまたはコマンドプロンプトで以下を実行:

``````powershell
rmsmf /h
txprobe /h
``````

ヘルプが表示されれば成功です。

## 含まれるツール

- **rmsmf.exe** (v$rmsmfVersion) - テキストファイルの文字エンコーディング変換・置換ツール
- **txprobe.exe** (v$txprobeVersion) - テキストファイル探索ツール

詳しい使い方は README.md を参照してください。
"@

$installGuide | Out-File -FilePath (Join-Path $releaseDir "INSTALL.txt") -Encoding UTF8
Write-Host "  ? INSTALL.txt" -ForegroundColor Green

# ZIPファイルを作成
Write-Host "`n[6/6] ZIPファイルの作成..." -ForegroundColor Yellow

if (Test-Path $zipPath) {
    Write-Host "既存のZIPファイルを削除します: $zipPath" -ForegroundColor Yellow
    Remove-Item $zipPath -Force
}

Compress-Archive -Path $releaseDir -DestinationPath $zipPath -CompressionLevel Optimal
Write-Host "? ZIPファイルを作成しました: $zipPath" -ForegroundColor Green

# 完了メッセージ
Write-Host "`n=== リリースパッケージ作成完了 ===" -ForegroundColor Green
Write-Host "フォルダー: $releaseDir" -ForegroundColor Cyan
Write-Host "ZIPファイル: $zipPath" -ForegroundColor Cyan

# ファイルサイズ表示
$zipSize = (Get-Item $zipPath).Length / 1KB
Write-Host "ZIPサイズ: $([math]::Round($zipSize, 2)) KB" -ForegroundColor Cyan

# 内容確認
Write-Host "`nパッケージ内容:" -ForegroundColor Yellow
Get-ChildItem -Path $releaseDir -Recurse -File | ForEach-Object {
    $relativePath = $_.FullName.Substring($releaseDir.Length + 1)
    Write-Host "  $relativePath" -ForegroundColor White
}

Write-Host "`n次のステップ:" -ForegroundColor Yellow
Write-Host "1. ZIPファイルを展開して内容を確認してください" -ForegroundColor White
Write-Host "2. 実際に動作するかテストしてください" -ForegroundColor White
Write-Host "3. GitHubでリリースを作成してください" -ForegroundColor White
Write-Host "   https://github.com/motoi-tsushima/rmsmf/releases/new" -ForegroundColor Cyan
Write-Host "4. タグ名: v$Version" -ForegroundColor White
Write-Host "5. ZIPファイルをアップロードしてください" -ForegroundColor White
