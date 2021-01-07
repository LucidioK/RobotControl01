if [%1]==[] goto :EXPL

if /I [%1] NEQ [console] (
	if /I [%1] NEQ [classlib] goto :EXPL
)

dotnet new %1 --type project --name %2 --language C#
goto :END
:EXPL
	@echo off
	echo newDotNet5Project.cmd console/classLib ProjectName
	echo Example:
	echo newDotNet5Project.cmd console ReadSerial
:END
