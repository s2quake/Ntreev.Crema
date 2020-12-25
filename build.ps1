# Released under the MIT License.

# Copyright (c) 2018 Ntreev Soft co., Ltd.
# Copyright (c) 2020 Jeesu Choi

# Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated
# documentation files (the "Software"), to deal in the Software without restriction, including without limitation the
# rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit
# persons to whom the Software is furnished to do so, subject to the following conditions:

# The above copyright notice and this permission notice shall be included in all copies or substantial portions of the
# Software.

# THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
# WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR
# COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR
# OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

# Forked from https://github.com/NtreevSoft/Crema
# Namespaces and files starting with "Ntreev" have been renamed to "JSSoft".

$location = Get-Location
try {
    Set-Location $PSScriptRoot
    $buildFile = "./.vscode/build.ps1"
    $outputPath = "bin"
    $propsPath = (
        "../JSSoft.Communication/Directory.Build.props",
        "../JSSoft.Library/Directory.Build.props",
        "../JSSoft.Library.Commands/Directory.Build.props",
        "../JSSoft.ModernUI.Framework/Directory.Build.props",
        "./Directory.Build.props"
    ) | ForEach-Object { "`"$_`"" }
    $propsPath = $propsPath -join ","
    $solutionPath = "./crema.sln"
    if (!(Test-Path $outputPath)) {
        New-Item $outputPath -ItemType Directory
    }
    Invoke-WebRequest -Uri "https://raw.githubusercontent.com/s2quake/build/master/build.ps1" -OutFile $buildFile
    Invoke-Expression "$buildFile $solutionPath $propsPath -Publish -Sign -OutputPath $outputPath $args"
}
finally {
    Remove-Item ./.vscode/build.ps1
    Set-Location $location
}
