@echo off
set MSBuild=%Windir%\Microsoft.Net\Framework\v4.0.30319\MSBuild.exe

%MSBuild% %*
%MSBuild% /p:TargetFrameworkVersion=v3.5 /p:BuildPath="%CD%\Build" Source\Xlnt.Data\Xlnt.Data.fsproj

