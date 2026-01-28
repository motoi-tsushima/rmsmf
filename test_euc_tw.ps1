# EUC-TW (51950) サポート確認テスト
Add-Type -Path "C:\Users\motoi\OneDrive\bin\System.Text.Encoding.CodePages.dll"
Add-Type -Path "C:\Users\motoi\OneDrive\bin\System.Runtime.CompilerServices.Unsafe.dll"

[System.Text.Encoding]::RegisterProvider([System.Text.CodePagesEncodingProvider]::Instance)

try {
    $encoding = [System.Text.Encoding]::GetEncoding(51950)
    Write-Host "成功: EUC-TW (51950) がサポートされています" -ForegroundColor Green
    Write-Host "エンコーディング名: $($encoding.EncodingName)"
    Write-Host "コードページ: $($encoding.CodePage)"
} catch {
    Write-Host "失敗: EUC-TW (51950) はサポートされていません" -ForegroundColor Red
    Write-Host "エラー: $($_.Exception.Message)"
}
