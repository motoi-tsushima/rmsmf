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

## 🏗️ アーキテクチャ

### クラス構成図

```
┌─────────────────────────────────────────────────────┐
│                    Colipex                          │
│  (コマンドライン解析基底クラス)                        │
│                                                     │
│  + Options: Dictionary<string, string>              │
│  + Parameters: List<string>                         │
│  + IsOption(name): bool                             │
│  # ConvertEscapeSequences(input): string            │
│  # ResolveEncoding(name): Encoding                  │
│  # EnsureEncodingInitialized(...)                   │
│  # LoadLinesFromFile(...): List<string>             │
└─────────────────────────────────────────────────────┘
                        ▲
                        │
        ┌───────────────┴───────────────┐
        │                               │
┌───────┴────────┐            ┌─────────┴────────┐
│ CommandOptions │            │ CommandOptions   │
│    (rmsmf)     │            │   (txprobe)      │
└────────────────┘            └──────────────────┘
```

### 主要クラス

#### **Colipex (基底クラス)**

コマンドライン解析の基底機能を提供します。

**責務:**
- コマンドライン引数の解析
- オプションとパラメータの管理
- 共通ユーティリティメソッド（エスケープシーケンス変換、エンコーディング解決）

**主要メソッド:**
- `ConvertEscapeSequences(string)` - `\r\n`, `\t` などの変換
- `ResolveEncoding(string)` - エンコーディング名またはコードページからEncodingオブジェクトを取得
- `EnsureEncodingInitialized(...)` - エンコーディング自動判定と設定
- `LoadLinesFromFile(...)` - ファイル読み込みとエスケープシーケンス変換

---

#### **OptionValidator (ユーティリティクラス)**

コマンドオプションの検証ロジックを提供します。

**責務:**
- オプションの組み合わせ検証
- 依存関係のチェック
- 共通検証ロジックの提供

**主要メソッド:**
- `ValidateFileSpecificationNotConflicting(...)` - ファイル指定方法の競合チェック
- `ValidateEncodingOptionDependency(...)` - エンコーディングオプションの依存関係チェック
- `ValidateAtLeastOneCondition(...)` - 必須パラメータのチェック

---

#### **ValidationMessages (定数クラス)**

すべてのエラーメッセージを一元管理します。

**メリット:**
- メッセージの一貫性
- 変更が容易
- 多言語対応の準備

**定数例:**
```csharp
public const string MissingRequiredParameters = "必須パラメータが入力されていません。";
public const string ConflictingFileSpecificationMethods = "...";
public const string InvalidEncodingName = "エンコーディング名が不正です。";
```

---

#### **CommandOptions (rmsmf / txprobe)**

各ツール固有のコマンドオプション処理を担当します。

**rmsmf の主要メソッド:**
- `ReadReplaceWords()` - 置換単語CSVの読み込み
- `ReadFileNameList()` - ファイルリストの読み込み
- `ParseEncodingOptions(...)` - エンコーディングオプションの解析
- `ValidateOptionConsistency()` - オプション整合性の検証

**txprobe の主要メソッド:**
- `ReadSearchWords()` - 検索単語リストの読み込み
- `ReadFileNameList()` - ファイルリストの読み込み
- `ParseEncodingOptions(...)` - エンコーディングオプションの解析
- `ValidateOptionConsistency()` - オプション整合性の検証

---

### データフロー

#### rmsmf の処理フロー

```
┌──────────────────┐
│ コマンドライン引数 │
└────────┬─────────┘
         │
         ▼
┌────────────────────┐
│ Program.Main()      │
│ - ヘルプチェック     │
└────────┬───────────┘
         │
         ▼
┌──────────────────────┐
│ CommandOptions()      │
│ - 引数解析            │
│ - エンコーディング設定 │
│ - 検証                │
└────────┬─────────────┘
         │
         ▼
┌──────────────────────┐
│ ReadReplaceWords()    │
│ - CSV読み込み         │
│ - パース              │
└────────┬─────────────┘
         │
         ▼
┌──────────────────────┐
│ ReadFileNameList()    │
│ - ファイルリスト取得   │
└────────┬─────────────┘
         │
         ▼
┌──────────────────────┐
│ ReplaceStringsInFiles │
│ - ファイル置換処理     │
└──────────────────────┘
```

