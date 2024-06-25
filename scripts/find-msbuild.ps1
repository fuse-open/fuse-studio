$env:Path += ";${env:ProgramFiles}\Microsoft Visual Studio\Installer"
$env:Path += ";${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer"

If (Get-Command "vswhere.exe" -ErrorAction SilentlyContinue)
{
    vswhere.exe | Out-String -Stream | Select-String -SimpleMatch "installationPath:" | ForEach {
        $installationPath = $_ -Replace "^installationPath: ", ""
        $msbuild = "$installationPath\MSBuild\Current\bin\msbuild.exe"
        If (Test-Path $msbuild)
        {
            Write-Output $msbuild
            exit 0
        }
        $msbuild = "$installationPath\MSBuild\15.0\bin\msbuild.exe"
        If (Test-Path $msbuild)
        {
            Write-Output $msbuild
            exit 0
        }
    }
}

$msbuild1="${env:ProgramFiles}\MSBuild\14.0\bin\MSBuild.exe"
$msbuild2="${env:ProgramFiles(x86)}\MSBuild\14.0\bin\MSBuild.exe"
If (Test-Path $msbuild1)
{
    Write-Output $msbuild1
    exit 0
}
ElseIf (Test-Path $msbuild2)
{
    Write-Output $msbuild2
    exit 0
}

Write-Error -Message @"
ERROR: Microsoft Build Tools 2015+ not installed (C# 6/.NET 4.5 support)
(not found) vswhere / Visual Studio 2019
(not found) vswhere / Visual Studio 2017
(not found) $msbuild1"
(not found) $msbuild2"
"@
exit 1
