if [%1] == [Clean] goto :Clean
if [%1] == [clean] goto :Clean

:Build
dotnet restore --configfile ..\RobotControl.Nuget.Config --no-cache --ignore-failed-sources  --runtime win-x64   --packages ..\packages
dotnet build   --no-restore --force  --runtime win-x64

dotnet restore --configfile ..\RobotControl.Nuget.Config --no-cache --ignore-failed-sources  --runtime linux-x64 --packages ..\packages
dotnet build   --no-restore --force  --runtime linux-x64
goto :End
:Clean
	dotnet clean
	del bin\*.* /s /q
	del bin\*.* /s /q
	rd  bin\*.* /s /q
	rd  bin\*.* /s /q
	del obj\*.* /s /q
	del obj\*.* /s /q
	rd  obj\*.* /s /q
	rd  obj\*.* /s /q
	goto :Build
:End