if [%1] == [Clean] goto :Clean
if [%1] == [clean] goto :Clean

:Build

dotnet restore --configfile ..\RobotControl.Nuget.Config --no-cache --ignore-failed-sources --runtime win-x64  --packages ..\packages
dotnet build   --no-restore --runtime win-x64 --no-dependencies --force
dotnet publish --no-restore --runtime win-x64 --no-dependencies --output Publish\win-64

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