# rmsmf Suite - 模範的なリリースフロー

## 🎯 推奨するリリース戦略

### レベル別の推奨方法

#### レベル1: 基本（現在推奨）✅
- **配布方法**: GitHub Releases で ZIP配布
- **パッケージ構成**: 統合スイート（rmsmf + txprobe）
- **適用場面**: 個人プロジェクト、小規模配布

#### レベル2: 標準
- **配布方法**: GitHub Releases + 自動化
- **追加要素**: GitHub Actions、CHANGELOG
- **適用場面**: OSS プロジェクト、中規模配布

#### レベル3: プロフェッショナル
- **配布方法**: 上記 + パッケージマネージャー
- **追加要素**: Chocolatey、WinGet
- **適用場面**: 広く一般に公開するツール

---

## 📋 現在のプロジェクトに最適なリリースフロー

### 準備フェーズ

1. **バージョン番号の決定**
   ```
   Semantic Versioning: MAJOR.MINOR.PATCH
   例: 1.0.0 (初回正式リリース)
   ```

2. **AssemblyInfo の更新**
   - `rmsmf/Properties/AssemblyInfo.cs`
   - `txprobe/Properties/AssemblyInfo.cs`

3. **CHANGELOG.md の更新**
   ```markdown
   ## [1.0.0] - 2025-01-15
   ### Added
   - txprobe: 新機能
   ### Changed
   - 変更内容
   ```

### ビルドフェーズ

4. **Releaseビルドの実行**
   ```
   Visual Studio:
   - ソリューション構成: Release
   - ビルド → ソリューションのリビルド
   ```

5. **動作確認**
   ```powershell
   # 各ツールの動作確認
   .\rmsmf\bin\Release\rmsmf.exe /h
   .\txprobe\bin\Release\txprobe.exe /h
   ```

### パッケージングフェーズ

6. **リリースパッケージの作成**
   ```powershell
   .\create-suite-release.ps1 -Version "1.0.0"
   ```
   
   生成物:
   ```
   rmsmf-suite-v1.0.0/
   ├── bin/
   │   ├── rmsmf.exe
   │   └── txprobe.exe
   ├── README.md
   ├── LICENSE.txt
   ├── CHANGELOG.md
   └── INSTALL.txt
   
   rmsmf-suite-v1.0.0.zip
   ```

7. **ローカルテスト**
   ```powershell
   # ZIPを展開
   Expand-Archive rmsmf-suite-v1.0.0.zip -DestinationPath C:\temp\test
   
   # PATHに追加してテスト
   $env:PATH += ";C:\temp\test\rmsmf-suite-v1.0.0\bin"
   rmsmf /h
   txprobe /h
   ```

### 公開フェーズ

8. **Git タグの作成**
   ```bash
   git add .
   git commit -m "Release v1.0.0"
   git tag -a v1.0.0 -m "rmsmf Suite v1.0.0"
   git push origin master
   git push origin v1.0.0
   ```

9. **GitHub Releasesの作成**
   - https://github.com/motoi-tsushima/rmsmf/releases/new
   - タグ: v1.0.0
   - タイトル: rmsmf Suite v1.0.0
   - 説明文（テンプレート）:
   
   ```markdown
   # rmsmf Suite v1.0.0
   
   ## 📦 含まれるツール
   
   - **rmsmf.exe** (v0.9.7) - テキストファイルの文字エンコーディング変換・置換ツール
   - **txprobe.exe** (v0.2.0) - テキストファイル探索ツール
   
   ## 🚀 インストール方法
   
   1. `rmsmf-suite-v1.0.0.zip` をダウンロード
   2. 任意のフォルダーに展開
   3. `bin` フォルダーを PATH 環境変数に追加
   
   詳しい手順は同梱の `INSTALL.txt` を参照してください。
   
   ## ✨ 新機能
   
   - txprobe: テキストファイルの文字エンコーディング・改行コード確認機能
   - txprobe: ファイル内文字列検索機能
   - txprobe: プローブモード
   
   ## 🔧 変更内容
   
   - txprobe: rmsmf.exe への依存を解消
   - 両ツールを統合スイートとしてパッケージ化
   
   詳細は [CHANGELOG.md](CHANGELOG.md) を参照してください。
   
   ## 📄 必要な環境
   
   - Windows 7 以降
   - .NET Framework 4.8 以降
   
   ## 📖 使い方
   
   ### rmsmf - 文字エンコーディング変換・置換
   
   \`\`\`powershell
   # UTF-8に変換
   rmsmf /w:utf-8 /b:true *.txt
   
   # 文字列置換
   rmsmf /r:words.csv *.txt
   \`\`\`
   
   ### txprobe - ファイル探索
   
   \`\`\`powershell
   # 文字エンコーディング確認
   txprobe *.txt
   
   # 文字列検索
   txprobe /s:search.txt *.txt
   \`\`\`
   
   詳しくは `rmsmf /h` または `txprobe /h` でヘルプを表示してください。
   ```
   
   - ファイル添付: `rmsmf-suite-v1.0.0.zip`

### 告知フェーズ（オプション）

10. **リリースの告知**
    - GitHub のリリースページで公開
    - SNSで告知（Twitter, Qiitaなど）
    - ブログ記事の投稿

---

## 🔄 継続的な改善

### 次のリリースに向けて

1. **フィードバックの収集**
   - GitHub Issues でバグ報告・機能要望を受付
   
2. **定期的なメンテナンス**
   - セキュリティアップデート
   - 依存関係の更新
   - ドキュメントの改善

3. **バージョンアップ計画**
   - パッチリリース (1.0.1): バグ修正
   - マイナーリリース (1.1.0): 新機能追加
   - メジャーリリース (2.0.0): 大きな変更

---

## 📊 リリースチェックリスト

リリース前に確認すべき項目:

### コード
- [ ] すべてのテストが通過
- [ ] ビルドエラーなし
- [ ] 警告の確認と対処

### ドキュメント
- [ ] README.md の更新
- [ ] CHANGELOG.md の更新
- [ ] バージョン番号の更新（AssemblyInfo）
- [ ] INSTALL.txt の内容確認

### パッケージ
- [ ] Releaseビルドの実行
- [ ] リリースパッケージの作成
- [ ] ZIPファイルの内容確認
- [ ] ローカルでの動作確認

### 公開
- [ ] Gitタグの作成
- [ ] GitHub Releasesの作成
- [ ] リリースノートの記載
- [ ] ZIPファイルのアップロード

### 事後確認
- [ ] ダウンロードリンクの動作確認
- [ ] インストール手順の確認
- [ ] ドキュメントリンクの確認

---

## 🛠️ トラブルシューティング

### よくある問題

**Q: GitHub Actions でビルドが失敗する**
- A: .NET Framework 4.8 のセットアップを確認
- A: NuGetパッケージの復元を確認

**Q: ZIPファイルが大きすぎる**
- A: 不要なPDBファイルを除外
- A: ドキュメントを外部リンクに変更

**Q: ユーザーから「動かない」と報告**
- A: .NET Framework 4.8のインストール確認
- A: PATH設定の確認
- A: エラーメッセージの詳細を確認

---

## 📚 参考資料

- [Semantic Versioning](https://semver.org/)
- [Keep a Changelog](https://keepachangelog.com/)
- [GitHub Releases](https://docs.github.com/ja/repositories/releasing-projects-on-github)
- [.NET アプリケーションのバージョン管理](https://docs.microsoft.com/ja-jp/dotnet/standard/library-guidance/versioning)
