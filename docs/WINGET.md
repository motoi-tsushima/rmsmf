# WinGet パッケージ化ガイド

## WinGet とは

Windows Package Manager。Windows 10/11 に標準搭載されています。
ユーザーは以下のコマンドでインストール可能になります：

```powershell
winget install motoi-tsushima.rmsmf-suite
```

## パッケージの作成手順

### 1. Manifest ファイルの作成

WinGetパッケージは、YAMLファイル（マニフェスト）で定義します。

#### パッケージマニフェスト: `manifests/m/motoi-tsushima/rmsmf-suite/1.0.0/motoi-tsushima.rmsmf-suite.yaml`

```yaml
# Package metadata
PackageIdentifier: motoi-tsushima.rmsmf-suite
PackageVersion: 1.0.0
PackageLocale: en-US
Publisher: motoi-tsushima
PublisherUrl: https://github.com/motoi-tsushima
PublisherSupportUrl: https://github.com/motoi-tsushima/rmsmf/issues
Author: motoi-tsushima
PackageName: rmsmf Suite
PackageUrl: https://github.com/motoi-tsushima/rmsmf
License: MIT
LicenseUrl: https://github.com/motoi-tsushima/rmsmf/blob/master/LICENSE.txt
Copyright: Copyright (c) 2020 motoi.tsushima
ShortDescription: Text file encoding conversion and search tools
Description: |-
  rmsmf Suite provides two command-line tools for text file processing:
  
  - rmsmf: Replace Multiple Strings in Multiple Files
    - Character encoding conversion (UTF-8, Shift_JIS, EUC-JP, etc.)
    - String replacement in multiple files
    - BOM control
  
  - txprobe: Text file probe tool
    - File encoding detection
    - Line break detection (CRLF, LF, CR)
    - String search in files
Tags:
  - text
  - encoding
  - conversion
  - cli
  - tool
Moniker: rmsmf
ReleaseNotesUrl: https://github.com/motoi-tsushima/rmsmf/releases/tag/v1.0.0
ManifestType: defaultLocale
ManifestVersion: 1.4.0
```

#### インストーラーマニフェスト: `manifests/m/motoi-tsushima/rmsmf-suite/1.0.0/motoi-tsushima.rmsmf-suite.installer.yaml`

```yaml
PackageIdentifier: motoi-tsushima.rmsmf-suite
PackageVersion: 1.0.0
Platform:
  - Windows.Desktop
MinimumOSVersion: 10.0.0.0
InstallerType: zip
NestedInstallerType: portable
NestedInstallerFiles:
  - RelativeFilePath: rmsmf-suite-v1.0.0\bin\rmsmf.exe
    PortableCommandAlias: rmsmf
  - RelativeFilePath: rmsmf-suite-v1.0.0\bin\txprobe.exe
    PortableCommandAlias: txprobe
Installers:
  - Architecture: neutral
    InstallerUrl: https://github.com/motoi-tsushima/rmsmf/releases/download/v1.0.0/rmsmf-suite-v1.0.0.zip
    InstallerSha256: CHECKSUM_HERE
ManifestType: installer
ManifestVersion: 1.4.0
```

#### バージョンマニフェスト: `manifests/m/motoi-tsushima/rmsmf-suite/1.0.0/motoi-tsushima.rmsmf-suite.version.yaml`

```yaml
PackageIdentifier: motoi-tsushima.rmsmf-suite
PackageVersion: 1.0.0
DefaultLocale: en-US
ManifestType: version
ManifestVersion: 1.4.0
```

### 2. マニフェストの検証

```powershell
# WinGet CLIツールをインストール
# (Windows 11には標準搭載)

# マニフェストを検証
winget validate --manifest manifests\m\motoi-tsushima\rmsmf-suite\1.0.0\
```

### 3. ローカルでテスト

```powershell
# ローカルマニフェストからインストール
winget install --manifest manifests\m\motoi-tsushima\rmsmf-suite\1.0.0\motoi-tsushima.rmsmf-suite.yaml

# 確認
rmsmf /h
txprobe /h

# アンインストール
winget uninstall motoi-tsushima.rmsmf-suite
```

### 4. Microsoft の WinGet Repository に公開

1. [microsoft/winget-pkgs](https://github.com/microsoft/winget-pkgs) リポジトリをフォーク
2. マニフェストファイルを追加
3. Pull Request を作成

**PR の作成手順:**

```bash
# リポジトリをクローン
git clone https://github.com/YOUR_USERNAME/winget-pkgs.git
cd winget-pkgs

# ブランチを作成
git checkout -b rmsmf-suite-1.0.0

# マニフェストを追加
mkdir -p manifests/m/motoi-tsushima/rmsmf-suite/1.0.0
# 上記のYAMLファイルを配置

# コミット
git add manifests/m/motoi-tsushima/rmsmf-suite/1.0.0/
git commit -m "Add rmsmf-suite version 1.0.0"

# プッシュ
git push origin rmsmf-suite-1.0.0
```

GitHub で Pull Request を作成し、レビューを待ちます。

### 5. 自動化スクリプト

SHA256チェックサムを自動計算してマニフェストを生成するスクリプト：

```powershell
# generate-winget-manifest.ps1
param(
    [string]$Version,
    [string]$ZipPath
)

# SHA256を計算
$hash = (Get-FileHash -Path $ZipPath -Algorithm SHA256).Hash

# マニフェストファイルを生成
# (上記のYAMLテンプレートを使用)
```

## 参考リンク

- [WinGet 公式ドキュメント](https://docs.microsoft.com/windows/package-manager/)
- [マニフェスト作成ガイド](https://docs.microsoft.com/windows/package-manager/package/manifest)
- [winget-pkgs リポジトリ](https://github.com/microsoft/winget-pkgs)
