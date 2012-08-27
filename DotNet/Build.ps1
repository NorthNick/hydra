# Build the Hydra dlls and run tests
# Adjust variables as per the comments, run this file with PowerShell

# Set MSBuild to the appropriate location for your version of the .NET Framework
$msBuild = "C:\Windows\Microsoft.NET\Framework\v4.0.30319\msbuild"

function Get-ScriptDirectory
{
$Invocation = (Get-Variable MyInvocation -Scope 1).Value
Split-Path $Invocation.MyCommand.Path
}

$scriptDir = (Get-ScriptDirectory)

& $msBuild "$scriptDir\Hydra.sln"
& $msBuild "$scriptDir\HydraScavengerService\HydraScavengerService.sln"
& $msBuild "$scriptDir\Examples\HydraStressTest\HydraStressTest.sln"

# If you have VS2012, change to VS110COMNTOOLS
& "$Env:VS100COMNTOOLS..\IDE\mstest.exe" /TestContainer:"$scriptDir\Tests\bin\Debug\Bollywell.Hydra.Tests.dll"
