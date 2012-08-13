Rem Build the Hydra dlls and run tests
Rem Adjust variables as per the comments, then open a CMD prompt, CD to this directory and run this file

Rem Set MSBuild to the appropriate location for your version of the .NET Framework
Set MSBuild="C:\Windows\Microsoft.NET\Framework\v4.0.30319\msbuild"
%MSBuild% Hydra.sln
%MSBuild% HydraScavengerService\HydraScavengerService.sln

Rem If you have VS2012, change to VS110COMNTOOLS
"%VS100COMNTOOLS%..\IDE\mstest.exe" /TestContainer:Tests\bin\Debug\Tests.dll