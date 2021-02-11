if [%1] == [Clean] goto :Clean

:Build
dotnet restore --configfile RobotControl.ClassLibrary.Standard.Nuget.Config --no-cache --ignore-failed-sources
dotnet build   --no-restore --force
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