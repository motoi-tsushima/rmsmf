# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### 今後の予定
- CI/CD パイプラインの構築（GitHub Actions）
- パフォーマンス最適化
- 追加のエンコーディング対応（UTF-16 など）

## [1.0.0] - 2026-01-17

### ?? メジャーリリース - 大規模リファクタリング完了

#### Added（追加）

**?? テスト**
- 単体テストプロジェクト追加 (`rmsmf.Tests`)
  - ColipexTests: 11テスト
  - OptionValidatorTests: 10テスト
  - CommandOptionsTests: 12テスト
  - **合計33テスト、全合格**

**??? 新クラス**
- `OptionValidator`: 共通検証ユーティリティクラス
  - `ValidateFileSpecificationNotConflicting()`
  - `ValidateEncodingOptionDependency()`
  - `ValidateAtLeastOneCondition()`
- `ValidationMessages`: エラーメッセージ定数クラス
  - すべてのエラーメッセージを一元管理

**?? 新メソッド（Colipex 基底クラス）**
- `ConvertEscapeSequences(string)`: エスケープシーケンス変換（基底クラスに統合）
- `ResolveEncoding(string)`: エンコーディング名解決
- `EnsureEncodingInitialized()`: エンコーディング自動判定
- `LoadLinesFromFile()`: ファイル読み込みユーティリティ

**?? ドキュメント**
- `README.md`: 包括的なプロジェクトドキュメント
- `CONTRIBUTING.md`: コントリビューションガイド
- `rmsmf.Tests\README.md`: テストプロジェクトのドキュメント

**?? txprobe ツール**
- テキストファイルの文字エンコーディングと改行コードを確認する機能
- 検索単語リストからファイル内の文字列を検索する機能
- プローブモード（検索単語が見つかったファイルのみ表示）
- サブディレクトリ検索オプション (/d)

#### Changed（変更）

**?? リファクタリング**

1. **コンストラクタの分割**
   - `CommandOptions` コンストラクタ: 200行 → 25行
   - 10個の小さなメソッドに分割

2. **検証ロジックの整理**
   - 5個の検証メソッドに分離
   - 責務の明確化

3. **ファイル読み込みメソッドの分割**
   - `ReadReplaceWords()`: 70行 → 25行
   - `ReadSearchWords()` (txprobe): 60行 → 25行
   - それぞれ3つのメソッドに分割

4. **エラーハンドリングの統一**
   - `ExecutionState` クラス削除
   - `RmsmfException` に統一
   - エラーメッセージを `ValidationMessages` に集約

5. **プロパティ化**
   - public フィールドを適切なプロパティに変更
   - カプセル化の強化

6. **txprobe の独立化**
   - rmsmf.exe への依存を解消
   - 完全に独立した実行ファイルに変更

#### Fixed（修正）

- スペルミスの修正
  - `repleaseEncoding` → `ReplaceEncoding`
  - その他、変数名の正規化
- 早期return問題の解決
  - コンストラクタから早期returnを削除
  - `CallHelp` プロパティ削除
  - ヘルプチェックを `Program.cs` に移動
- 不要なcatch句の削除
  - `DirectoryNotFoundException`
  - `ArgumentOutOfRangeException`
- txprobe: PATH環境変数のフォルダーに配置しても動作しなかった問題を修正
- 入力検証の強化
  - 置換単語CSVのフォーマット検証
  - ファイルリストの検証
  - エンコーディング名の検証

#### Removed（削除）

- `ExecutionState` クラス（`RmsmfException` に置き換え）
- `CallHelp` プロパティ（不要になった）
- コンストラクタ内の早期return
- 重複する `ConvertEscapeSequences()` メソッド（基底クラスに統合）

#### Performance（パフォーマンス）

- コード削減: 約150行以上の重複コード削除
- メソッド平均行数: 100行超 → 20行以下
- 循環的複雑度の大幅な低減

## [0.9.7] - 2024-XX-XX

### Added
- rmsmf: 複数ファイルの文字列置換機能
- rmsmf: 文字エンコーディング変換機能
- rmsmf: BOM制御機能

### Changed
- rmsmf: パフォーマンス最適化

### Fixed
- rmsmf: 一部のエンコーディング判定の不具合を修正

---

## リリースノートの書き方

### バージョン番号の決め方（Semantic Versioning）

- **MAJOR** (1.x.x): 互換性のない大きな変更
- **MINOR** (x.1.x): 後方互換性のある機能追加
- **PATCH** (x.x.1): 後方互換性のあるバグ修正

### カテゴリ

- **Added**: 新機能
- **Changed**: 既存機能の変更
- **Deprecated**: 非推奨になった機能
- **Removed**: 削除された機能
- **Fixed**: バグ修正
- **Security**: セキュリティ修正

[Unreleased]: https://github.com/motoi-tsushima/rmsmf/compare/v1.0.0...HEAD
[1.0.0]: https://github.com/motoi-tsushima/rmsmf/releases/tag/v1.0.0
[0.9.7]: https://github.com/motoi-tsushima/rmsmf/releases/tag/v0.9.7
