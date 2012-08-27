# UpdateVersion version - updates version numbers to version.

param([string]$version= $(throw "Version required."))

# In AssemblyInfo files, replace these lines:
# [assembly: AssemblyVersion("0.4.0.0")]
# [assembly: AssemblyFileVersion("0.4.0.0")]
$assemblyInfoFiles=get-childitem . AssemblyInfo.cs -rec
foreach ($file in $assemblyInfoFiles)
{
(Get-Content $file.PSPath) | 
Foreach-Object {$_ -replace "AssemblyVersion\(`"[^`"]*`"\)", "AssemblyVersion(`"$version`")"} | 
Foreach-Object {$_ -replace "AssemblyFileVersion\(`"[^`"]*`"\)", "AssemblyFileVersion(`"$version`")"} | 
Set-Content $file.PSPath
}

# In nuspec files, replace these lines:
# <version>0.4.0</version>
# <dependency id="Hydra-Messaging" version="0.4.0" />
$nugetFiles=get-childitem . *.nuspec -rec
foreach ($file in $nugetFiles)
{
(Get-Content $file.PSPath) | 
Foreach-Object {$_ -replace "<version>[^<]*</version>", "<version>$version</version>"} | 
Foreach-Object {$_ -replace "<dependency id=`"Hydra-Messaging`" version=`"[^`"]*`" \/>", "<dependency id=`"Hydra-Messaging`" version=`"$version`" \/>"} | 
Set-Content $file.PSPath
}