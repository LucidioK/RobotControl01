
cd RobotControl.ClassLibrary.Standard
CALL ..\buildProject.cmd %1
cd ..
PAUSE

cd RobotControl.ClassLibrary
CALL ..\buildProject.cmd %1
cd ..
PAUSE

cd RobotControl.Console.Core
CALL ..\buildProject.cmd %1
cd ..
PAUSE

cd RobotControl.UI
CALL ..\buildProject.cmd %1
cd ..
