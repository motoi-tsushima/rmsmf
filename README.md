# rmsmf - Replace Multiple Strings in Multiple Files

[![.NET Framework](https://img.shields.io/badge/.NET%20Framework-4.8-blue.svg)](https://dotnet.microsoft.com/download/dotnet-framework/net48)
[![License](https://img.shields.io/badge/license-MIT-green.svg)](LICENSE)
[![Tests](https://img.shields.io/badge/tests-33%20passing-brightgreen.svg)](rmsmf.Tests)

複数のファイル内の複数の文字列を一括置換するコマンドラインツールです。文字エンコーディングの変換や、改行コードの変換に、BOM（Byte Order Mark）の制御も可能です。

## 📑 目次

- [プロジェクト構成](#📦-プロジェクト構成)
- [機能](#✨-機能)
- [インストール](#🚀-インストール)
- [使用方法](#📖-使用方法)
  - [rmsmf - 文字列置換ツール](#rmsmf---文字列置換ツール)
  - [txprobe - テキストファイル検索ツール](#txprobe---テキストファイル検索ツール)
- [アーキテクチャ](#🏗️-アーキテクチャ)
- [リファクタリング内容](#🔄-リファクタリング内容)
- [ビルド方法](#🔨-ビルド方法)
- [テスト](#🧪-テスト)
- [開発環境](#💻-開発環境)
- [ライセンス](#📝-ライセンス)

---

## 📦 プロジェクト構成

このリポジトリには以下の3つのプロジェクトが含まれています：

```
rmsmf/
├── rmsmf/              # 文字列置換ツール（メインプロジェクト）
├── txprobe/            # テキストファイル検索ツール
└── rmsmf.Tests/        # 単体テストプロジェクト
```

### プロジェクト詳細

| プロジェクト | 説明 | 出力タイプ |
|-------------|------|-----------|
| **rmsmf** | 複数ファイルの文字列一括置換 | コンソールアプリ (.exe) |
| **txprobe** | テキストファイル内容の検索・解析 | コンソールアプリ (.exe) |
| **rmsmf.Tests** | 単体テスト (33テスト) | ライブラリ (.dll) |

---

## ✨ 機能

### rmsmf の機能

- ✅ **複数文字列の一括置換**: CSVファイルで指定した複数の検索・置換ペアを一度に処理
- ✅ **ワイルドカード対応**: `*.txt` などのパターンでファイルを指定可能
- ✅ **サブディレクトリ検索**: `/d` オプションでサブディレクトリも処理対象に
- ✅ **文字エンコーディング変換**: Shift_JIS ⇔ UTF-8 などの相互変換
- ✅ **BOM制御**: BOMの追加・削除を制御可能
- ✅ **改行コード変換**: CRLF / LF / CR の相互変換
- ✅ **ファイルリスト対応**: テキストファイルで処理対象ファイルを指定可能
- ✅ **エスケープシーケンス対応**: `\r\n`, `\t` などをCSV内で使用可能
- ✅ **多言語対応**: 日本語、英語、韓国語、中国語（簡体字・繁体字）のヘルプ表示
- ✅ **拡張ヘルプ**: `/h:cul` でカルチャー情報一覧、`/h:enc` でエンコーディング情報一覧を表示

### txprobe の機能

- ✅ **テキストファイル検索**: 指定した単語を含むファイルを検索
- ✅ **エンコーディング判定**: ファイルの文字エンコーディングを自動判定・表示
- ✅ **改行コード判定**: CRLF / LF / CR を判定・表示
- ✅ **BOM検出**: BOMの有無を検出・表示
- ✅ **プローブモード**: 検索単語を含むファイルのみ表示
- ✅ **多言語対応**: 日本語、英語、韓国語、中国語（簡体字・繁体字）のヘルプ表示
- ✅ **拡張ヘルプ**: `/h:cul` でカルチャー情報一覧、`/h:enc` でエンコーディング情報一覧を表示

---

## 🚀 インストール

### 必要環境

- **.NET Framework 4.8** 以上
- Windows OS

### ビルド済みバイナリ

リリースページから最新版をダウンロードしてください。

### ソースからビルド

```powershell
# リポジトリをクローン
git clone https://github.com/motoi-tsushima/rmsmf.git
cd rmsmf

# Visual Studioでソリューションを開く
start rmsmf.sln

# または、MSBuildでビルド
msbuild rmsmf.sln /p:Configuration=Release
```

---

## 📖 使用方法

### rmsmf - 文字列置換ツール

#### 基本構文

```
rmsmf <オプション> <対象ファイル名>
```

#### オプション一覧

| オプション | 説明 | 例 |
|----------|------|-----|
| `/r:<CSVファイル>` | 置換単語リストCSVのパス | `/r:words.csv` |
| `/c:<エンコーディング>` | 読み込みファイルのエンコーディング | `/c:shift_jis` |
| `/w:<エンコーディング>` | 書き込みファイルのエンコーディング | `/w:UTF-8` |
| `/b:<true\|false>` | BOMの有無 | `/b:true` |
| `/d` | サブディレクトリも検索対象 | `/d` |
| `/f:<ファイルリスト>` | 処理対象ファイルリスト | `/f:files.txt` |
| `/rc:<エンコーディング>` | 置換単語CSVのエンコーディング | `/rc:UTF-8` |
| `/fc:<エンコーディング>` | ファイルリストのエンコーディング | `/fc:UTF-8` |
| `/nl:<改行コード>` | 改行コード (crlf/lf/cr) | `/nl:lf` |
| `/h` | ヘルプ表示 | `/h` |
| `/h:cul` | 使用可能なカルチャー情報の一覧表示 | `/h:cul` |
| `/h:enc` | 使用可能なエンコーディング情報の一覧表示 | `/h:enc` |
| /det:<0\|1\|3> | 文字エンコーディング判定処理の指定 | /det:3 |
| /ci:<カルチャー情報> | カルチャー情報(国・言語識別コード) | /ci:en-US |

#### 置換単語リストCSVの書式

```csv
検索ワード1,置換ワード1
検索ワード2,置換ワード2
検索ワード3,置換ワード3
```

**エスケープシーケンス対応:**
- `\r\n` - CRLF（改行）
- `\n` - LF（改行）
- `\r` - CR（改行）
- `\t` - タブ
- `\\` - バックスラッシュ

#### 使用例

**1. 基本的な文字列置換**

```powershell
rmsmf /r:words.csv *.txt
```

**2. エンコーディング変換 + 文字列置換**

```powershell
# Shift_JIS → UTF-8 (BOM付き)
rmsmf /r:words.csv *.txt /c:shift_jis /w:UTF-8 /b:true

# UTF-8 → Shift_JIS
rmsmf /r:words.csv *.txt /c:UTF-8 /w:shift_jis

# Shift_JIS → UTF-8 (BOM付き)  :文字エンコーディング自動判定任せ
rmsmf /r:words.csv *.txt /w:UTF-8 /b:true

# UTF-8 → Shift_JIS  :文字エンコーディング自動判定任せ
rmsmf /r:words.csv *.txt /w:shift_jis
```

**3. エンコーディング変換のみ（置換なし）**

```powershell
# Shift_JIS → UTF-8 (BOM付き)
rmsmf *.txt /c:shift_jis /w:UTF-8 /b:true

# UTF-8 → Shift_JIS
rmsmf *.txt /c:UTF-8 /w:shift_jis

# Shift_JIS → UTF-8 (BOM付き)  :文字エンコーディング自動判定任せ
rmsmf *.txt /w:UTF-8 /b:true
```

**4. サブディレクトリも含めて処理**

```powershell
rmsmf /r:words.csv /d *.txt
```

**5. ファイルリストで対象を指定**

```powershell
rmsmf /r:words.csv /f:filelist.txt
```

**filelist.txt の例:**
```
C:\Projects\file1.txt
C:\Projects\file2.txt
C:\Projects\docs\readme.txt
```

**6. 改行コード変換**

```powershell
# CRLF → LF
rmsmf *.txt /c:UTF-8 /w:UTF-8 /nl:lf

# LF → CRLF
rmsmf *.txt /c:UTF-8 /w:UTF-8 /nl:crlf

# LF → CRLF  :文字エンコーディング自動判定任せ
rmsmf *.txt /nl:crlf
```

/c:  /w:  などの文字エンコーディング指定は省略可能です。
省略すると文字エンコーディング自動判定をしますが、自動判定は完璧ではありませんので、ご注意ください。
完璧な自動判定は有り得ません。

```powershell
# CRLF → LF
rmsmf *.txt /nl:lf
```

**7. ヘルプオプション**

```powershell
# 通常のヘルプを表示
rmsmf /h

# 使用可能なカルチャー情報の一覧を表示（/ci: オプションで使用可能な値）
rmsmf /h:cul

# 使用可能なエンコーディング情報の一覧を表示（/c: と /w: オプションで使用可能な値）
rmsmf /h:enc
```

---

### txprobe - テキストファイル検索ツール

#### 基本構文

```
txprobe <オプション> <対象ファイル名>
```

#### オプション一覧

| オプション | 説明 | 例 |
|----------|------|-----|
| `/s:<検索単語ファイル>` | 検索単語リストのパス | `/s:search.txt` |
| `/c:<エンコーディング>` | 読み込みファイルのエンコーディング | `/c:UTF-8` |
| `/d` | サブディレクトリも検索対象 | `/d` |
| `/f:<ファイルリスト>` | 検索対象ファイルリスト | `/f:files.txt` |
| `/sc:<エンコーディング>` | 検索単語ファイルのエンコーディング | `/sc:UTF-8` |
| `/fc:<エンコーディング>` | ファイルリストのエンコーディング | `/fc:UTF-8` |
| `/p` | プローブモード（検索単語を含むファイルのみ表示） | `/p` |
| `/h` | ヘルプ表示 | `/h` |
| `/h:cul` | 使用可能なカルチャー情報の一覧表示 | `/h:cul` |
| `/h:enc` | 使用可能なエンコーディング情報の一覧表示 | `/h:enc` |
| /det:<0\|1\|3> | 文字エンコーディング判定処理の指定 | /det:3 |
| /ci:<カルチャー情報> | カルチャー情報(国・言語識別コード) | /ci:en-US |

#### 検索単語リストの書式

```
検索ワード1
検索ワード2
検索ワード3
```

#### 使用例

**1. 基本的な検索**

```powershell
# カレントディレクトリの全txtファイルを解析
txprobe *.txt
```

**出力例:**
```
ファイル名: sample.txt
  エンコーディング: UTF-8 (BOM付き)
  改行コード: CRLF
```

**2. 検索単語を含むファイルを検索**

```powershell
txprobe /s:search.txt *.txt
```

**3. プローブモード（単語を含むファイルのみ表示）**

```powershell
txprobe /s:search.txt /p *.txt
```

**4. サブディレクトリも含めて検索**

```powershell
txprobe /d *.txt
```

**5. エンコーディング指定**

```powershell
txprobe /c:UTF-8 *.txt
```

**6. ヘルプオプション**

```powershell
# 通常のヘルプを表示
txprobe /h

# 使用可能なカルチャー情報の一覧を表示（/ci: オプションで使用可能な値）
txprobe /h:cul

# 使用可能なエンコーディング情報の一覧を表示（/c: オプションで使用可能な値）
txprobe /h:enc
```

---

## 🔨 ビルド方法

### Visual Studio でビルド

1. `rmsmf.sln` を Visual Studio 2019/2022 で開く
2. ソリューション構成を「Release」に変更
3. メニュー → 「ビルド」→「ソリューションのビルド」

### MSBuild でビルド

```powershell
# Developer Command Promptで実行
msbuild rmsmf.sln /p:Configuration=Release /p:Platform="Any CPU"
```

### 出力先

```
rmsmf/bin/Release/rmsmf.exe
txprobe/bin/Release/txprobe.exe
```

---

## 🧪 テスト

### 単体テスト

rmsmf.Tests プロジェクトには 33 の単体テストが含まれています。

#### Visual Studio でテストを実行

1. Visual Studio で `rmsmf.sln` を開く
2. メニュー → 「テスト」→「すべてのテストを実行」

#### コマンドラインでテストを実行

```powershell
# MSTest を使用
vstest.console.exe rmsmf.Tests\bin\Debug\rmsmf.Tests.dll
```

### 結合テスト

rmsmf と txprobe の連携動作を検証する結合テストスクリプトを用意しています。

#### 実行方法

```powershell
# Debug ビルドでテスト実行
.\integration-test.ps1

# Release ビルドでテスト実行
.\integration-test.ps1 -Configuration Release
```

#### テストシナリオ

結合テストでは以下のシナリオをカバーしています：

1. **txprobe 検索 → rmsmf 置換**
   - txprobe で特定の文字列を含むファイルを検索
   - 検索結果を元に rmsmf で文字列を置換
   - 置換結果の検証

2. **エンコーディング変換とBOM制御**
   - Shift-JIS ファイルを作成
   - txprobe でエンコーディングを確認
   - rmsmf で UTF-8 (BOM付き) に変換
   - BOM の有無を検証

3. **複数ファイルの一括処理**
   - 複数のテストファイルを作成
   - txprobe で検索
   - rmsmf で一括置換
   - すべてのファイルで置換が成功したことを検証

4. **CSV ファイルを使った複数文字列置換**
   - 設定ファイル (XML) を作成
   - CSV で複数の置換ルールを定義
   - rmsmf で一括置換
   - すべての設定値が正しく置換されたことを検証

5. **BOM の追加と削除**
   - BOM なしのファイルを作成
   - rmsmf で BOM を追加
   - rmsmf で BOM を削除
   - 各操作後に BOM の状態を検証

#### 結合テストの実装詳細

結合テストは以下の特徴を持っています：

- ✅ 実際の `.exe` ファイルを使用
- ✅ 実ファイルシステムで動作検証
- ✅ 複数のシナリオを自動実行
- ✅ 詳細なアサーションとエラーレポート
- ✅ テスト環境の自動セットアップとクリーンアップ

---

## 💻 開発環境

### 必須環境

- **OS**: Windows 10/11
- **IDE**: Visual Studio 2019/2022
- **.NET Framework**: 4.8 以上
- **C#**: 7.3

### 推奨ツール

- **Git**: バージョン管理
- **Visual Studio Code**: 軽量エディタとして
- **PowerShell**: コマンド実行

### NuGet パッケージ

テストプロジェクトのみ以下を使用：

- `MSTest.TestFramework` 2.2.10
- `MSTest.TestAdapter` 2.2.10

---

## 📝 ライセンス

このプロジェクトのライセンスについては、[LICENSE](LICENSE) ファイルを参照してください。

---

## 🤝 コントリビューション

プルリクエストを歓迎します！以下の点にご協力ください：

1. **フォーク**してブランチを作成
2. **テスト**を追加（新機能の場合）
3. **コミットメッセージ**は明確に
4. **プルリクエスト**を作成

### コーディング規約

- インデント: 4スペース
- 命名規則: Microsoftの C# コーディング規約に準拠
- コメント: 日本語可

---

## 参考資料

### 関連プロジェクト

- [iconv](https://www.gnu.org/software/libiconv/) - 文字エンコーディング変換ツール
- [nkf](https://osdn.net/projects/nkf/) - Network Kanji Filter

### ドキュメント

- [.NET Framework ドキュメント](https://docs.microsoft.com/ja-jp/dotnet/framework/)
- [文字エンコーディング一覧](https://docs.microsoft.com/ja-jp/dotnet/api/system.text.encoding)

---

## お問い合わせ

質問や提案がある場合は、[Issues](https://github.com/motoi-tsushima/rmsmf/issues) を作成してください。

---

## 更新履歴

### v1.0.0 (2026-01-17)

- 🚀 大規模リファクタリング完了
  - コンストラクタの分割（200行 → 25行）
  - 検証ロジックの整理
  - エラーメッセージの定数化
  - 共通ユーティリティクラスの追加
- 🧪 単体テスト追加（33テスト、全合格）
- 📚 ドキュメント整備

### v0.9.x

- 初期バージョン
- 基本的な文字列置換機能
- エンコーディング変換機能

---

<div align="center">

**Made with ❤️ by [motoi-tsushima](https://github.com/motoi-tsushima)**

</div>
