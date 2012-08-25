Rem Create the NuGet packages
Rem You need to build the DLLs using Built.bat before running this
Rem Adjust variables as per the comments, then open a CMD prompt, CD to this directory and run this file
Rem Packages are placed in the NuGet\Packages directory

Rem Set NuGet to the location of nuget.exe
Set NuGet="C:\Program Files (x86)\NuGet\NuGet.exe"


Set Root=NuGet
Set OutputDir=%Root%\Packages

Rem Hydra-Messaging
Set Dll="Messaging"
Set Pkg=Hydra-%Dll%
Set LibDir=%Root%\%Pkg%\lib
del /q %LibDir%
mkdir %LibDir%
copy %Dll%\bin\debug\Bollywell.Hydra.%Dll%.dll %LibDir%
copy %Dll%\bin\debug\LoveSeat.dll %LibDir%
copy %Dll%\bin\debug\LoveSeat.Interfaces.dll %LibDir%
%NuGet% pack %Root%\%Pkg%\%Pkg%.nuspec -OutputDirectory %OutputDir%

Rem Hydra-Conversations
Set Dll="Conversations"
Set Pkg=Hydra-%Dll%
Set LibDir=%Root%\%Pkg%\lib
del /q %LibDir%
mkdir %LibDir%
copy %Dll%\bin\debug\Bollywell.Hydra.%Dll%.dll %LibDir%
%NuGet% pack %Root%\%Pkg%\%Pkg%.nuspec -OutputDirectory %OutputDir%

Rem Hydra-PubSubByType
Set Dll="PubSubByType"
Set Pkg=Hydra-%Dll%
Set LibDir=%Root%\%Pkg%\lib
del /q %LibDir%
mkdir %LibDir%
copy %Dll%\bin\debug\Bollywell.Hydra.%Dll%.dll %LibDir%
%NuGet% pack %Root%\%Pkg%\%Pkg%.nuspec -OutputDirectory %OutputDir%