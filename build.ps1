$items = @("crema", "cremaconsole", "cremaserver", "cremaserverApp", "cremadev", "cremadevApp")
$msbuildPath = ""
$releasePath = "$PSScriptRoot\bin\Release"
$deploymentPath = "$PSScriptRoot\build"
$solutionPath = "$PSScriptRoot\crema.sln"

$msbuildPath = Invoke-Expression "$PSScriptRoot\vswhere.exe -latest -requires Microsoft.Component.MSBuild -find MSBuild\**\Bin\MSBuild.exe"

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
    $path = Join-Path "$releasePath" $item
    Write-Host $path
    if (Test-Path "$path") {
        Remove-Item "$path" -Recurse -ErrorAction Stop
    }
}

if (Test-Path "$deploymentPath") {
    Remove-Item "$deploymentPath" -Recurse -ErrorAction Stop
}

Write-Host "Restore"
Invoke-Expression "&'$msbuildPath' '$solutionPath' -t:restore -v:q -p:configuration=Release" 

if (-Not $LASTEXITCODE -eq 0) {
    Write-Error "Restore failed." -ErrorAction Stop
}

Write-Host "Build"
Invoke-Expression "&'$msbuildPath' '$solutionPath' -t:build -v:q -p:configuration=Release"

if (-Not $LASTEXITCODE -eq 0) {
    Write-Error "Build failed." -ErrorAction Stop
}

Write-Host "Copy"
foreach ($item in $items) {
    $srcPath = Join-Path "$releasePath" $item
    $destPath = Join-Path $deploymentPath $item
    Copy-Item "$srcPath" "$destPath" -Recurse -Exclude "*.pdb","*.xml"
}
