# rmsmf.Tests

rmsmf プロジェクトの単体テストプロジェクトです。

## セットアップ

### 1. Visual Studio でソリューションを開く

1. `rmsmf.sln` を Visual Studio で開きます
2. ソリューション エクスプローラーでソリューションを右クリック
3. 「追加」→「既存のプロジェクト」を選択
4. `rmsmf.Tests\rmsmf.Tests.csproj` を選択

### 2. NuGet パッケージの復元

1. ソリューション エクスプローラーで `rmsmf.Tests` プロジェクトを右クリック
2. 「NuGet パッケージの管理」を選択
3. 以下のパッケージをインストール：
   - `MSTest.TestFramework` (バージョン 2.2.10)
   - `MSTest.TestAdapter` (バージョン 2.2.10)

または、パッケージ マネージャー コンソールで：
```powershell
Install-Package MSTest.TestFramework -Version 2.2.10 -ProjectName rmsmf.Tests
Install-Package MSTest.TestAdapter -Version 2.2.10 -ProjectName rmsmf.Tests
```

### 3. プロジェクト参照の追加

`rmsmf.Tests` プロジェクトに `rmsmf` プロジェクトへの参照を追加します：

1. `rmsmf.Tests` プロジェクトを右クリック
2. 「追加」→「参照」を選択
3. 「プロジェクト」タブで `rmsmf` を選択
4. 「OK」をクリック

## テストの実行

### Visual Studio のテスト エクスプローラーを使用

1. メニューから「テスト」→「テスト エクスプローラー」を選択
2. 「すべて実行」をクリック

### コマンドラインから実行

```powershell
dotnet test rmsmf.Tests\rmsmf.Tests.csproj
```

または

```powershell
vstest.console.exe rmsmf.Tests\bin\Debug\rmsmf.Tests.dll
```

## テストクラス

### ColipexTests
コマンドライン引数の解析機能をテストします。

- オプションとパラメータの解析
- コロン（:）および等号（=）セパレータのサポート
- 重複オプションの検出
- 混在したパラメータとオプションの処理

**テストケース数**: 11

### OptionValidatorTests
共通検証ユーティリティの機能をテストします。

- ファイル指定方法の競合検証
- エンコーディングオプションの依存関係検証
- 必須パラメータの検証

**テストケース数**: 10

### CommandOptionsTests
CommandOptions クラスの検証ロジックをテストします。

- 必須パラメータの検証
- エンコーディングオプションの設定
- BOM オプションの設定
- 改行コードオプションの設定
- AllDirectories オプションの設定

**テストケース数**: 12

## カバレッジ

このテストスイートは以下のクラスをカバーしています：

- ? `Colipex` - コマンドライン解析基底クラス
- ? `OptionValidator` - 共通検証ユーティリティ
- ? `CommandOptions` - コマンドオプション解析と検証
- ?? `ValidationMessages` - 定数クラス（テスト不要）

## 今後の拡張

以下のテストの追加を検討してください：

1. **EncodingJudgmentTests** - エンコーディング判定機能
2. **ByteOrderMarkJudgmentTests** - BOM 判定機能
3. **統合テスト** - 実際のファイル操作を伴うテスト
4. **ReadReplaceWordsTests** - 置換単語リストの読み込み
5. **ReadFileNameListTests** - ファイルリストの読み込み

## トラブルシューティング

### テストが見つからない場合

1. ソリューションをリビルド
2. テスト エクスプローラーを更新
3. NuGet パッケージが正しくインストールされているか確認

### 参照エラーが発生する場合

1. `rmsmf` プロジェクトへの参照が正しく追加されているか確認
2. ターゲット フレームワークが一致しているか確認（両方とも .NET Framework 4.8）

## ライセンス

このテストプロジェクトは rmsmf プロジェクトと同じライセンスに従います。
