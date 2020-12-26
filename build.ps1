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

param(
    [string]$OutputPath = "",
    [string]$Framework = "netcoreapp3.1",
    [string]$KeyPath = "",
    [string]$LogPath = "",
    [switch]$Force
)

$solutionPath = "./crema.sln"
$propPaths = (
    "../JSSoft.Communication/Directory.Build.props",
    "../JSSoft.Library/Directory.Build.props",
    "../JSSoft.Library.Commands/Directory.Build.props",
    "../JSSoft.ModernUI.Framework/Directory.Build.props",
    "./Directory.Build.props"
)

try {
    $buildFile = "./build-temp.ps1"
    $solutionPath = Join-Path $PSScriptRoot $solutionPath -Resolve
    $propPaths = $propPaths | ForEach-Object { Join-Path $PSScriptRoot $_ -Resolve }
    Invoke-WebRequest -Uri "https://raw.githubusercontent.com/s2quake/build/master/build.ps1" -OutFile $buildFile
    $buildFile = Resolve-Path $buildFile
    & $buildFile $solutionPath $propPaths -Publish -KeyPath $KeyPath -Sign -OutputPath $OutputPath -Framework $Framework -LogPath $LogPath -Force:$Force
}
finally {
    Remove-Item $buildFile
}
