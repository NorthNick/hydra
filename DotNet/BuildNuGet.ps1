# Create the NuGet packages
# You need to build the DLLs using Built.bat before running this
# Adjust variables as per the comments, then open a CMD prompt, CD to this directory and run this file
# Packages are placed in the NuGet\Packages directory

# $NuGet to the location of nuget.exe
$nuget = "C:\Program Files (x86)\NuGet\NuGet.exe"

function Get-ScriptDirectory
{
$Invocation = (Get-Variable MyInvocation -Scope 1).Value
Split-Path $Invocation.MyCommand.Path
}

$scriptDir = (Get-ScriptDirectory)
$root = Join-Path $scriptDir "NuGet"
$outputDir = $root + "\Packages"

# Hydra-Messaging
$dll = "Messaging"
$package = "Hydra-" + $dll
$libDir = $root + "\" + $package + "\lib"
$dllDir = "$scriptDir\$dll\bin\debug"
Remove-Item $libDir -recurse
New-Item $libDir -type directory
Copy-Item "$dllDir\Bollywell.Hydra.$dll.dll" $libDir
Copy-Item "$dllDir\Bollywell.Hydra.$dll.pdb" $libDir
Copy-Item "$dllDir\LoveSeat.dll" $libDir
Copy-Item "$dllDir\LoveSeat.Interfaces.dll" $libDir
& $nuget pack "$root\$package\$package.nuspec" -Symbols -OutputDirectory $outputDir

# Hydra-Conversations
$dll = "Conversations"
$package = "Hydra-" + $dll
$libDir = $root + "\" + $package + "\lib"
$dllDir = "$scriptDir\$dll\bin\debug"
Remove-Item $libDir -recurse
New-Item $libDir -type directory
Copy-Item "$dllDir\Bollywell.Hydra.$dll.dll" $libDir
Copy-Item "$dllDir\Bollywell.Hydra.$dll.pdb" $libDir
& $nuget pack "$root\$package\$package.nuspec" -Symbols -OutputDirectory $outputDir

# Hydra-PubSubByType
$dll = "PubSubByType"
$package = "Hydra-" + $dll
$libDir = $root + "\" + $package + "\lib"
$dllDir = "$scriptDir\$dll\bin\debug"
Remove-Item $libDir -recurse
New-Item $libDir -type directory
Copy-Item "$dllDir\Bollywell.Hydra.$dll.dll" $libDir
Copy-Item "$dllDir\Bollywell.Hydra.$dll.pdb" $libDir
& $nuget pack "$root\$package\$package.nuspec" -Symbols -OutputDirectory $outputDir
