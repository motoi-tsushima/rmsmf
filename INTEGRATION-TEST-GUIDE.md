# 結合テスト実行ガイド

このドキュメントでは、rmsmf と txprobe の結合テストの実行方法と詳細について説明します。

## 📋 前提条件

### 必須環境

- Windows 10/11
- PowerShell 5.1 以上
- .NET Framework 4.8 以上
- ビルド済みの rmsmf.exe と txprobe.exe

### ビルド

テストを実行する前に、プロジェクトをビルドしてください。

```powershell
# Debug ビルド
msbuild rmsmf.sln /p:Configuration=Debug

# Release ビルド
msbuild rmsmf.sln /p:Configuration=Release
```

## 🚀 テストの実行

### 基本的な実行方法

```powershell
# カレントディレクトリをプロジェクトルートに移動
cd C:\path\to\rmsmf

# Debug ビルドでテスト実行（デフォルト）
.\integration-test.ps1

# Release ビルドでテスト実行
.\integration-test.ps1 -Configuration Release
```

### カスタムパスの指定

exe ファイルのパスをカスタマイズする場合：

```powershell
.\integration-test.ps1 `
    -RmsmfPath "C:\custom\path\rmsmf.exe" `
    -TxprobePath "C:\custom\path\txprobe.exe"
```

## 📊 テストシナリオの詳細

### シナリオ1: txprobe 検索 → rmsmf 置換

**目的**: 2つのツールを連携させた基本的なワークフローをテストします。

**手順**:
1. C# ソースファイルを作成
2. txprobe で `oldName` という文字列を検索
3. rmsmf で `oldName` を `newName` に置換
4. 置換結果を検証

**検証項目**:
- ✅ txprobe が対象ファイルを正しく検出
- ✅ rmsmf が文字列を正しく置換
- ✅ 置換後のファイルに新しい文字列が含まれる
- ✅ 古い文字列が残っていない

### シナリオ2: エンコーディング変換とBOM制御

**目的**: 文字エンコーディングの検出と変換機能をテストします。

**手順**:
1. Shift-JIS エンコーディングのファイルを作成
2. txprobe でエンコーディングを確認
3. rmsmf で UTF-8 (BOM付き) に変換
4. BOM の有無を検証

**検証項目**:
- ✅ txprobe が Shift-JIS を正しく検出
- ✅ rmsmf がエンコーディングを正しく変換
- ✅ UTF-8 BOM が正しく追加されている
- ✅ 日本語テキストが文字化けしていない

### シナリオ3: 複数ファイルの一括処理

**目的**: ワイルドカードを使った複数ファイルの一括処理をテストします。

**手順**:
1. 3つのテキストファイルを作成
2. txprobe で `PLACEHOLDER` を含むファイルを検索
3. rmsmf で `PLACEHOLDER` を `ACTUAL_VALUE` に一括置換
4. すべてのファイルで置換を検証

**検証項目**:
- ✅ txprobe が3つのファイルすべてを検出
- ✅ rmsmf がすべてのファイルで置換を実行
- ✅ 各ファイルで正しく置換されている
- ✅ 元の文字列が残っていない

### シナリオ4: CSV ファイルを使った複数文字列置換

**目的**: CSV ファイルを使った複雑な置換ルールの適用をテストします。

**手順**:
1. XML 設定ファイルを作成（開発環境用の設定）
2. CSV で本番環境用の置換ルールを定義
   - サーバー名
   - データベース名
   - 環境名
   - 認証情報
3. rmsmf で CSV を使って一括置換
4. すべての設定値が更新されたことを検証

**検証項目**:
- ✅ txprobe が設定ファイルを検出
- ✅ rmsmf が CSV の全ルールを適用
- ✅ 5つの設定値すべてが正しく置換されている
- ✅ 開発環境の設定値が残っていない

### シナリオ5: BOM の追加と削除

**目的**: BOM (Byte Order Mark) の制御機能をテストします。

**手順**:
1. UTF-8 (BOM なし) のファイルを作成
2. rmsmf で BOM を追加
3. BOM の有無を検証
4. rmsmf で BOM を削除
5. BOM がないことを検証

**検証項目**:
- ✅ 初期状態で BOM がない
- ✅ rmsmf が BOM を正しく追加
- ✅ rmsmf が BOM を正しく削除
- ✅ BOM 操作後もテキストが保持されている

## 📈 テスト出力の見方

### 成功時の出力例

