# Create the NuGet packages
# You need to build the DLLs using Build.ps1 before running this
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
$srcDir = $root + "\" + $package + "\src"
$projDir = "$scriptDir\$dll"
$dllDir = "$projDir\bin\debug"
Remove-Item $libDir -recurse
Remove-Item $srcDir -recurse
New-Item $libDir -type directory
New-Item $srcDir -type directory
Copy-Item "$dllDir\Shastra.Hydra.$dll.dll" $libDir
Copy-Item "$dllDir\Shastra.Hydra.$dll.pdb" $libDir
Copy-Item "$dllDir\LoveSeat.dll" $libDir
Copy-Item "$dllDir\LoveSeat.Interfaces.dll" $libDir
Copy-Item -Recurse -Filter *.cs $projDir $srcDir
& $nuget pack "$root\$package\$package.nuspec" -Symbols -OutputDirectory $outputDir

# Hydra-Conversations
$dll = "Conversations"
$package = "Hydra-" + $dll
$libDir = $root + "\" + $package + "\lib"
$srcDir = $root + "\" + $package + "\src"
$projDir = "$scriptDir\$dll"
$dllDir = "$scriptDir\$dll\bin\debug"
Remove-Item $libDir -recurse
Remove-Item $srcDir -recurse
New-Item $libDir -type directory
New-Item $srcDir -type directory
Copy-Item "$dllDir\Shastra.Hydra.$dll.dll" $libDir
Copy-Item "$dllDir\Shastra.Hydra.$dll.pdb" $libDir
Copy-Item -Recurse -Filter *.cs $projDir $srcDir
& $nuget pack "$root\$package\$package.nuspec" -Symbols -OutputDirectory $outputDir

# Hydra-PubSubByType
$dll = "PubSubByType"
$package = "Hydra-" + $dll
$libDir = $root + "\" + $package + "\lib"
$srcDir = $root + "\" + $package + "\src"
$projDir = "$scriptDir\$dll"
$dllDir = "$scriptDir\$dll\bin\debug"
Remove-Item $libDir -recurse
Remove-Item $srcDir -recurse
New-Item $libDir -type directory
New-Item $srcDir -type directory
Copy-Item "$dllDir\Shastra.Hydra.$dll.dll" $libDir
Copy-Item "$dllDir\Shastra.Hydra.$dll.pdb" $libDir
Copy-Item -Recurse -Filter *.cs $projDir $srcDir
& $nuget pack "$root\$package\$package.nuspec" -Symbols -OutputDirectory $outputDir
