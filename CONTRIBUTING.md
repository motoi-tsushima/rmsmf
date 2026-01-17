# コントリビューションガイド

rmsmf プロジェクトへのコントリビューションに興味を持っていただき、ありがとうございます！

## ?? コントリビューションの方法

### 1. Issue の作成

バグ報告や機能要望は、[Issues](https://github.com/motoi-tsushima/rmsmf/issues) で受け付けています。

**バグ報告の際は以下を含めてください:**
- 問題の説明
- 再現手順
- 期待される動作
- 実際の動作
- 環境情報（OS、.NET Framework バージョンなど）

**機能要望の際は以下を含めてください:**
- 提案する機能の説明
- ユースケース
- 期待される利点

### 2. プルリクエストの作成

#### ステップ1: フォークとクローン

```bash
# リポジトリをフォーク（GitHubのWebUIで）

# フォークしたリポジトリをクローン
git clone https://github.com/YOUR_USERNAME/rmsmf.git
cd rmsmf

# upstream リモートを追加
git remote add upstream https://github.com/motoi-tsushima/rmsmf.git
```

#### ステップ2: ブランチを作成

```bash
# 最新の master を取得
git checkout master
git pull upstream master

# 新しいブランチを作成
git checkout -b feature/your-feature-name
# または
git checkout -b fix/your-bug-fix
```

**ブランチ命名規則:**
- `feature/` - 新機能
- `fix/` - バグ修正
- `docs/` - ドキュメント更新
- `refactor/` - リファクタリング
- `test/` - テスト追加

#### ステップ3: 変更を加える

1. **コードを変更**
2. **テストを追加** (新機能の場合)
3. **既存のテストを確認** (すべて合格すること)

```powershell
# テストの実行
# Visual Studio のテストエクスプローラーで実行
# または
vstest.console.exe rmsmf.Tests\bin\Debug\rmsmf.Tests.dll
```

#### ステップ4: コミット

```bash
# 変更をステージング
git add .

# コミット（明確なメッセージで）
git commit -m "feat: Add new feature XYZ

- Add functionality to do ABC
- Update documentation
- Add unit tests
"
```

**コミットメッセージの規則:**

```
<type>: <subject>

<body>
```

**Type:**
- `feat`: 新機能
- `fix`: バグ修正
- `docs`: ドキュメント変更
- `style`: コードスタイル変更（機能に影響なし）
- `refactor`: リファクタリング
- `test`: テスト追加・修正
- `chore`: ビルドプロセスやツールの変更

**例:**
```
feat: Add support for UTF-16 encoding

- Add UTF-16LE and UTF-16BE encoding options
- Update help message
- Add unit tests for UTF-16 handling

Closes #42
```

#### ステップ5: プッシュとPR作成

```bash
# フォークにプッシュ
git push origin feature/your-feature-name
```

GitHub の Web UI でプルリクエストを作成してください。

**PRの説明に含めるべき内容:**
- 変更の概要
- 関連する Issue 番号（`Closes #123` など）
- テスト結果のスクリーンショット（該当する場合）
- 破壊的変更がある場合はその説明

---

## ?? コーディング規約

### C# コーディングスタイル

#### 命名規則

```csharp
// クラス、メソッド、プロパティ: PascalCase
public class CommandOptions { }
public void ParseOptions() { }
public string FileName { get; set; }

// private フィールド: _camelCase
private string _fileName;
private bool _enableBOM;

// ローカル変数、パラメータ: camelCase
string fileName = "test.txt";
public void Method(string paramName) { }

// 定数: PascalCase
public const string DefaultEncoding = "UTF-8";
```

#### インデントと改行

- **インデント**: 4スペース
- **中括弧**: 新しい行で開始（Allman スタイル）

```csharp
// 正しい
if (condition)
{
    DoSomething();
}

// 間違い
if (condition) {
    DoSomething();
}
```

#### コメント

- **XMLコメント**: public メンバーには必須

```csharp
/// <summary>
/// ファイルから行を読み込み、エスケープシーケンスを変換する
/// </summary>
/// <param name="fileName">読み込むファイル名</param>
/// <param name="encoding">使用するエンコーディング</param>
/// <returns>読み込んだ行のリスト</returns>
protected List<string> LoadLinesFromFile(string fileName, Encoding encoding)
{
    // 実装
}
```

- **インラインコメント**: 日本語・英語どちらでも可

```csharp
// エンコーディングの判定と設定
EnsureEncodingInitialized(...);
```

#### メソッド長

- **推奨**: 20-30行以内
- **最大**: 50行（超える場合は分割を検討）

#### クラス設計

- **単一責任の原則**: 1つのクラスは1つの責務
- **DRY原則**: コードの重複を避ける
- **適切なカプセル化**: private/protected を活用

---

## ?? テストガイドライン

### テストの追加

新機能を追加する場合は、必ず単体テストも追加してください。

```csharp
[TestClass]
public class YourNewFeatureTests
{
    [TestMethod]
    public void Method_WithValidInput_ReturnsExpectedResult()
    {
        // Arrange - テストデータの準備
        var input = "test";
        var expected = "expected";

        // Act - テスト対象の実行
        var actual = MethodUnderTest(input);

        // Assert - 結果の検証
        Assert.AreEqual(expected, actual);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void Method_WithInvalidInput_ThrowsException()
    {
        // Arrange
        var invalidInput = "";

        // Act
        MethodUnderTest(invalidInput);

        // Assert - ExpectedException により例外が期待される
    }
}
```

### テストの命名規則

```
<MethodName>_<Scenario>_<ExpectedResult>
```

**例:**
- `Constructor_WithValidArguments_CreatesInstance`
- `Parse_WithNullInput_ThrowsArgumentNullException`
- `Validate_WithMissingOption_ReturnsFalse`

### テストカバレッジ

- **目標**: 主要クラスの70%以上
- **必須**: public メソッドはすべてテスト
- **推奨**: edge cases（境界値）もテスト

---

## ?? レビュープロセス

### プルリクエストのレビュー基準

1. **機能性**: 期待通りに動作するか
2. **テスト**: 適切なテストが含まれているか
3. **コードスタイル**: コーディング規約に従っているか
4. **ドキュメント**: 必要に応じてドキュメントが更新されているか
5. **破壊的変更**: 既存機能に影響がないか

### レビュー時間

- 通常、1-3営業日以内にレビューします
- 緊急の場合は Issue または PR でメンションしてください

---

## ?? バグ修正のガイドライン

### バグ修正のステップ

1. **Issue を作成**して問題を報告
2. **再現テストを作成**（失敗するテスト）
3. **バグを修正**
4. **テストが合格することを確認**
5. **PR を作成**

### バグ修正のPR

- タイトルに `fix:` を含める
- Issue 番号を参照（`Closes #123`）
- 修正前後の動作を説明

---

## ?? ドキュメントの貢献

ドキュメントの改善も歓迎します！

- タイポの修正
- 説明の明確化
- 使用例の追加
- 翻訳（英語版など）

ドキュメントのみの変更の場合、テストは不要です。

---

## ?? 行動規範

### 基本原則

- **尊重**: すべての貢献者を尊重する
- **建設的**: 批判は建設的に
- **協力的**: 助け合いの精神で
- **包括的**: 多様性を尊重する

### 禁止事項

- 差別的な言動
- ハラスメント
- スパム
- 悪意のある行動

違反者は、プロジェクトから除外される可能性があります。

---

## ?? 質問やヘルプ

質問がある場合は、以下の方法で問い合わせてください：

1. **Issue を作成**（`question` ラベル）
2. **Discussions を使用**（有効な場合）
3. **PRにコメント**

---

## ?? 貢献者への感謝

すべての貢献者に感謝します！

貢献者リストは [Contributors](https://github.com/motoi-tsushima/rmsmf/graphs/contributors) で確認できます。

---

ご協力ありがとうございます！??
