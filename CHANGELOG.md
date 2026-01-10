# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added
- txprobe: 新しいテキストファイル探索ツールを追加

## [1.0.0] - 2025-01-XX

### Added
- txprobe: テキストファイルの文字エンコーディングと改行コードを確認する機能
- txprobe: 検索単語リストからファイル内の文字列を検索する機能
- txprobe: プローブモード（検索単語が見つかったファイルのみ表示）
- txprobe: サブディレクトリ検索オプション (/d)

### Changed
- txprobe: rmsmf.exe への依存を解消し、完全に独立した実行ファイルに変更
- プロジェクト構成: rmsmf と txprobe を統合スイートとしてリリース

### Fixed
- txprobe: PATH環境変数のフォルダーに配置しても動作しなかった問題を修正

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