```
╔═══════════════════════════════════════════════════════════════════╗
║                                                                   ║
║   rmsmf & txprobe 結合テストスイート                              ║
║                                                                   ║
╚═══════════════════════════════════════════════════════════════════╝

======================================================================
  環境セットアップ
======================================================================
  → rmsmf.exe を確認: .\rmsmf\bin\Debug\rmsmf.exe
  → txprobe.exe を確認: .\txprobe\bin\Debug\txprobe.exe
  → テストディレクトリを作成: .\test-workspace
  ✓ 環境セットアップ完了

======================================================================
  シナリオ1: txprobe 検索 → rmsmf 置換
======================================================================
  → テストファイルを作成
  → txprobe で 'oldName' を検索
  ✓ txprobe で対象ファイルを検出
  → rmsmf で 'oldName' → 'newName' に置換
  ✓ 置換後のファイルに 'newName' が含まれている
  ✓ 置換後のファイルに 'oldName' が含まれていない

...（他のシナリオ）...

======================================================================
  テスト実行結果サマリー
======================================================================

  合計アサーション数: 23
  成功: 23
  失敗: 0
  成功率: 100%

  シナリオ別結果:
    ✓ シナリオ1: txprobe 検索 → rmsmf 置換
    ✓ シナリオ2: エンコーディング変換とBOM制御
    ✓ シナリオ3: 複数ファイルの一括処理
    ✓ シナリオ4: CSV ファイルを使った複数文字列置換
    ✓ シナリオ5: BOM の追加と削除

======================================================================
  すべてのテストが成功しました！ 🎉
======================================================================
```

### 失敗時の出力例

```
======================================================================
  シナリオ1: txprobe 検索 → rmsmf 置換
======================================================================
  → テストファイルを作成
  → txprobe で 'oldName' を検索
  ✗ シナリオが失敗しました
    詳細: txprobe の実行に失敗しました: ファイルが見つかりません

======================================================================
  テスト実行結果サマリー
======================================================================

  合計アサーション数: 0
  成功: 0
  失敗: 1
  成功率: 0%

  シナリオ別結果:
    ✗ シナリオ1: txprobe 検索 → rmsmf 置換
      エラー: txprobe の実行に失敗しました: ファイルが見つかりません

======================================================================
  一部のテストが失敗しました。
======================================================================
```

## 🔧 トラブルシューティング

### エラー: "rmsmf.exe が見つかりません"

**原因**: プロジェクトがビルドされていない、または exe のパスが正しくありません。

**解決策**:
```powershell
# プロジェクトをビルド
msbuild rmsmf.sln /p:Configuration=Debug

# または、カスタムパスを指定
.\integration-test.ps1 -RmsmfPath "C:\path\to\rmsmf.exe" -TxprobePath "C:\path\to\txprobe.exe"
```

### エラー: "実行ポリシーエラー"

**原因**: PowerShell の実行ポリシーでスクリプトの実行が制限されています。

**解決策**:
```powershell
# 実行ポリシーを一時的に変更
Set-ExecutionPolicy -ExecutionPolicy Bypass -Scope Process

# その後、テストを実行
.\integration-test.ps1
```

### 警告: "Shift-JIS の検出結果が期待と異なる"

**原因**: エンコーディング検出ロジックが環境により異なる結果を返す場合があります。

**影響**: この警告は通常、テストの成否に影響しません。エンコーディング変換自体は正しく動作しています。

## 🎯 CI/CD での使用

### GitHub Actions での例

```yaml
name: Integration Tests

on: [push, pull_request]

jobs:
  integration-test:
    runs-on: windows-latest
    
    steps:
    - uses: actions/checkout@v3
    
    - name: Setup MSBuild
      uses: microsoft/setup-msbuild@v1
    
    - name: Restore NuGet packages
      run: nuget restore rmsmf.sln
    
    - name: Build solution
      run: msbuild rmsmf.sln /p:Configuration=Release
    
    - name: Run integration tests
      run: .\integration-test.ps1 -Configuration Release
      shell: powershell
```

## 📝 テストの拡張

新しいシナリオを追加するには、`integration-test.ps1` を編集します：

```powershell
function Test-ScenarioX-YourNewScenario {
    Write-TestStep "新しいテストの説明"
    
    # テストの実装
    # ...
    
    # アサーション
    Assert-FileContains -FilePath $testFile -ExpectedContent "期待する文字列" `
        -Message "検証メッセージ"
}

# メイン実行部分に追加
Invoke-TestScenario -Name "シナリオX: 新しいテスト" `
    -Test { Test-ScenarioX-YourNewScenario }
```

## 📞 サポート

テストに関する質問や問題がある場合は、以下を参照してください：

- [README.md](README.md) - プロジェクトの概要
- [GitHub Issues](https://github.com/motoi-tsushima/rmsmf/issues) - バグ報告や機能リクエスト

---

最終更新: 2024
