if [%1] == [Clean] goto :Clean

:Build
dotnet restore --configfile RobotControl.Console.Core.Nuget.Config --no-cache --ignore-failed-sources --runtime win-x64
dotnet build   --no-restore --force --runtime win-x64
dotnet publish --output Publish\win-64 --no-restore --runtime win-x64

dotnet restore --configfile RobotControl.Console.Core.Nuget.Config --no-cache --ignore-failed-sources --runtime linux-x64
dotnet build   --no-restore --force --runtime linux-x64
dotnet publish --output Publish\linux-64 --no-restore --runtime linux-x64
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
	del publish\*.* /s /q
	del publish\*.* /s /q
	rd  publish\*.* /s /q
	rd  publish\*.* /s /q	
	goto :Build
:End