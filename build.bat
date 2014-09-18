@echo off
set MSBuild="%ProgramFiles(x86)%\MSBuild\12.0\Bin\MSBuild.exe"

%MSBuild% /m /nologo %*
