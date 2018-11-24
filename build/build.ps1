$items = @("crema", "cremaconsole", "cremaserver", "cremaserverApp", "cremadev", "cremadevApp")
$msbuildPath = ""
$deploymentPath = ".\Release"

foreach ($item in Invoke-Expression ".\vswhere.exe") {
    if ($item -match "^installationPath: (.+)") {
        $msbuildPath = Join-Path $Matches[1] "\MSBuild\15.0\Bin\MSBuild.exe"
        break;
    }
}

if ($msbuildPath -eq "") {
    Write-Error "cannot not build"
    return 1
}

Write-Host "delete release"
foreach ($item in $items) {
    $path = Join-Path "..\bin\Release" $item
    if (Test-Path "$path") {
        Remove-Item "$path" -Recurse
    }
}

if (Test-Path "$deploymentPath") {
    Remove-Item "$deploymentPath" -Recurse
}

Invoke-Expression "&`"$msbuildPath`" `"..\crema.sln`" -t:restore"

if (-Not $LASTEXITCODE -eq 0) {
    Write-Error "Restore failed." -ErrorAction Stop
}

Invoke-Expression "&`"$msbuildPath`" `"..\crema.sln`" -t:rebuild -v:q -p:Configuration=Release"

if (-Not $LASTEXITCODE -eq 0) {
    Write-Error "Build failed." -ErrorAction Stop
}


foreach ($item in $items) {
    $srcPath = Join-Path "..\bin\Release" $item
    $destPath = Join-Path $deploymentPath $item
    Copy-Item "$srcPath" "$destPath" -Recurse -Exclude "*.pdb","*.xml"
}
