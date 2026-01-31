# rmsmf & txprobe 結合テスト - クイックスタート

## 🚀 3ステップで始める

### 1. プロジェクトをビルド

```powershell
# リポジトリのルートディレクトリで実行
msbuild rmsmf.sln /p:Configuration=Debug
```

### 2. テストを実行

```powershell
# 結合テストを実行
.\integration-test.ps1
```

### 3. 結果を確認

テストが成功すると、以下のようなサマリーが表示されます：

```
======================================================================
  すべてのテストが成功しました！ 🎉
======================================================================
```

## 📚 詳細情報

- **詳細なガイド**: [INTEGRATION-TEST-GUIDE.md](INTEGRATION-TEST-GUIDE.md)
- **プロジェクトドキュメント**: [README.md](README.md)

## 🎯 実行オプション

```powershell
# Release ビルドでテスト
.\integration-test.ps1 -Configuration Release

# カスタムパスを指定
.\integration-test.ps1 -RmsmfPath "C:\path\to\rmsmf.exe" -TxprobePath "C:\path\to\txprobe.exe"
```

## 📊 テストシナリオ

1. ✅ txprobe 検索 → rmsmf 置換
2. ✅ エンコーディング変換とBOM制御
3. ✅ 複数ファイルの一括処理
4. ✅ CSV ファイルを使った複数文字列置換
5. ✅ BOM の追加と削除

---

**次のステップ**: [詳細ガイドを読む](INTEGRATION-TEST-GUIDE.md)
