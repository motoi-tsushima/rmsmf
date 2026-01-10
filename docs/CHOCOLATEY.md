# Chocolatey パッケージ作成ガイド

## Chocolatey とは

Windows用のパッケージマネージャー。ユーザーは以下のコマンドでインストール可能になります：

```powershell
choco install rmsmf-suite
```

## パッケージの作成手順

### 1. Chocolatey のインストール

```powershell
Set-ExecutionPolicy Bypass -Scope Process -Force
[System.Net.ServicePointManager]::SecurityProtocol = [System.Net.ServicePointManager]::SecurityProtocol -bor 3072
iex ((New-Object System.Net.WebClient).DownloadString('https://community.chocolatey.org/install.ps1'))
```

### 2. パッケージテンプレートの作成

```powershell
mkdir chocolatey
cd chocolatey
choco new rmsmf-suite
```

### 3. nuspec ファイルの編集

`rmsmf-suite.nuspec`:

```xml
<?xml version="1.0" encoding="utf-8"?>
<package xmlns="http://schemas.microsoft.com/packaging/2015/06/nuspec.xsd">
  <metadata>
    <id>rmsmf-suite</id>
    <version>1.0.0</version>
    <packageSourceUrl>https://github.com/motoi-tsushima/rmsmf</packageSourceUrl>
    <owners>motoi-tsushima</owners>
    <title>rmsmf Suite</title>
    <authors>motoi-tsushima</authors>
    <projectUrl>https://github.com/motoi-tsushima/rmsmf</projectUrl>
    <licenseUrl>https://github.com/motoi-tsushima/rmsmf/blob/master/LICENSE.txt</licenseUrl>
    <requireLicenseAcceptance>false</requireLicenseAcceptance>
    <description>
Text file encoding conversion and search tools.

- rmsmf: Replace Multiple Strings in Multiple Files
- txprobe: Text file probe tool

Features:
- Character encoding conversion (UTF-8, Shift_JIS, EUC-JP, etc.)
- String replacement in multiple files
- File encoding and line break detection
- String search in files
    </description>
    <summary>Text file encoding conversion and search tools</summary>
    <releaseNotes>https://github.com/motoi-tsushima/rmsmf/blob/master/CHANGELOG.md</releaseNotes>
    <tags>text encoding conversion tool cli</tags>
  </metadata>
  <files>
    <file src="tools\**" target="tools" />
  </files>
</package>
```

### 4. インストールスクリプトの作成

`tools/chocolateyinstall.ps1`:

```powershell
$ErrorActionPreference = 'Stop'

$packageName = 'rmsmf-suite'
$toolsDir = "$(Split-Path -parent $MyInvocation.MyCommand.Definition)"
$url = 'https://github.com/motoi-tsushima/rmsmf/releases/download/v1.0.0/rmsmf-suite-v1.0.0.zip'

$packageArgs = @{
  packageName   = $packageName
  unzipLocation = $toolsDir
  url           = $url
  checksum      = 'CHECKSUM_HERE'
  checksumType  = 'sha256'
}

Install-ChocolateyZipPackage @packageArgs

# Add to PATH
$binPath = Join-Path $toolsDir 'rmsmf-suite-v1.0.0\bin'
Install-ChocolateyPath -PathToInstall $binPath -PathType 'Machine'
```

### 5. パッケージのビルドとテスト

```powershell
# パッケージをビルド
choco pack

# ローカルでテスト
choco install rmsmf-suite -s . -y

# 確認
rmsmf /h
txprobe /h

# アンインストール
choco uninstall rmsmf-suite -y
```

### 6. Chocolatey Community Repository に公開

1. https://community.chocolatey.org/ でアカウント作成
2. API キーを取得
3. パッケージをプッシュ

```powershell
choco apikey --key YOUR_API_KEY --source https://push.chocolatey.org/
choco push rmsmf-suite.1.0.0.nupkg --source https://push.chocolatey.org/
```

## 参考リンク

- [Chocolatey 公式ドキュメント](https://docs.chocolatey.org/)
- [パッケージ作成ガイド](https://docs.chocolatey.org/en-us/create/create-packages)