---

## 🔄 リファクタリング内容

このプロジェクトは、2026年1月に大規模なリファクタリングを実施しました。

### 実施した改善

| # | 改善項目 | 詳細 | 効果 |
|---|---------|------|------|
| 1 | スペルミス修正 | `repleaseEncoding` → `ReplaceEncoding` | 可読性向上 |
| 2 | フィールドのプロパティ化 | public フィールド → プロパティ | カプセル化 |
| 3 | エラーハンドリング統一 | `ExecutionState` → `RmsmfException` | 一貫性 |
| 4 | 共通メソッド抽出 | 重複コード削除 | DRY原則 |
| 5 | コンストラクタ分割 | 200行 → 25行 (10メソッド) | 可読性 |
| 6 | ヘルプチェック外部化 | 早期return削除 | 設計改善 |
| 7 | 検証ロジック整理 | 5個のメソッドに分離 | 単一責任 |
| 8 | エラーメッセージ定数化 | `ValidationMessages` クラス | 保守性 |
| 9 | 検証ユーティリティ | `OptionValidator` クラス | 再利用性 |
| 10 | 基底クラス整理 | `ConvertEscapeSequences` 統合 | DRY原則 |
| 11 | メソッド分割 | `ReadReplaceWords` など | 可読性 |
| 12 | **単体テスト追加** | **33テスト (全合格)** | **品質保証** |

### Before / After

#### Before: 長大なコンストラクタ（200行）

```csharp
public CommandOptions(string[] args) : base(args)
{
    // 200行以上のコード
    // - 検証
    // - エンコーディング設定
    // - オプション解析
    // - ファイル存在確認
    // すべてが1つのメソッドに...
}
```

#### After: 分割されたコンストラクタ（25行）

```csharp
public CommandOptions(string[] args) : base(args)
{
    ValidateRequiredParameters();
    
    ParseEncodingOptions(
        out string readCharacterSet,
        out string writeCharacterSet,
        out string replaceWordsCharacterSet,
        out string filesCharacterSet);
    
    this._enableBOM = ParseBomOption();
    this.searchOptionAllDirectories = ParseAllDirectoriesOption();
    this._writeNewLine = ParseNewLineOption();
    
    InitializeEncodings(
        readCharacterSet,
        writeCharacterSet,
        replaceWordsCharacterSet,
        filesCharacterSet);
    
    ValidateAndSetFileOptions();
    ValidateOptionConsistency();
}
```

### 定量的な改善

- **コード削減**: 約150行以上の重複コード削除
- **メソッド平均行数**: 100行超 → 20行以下
- **循環的複雑度**: 大幅に低減
- **テストカバレッジ**: 0% → 主要クラス高カバレッジ

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

### テストプロジェクト構成

```
rmsmf.Tests/
├── ColipexTests.cs          # 11テスト
├── OptionValidatorTests.cs  # 10テスト
└── CommandOptionsTests.cs   # 12テスト
```

**合計: 33テスト、全合格 ?**

### テスト実行方法

#### Visual Studio のテストエクスプローラー

1. メニュー → 「テスト」→「テスト エクスプローラー」
2. 「すべて実行」をクリック

#### コマンドラインから

```powershell
# MSTestを使用
vstest.console.exe rmsmf.Tests\bin\Debug\rmsmf.Tests.dll
```

### テスト内容

#### ColipexTests (11テスト)

- ? コマンドライン引数の解析
- ? オプションとパラメータの分離
- ? コロン・等号セパレータのサポート
- ? 重複オプションの検出
- ? 空の引数の処理

#### OptionValidatorTests (10テスト)

- ? ファイル指定方法の競合検証
- ? エンコーディングオプションの依存関係検証
- ? 必須パラメータの検証
- ? エラーメッセージの正確性

#### CommandOptionsTests (12テスト)

- ? エンコーディングオプションの設定
- ? BOMオプションの設定
- ? 改行コードオプションの設定
- ? 例外の適切なスロー

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
