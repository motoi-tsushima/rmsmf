# NuGetパッケージを手動でダウンロードして展開するスクリプト

$packagesDir = ".\packages"
$packages = @(
    @{Id="System.Runtime.CompilerServices.Unsafe"; Version="4.7.1"},
    @{Id="System.Text.Encoding.CodePages"; Version="4.7.1"}
)

foreach ($pkg in $packages) {
    $pkgId = $pkg.Id
    $pkgVer = $pkg.Version
    $pkgDir = "$packagesDir\$pkgId.$pkgVer"
    
    if (Test-Path $pkgDir) {
        Write-Host "Already exists: $pkgId.$pkgVer" -ForegroundColor Green
        continue
    }
    
    $nupkgUrl = "https://www.nuget.org/api/v2/package/$pkgId/$pkgVer"
    $nupkgFile = "$env:TEMP\$pkgId.$pkgVer.nupkg"
    
    Write-Host "Downloading: $pkgId $pkgVer..." -ForegroundColor Yellow
    Invoke-WebRequest -Uri $nupkgUrl -OutFile $nupkgFile
    
    Write-Host "Extracting to: $pkgDir" -ForegroundColor Yellow
    Expand-Archive -Path $nupkgFile -DestinationPath $pkgDir -Force
    
    Remove-Item $nupkgFile
    Write-Host "Installed: $pkgId.$pkgVer" -ForegroundColor Green
}

Write-Host "`nAll packages installed!" -ForegroundColor Cyan
