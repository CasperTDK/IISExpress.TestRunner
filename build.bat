set msbuild=%windir%\Microsoft.NET\Framework\v4.0.30319\msbuild.exe
set currentDir=%~dp0%

%msbuild% %currentDir%\scripts\build.proj