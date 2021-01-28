using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RobotControl.ClassLibrary
{
    class Constants
    {
        public static int BAUD_RATE => 9600;
        public static string PROGRAM_ID => "137410F4-0EE3-4F30-AC75-8B5DFE3EBB23";
        public static string ROBOT_PROGRAM_MARK => $"ROBOT {PROGRAM_ID}";
    }
}
