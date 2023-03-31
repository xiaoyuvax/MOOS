using System;

namespace MOOS
{
    public class Serial
    {
        public const ushort COM1 = 0x3F8;
        //public const ushort COM2 = 0x2F8;
        //public const ushort COM3 = 0x3E8;
        //public const ushort COM4 = 0x2E8;
        //public const ushort COM5 = 0x5F8;
        //public const ushort COM6 = 0x4F8;
        //public const ushort COM7 = 0x5E8;
        //public const ushort COM8 = 0x4E8;

        public static void Initialise()
        {
            Native.Out8(COM1 + 1, 0x00);    // Disable all interrupts
            Native.Out8(COM1 + 3, 0x80);    // Enable DLAB (set baud rate divisor)
            Native.Out8(COM1 + 0, 0x01);    // Set divisor to 3 (lo byte) 38400 baud
            Native.Out8(COM1 + 1, 0x00);    //                  (hi byte)
            Native.Out8(COM1 + 3, 0x03);    // 8 bits, no parity, one stop bit
            Native.Out8(COM1 + 2, 0xC7);    // Enable FIFO, clear them, with 14-byte threshold
            Native.Out8(COM1 + 4, 0x0B);    // IRQs enabled, RTS/DSR set

            WriteLine("COM1 is ready!");
        }

        public static void Write(string s)
        {
            for (int i = 0; i < s.Length; i++)
            {
                Write(s[i]);
            }
            s.Dispose();
        }

        public static void Write(char c)
        {
            while (isTransmitEmpty() == 0) ;
            Native.Out8(COM1, (byte)(c & 0xFF));
        }

        private static int isTransmitEmpty() => Native.In8(COM1 + 5) & 0x20;

        public static void WriteLine(string s)
        {
            Write(s);
            WriteLine();
            s.Dispose();
        }

        public static void WriteLine()
        {
            Write('\r');
            Write('\n');
        }

        private static int serialReceived() => Native.In8(COM1 + 5) & 1;

        public static char ReadSerial()
        {
            while (serialReceived() == 0) ;
            return (char)Native.In8(COM1);
        }
    }

    public class ComDebugger
    {
        public static void Info(string module, string msg) => DebugModuleWrite(module, $"INFO\t{msg}");

        public static void Debug(string module, string msg) => DebugModuleWrite(module, $"DEBUG\t{msg}");

        public static void Error(string module, string msg) => DebugModuleWrite(module, $"Error\t{msg}");

        public static void Warn(string module, string msg) => DebugModuleWrite(module, $"WARN\t{msg}");

        public static void DebugModuleWrite(string module, string msg) => Serial.WriteLine($"{DateTime.Now}\t{module}\t{msg}");

        public static void DebugWrite(string msg) => Serial.WriteLine($"{DateTime.Now}\t{msg}");
    }
}