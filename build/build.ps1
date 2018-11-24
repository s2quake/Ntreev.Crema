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
    if (Test-Path "C:\Program Files (x86)\Microsoft Visual Studio\2017\BuildTools\MSBuild\15.0\Bin\MSBuild.exe") {    
        $msbuildPath = "C:\Program Files (x86)\Microsoft Visual Studio\2017\BuildTools\MSBuild\15.0\Bin\MSBuild.exe"
    }
}

if ($msbuildPath -eq "") {
    Write-Error "cannot not build"
    return 1
}

Write-Host "Delete Release"
foreach ($item in $items) {
    $path = Join-Path "..\bin\Release" $item
    if (Test-Path "$path") {
        Remove-Item "$path" -Recurse
    }
}

if (Test-Path "$deploymentPath") {
    Remove-Item "$deploymentPath" -Recurse
}

Write-Host "Restore"
Invoke-Expression "&`"$msbuildPath`" `"..\crema.sln`" -t:restore -v:q" 

if (-Not $LASTEXITCODE -eq 0) {
    Write-Error "Restore failed." -ErrorAction Stop
}

Write-Host "Build"
Invoke-Expression "&`"$msbuildPath`" `"..\crema.sln`" -t:rebuild -v:q -p:Configuration=Release"

if (-Not $LASTEXITCODE -eq 0) {
    Write-Error "Build failed." -ErrorAction Stop
}

Write-Host "Copy"
foreach ($item in $items) {
    $srcPath = Join-Path "..\bin\Release" $item
    $destPath = Join-Path $deploymentPath $item
    Copy-Item "$srcPath" "$destPath" -Recurse -Exclude "*.pdb","*.xml"
}
