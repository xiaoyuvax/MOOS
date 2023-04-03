using MOOS.Misc;
using System;

namespace MOOS
{
    public class ComDebugger
    {
        private readonly string _module = "";
        private static long counter = 0;
        public static bool IsTimerInitialized = false;

        public ComDebugger(string moduleName)
        {
            _module = moduleName;
        }

        public static void Debug(string module, string msg) => DebugWrite(module, $"DEBUG\t{msg}");

        public static void DebugWrite(string module, string msg) => DebugWrite($"{module}\t{msg}");

        public static void DebugWrite(string msg)
        {
            if (Serial.IsInitialized)
            {
                Serial.WriteLine($"{(IsTimerInitialized ? DateTime.Now.ToString() : counter++.ToString())}\t{msg}");
            }
        }

        public static void Error(string module, string msg) => DebugWrite(module, $"Error\t{msg}");

        public static void Info(string module, string msg) => DebugWrite(module, $"INFO\t{msg}");

        public static void Warn(string module, string msg) => DebugWrite(module, $"WARN\t{msg}");

        public void Debug(string msg) => Debug(_module, msg);

        public void Error(string msg) => Error(_module, msg);

        public void Info(string msg) => Info(_module, msg);

        public void Warn(string msg) => Warn(_module, msg);

        
    }
}