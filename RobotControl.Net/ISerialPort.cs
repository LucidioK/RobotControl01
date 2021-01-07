﻿namespace RobotControl.Net
{
    interface ISerialPort
    {

        bool Open(int portNumber);
        void Write(string s);
        string ReadLine();
        string ReadExisting();
    }
}