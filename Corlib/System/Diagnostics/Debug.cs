
using System.Runtime.InteropServices;

namespace System.Diagnostics
{
    public static class Debug
    {
        public static void WriteLine(string s)
        {
            for (int i = 0; i < s.Length; i++)
            {
                DebugWrite(s[i]);
            }
            DebugWriteLine();
            s.Dispose();
        }

        public static void WriteLine()
        {
            DebugWriteLine();
        }

        public static void Write(char c)
        {
            DebugWrite(c);
        }

        public static void Write(string s)
        {
            for (int i = 0; i < s.Length; i++)
            {
                DebugWrite(s[i]);
            }
            s.Dispose();
        }

        [DllImport("*")]
        static extern void DebugWrite(char c);

        [DllImport("*")]
        static extern void DebugWriteLine();

        [Conditional("DEBUG")]
        public static void Assert(bool condition)
        {
            if (!condition)
                unsafe { *(int*)0 = 0; }
        }

        [Conditional("DEBUG")]
        public static void Assert(bool condition, string msg)
        {
            if (!condition)
                unsafe { *(int*)0 = 0; }
        }
    }
}
