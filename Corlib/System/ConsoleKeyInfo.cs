using System.Runtime.InteropServices;

namespace System
{
    [StructLayout(LayoutKind.Sequential)]
    public struct ConsoleKeyInfo
    {
        public ConsoleKeyInfo(char keyChar, ConsoleKey key, bool shift, bool alt, bool control)
        {
            Key = key;
            KeyChar = keyChar;
            if (shift) Modifiers = ConsoleModifiers.Shift;
            if (alt) Modifiers = ConsoleModifiers.Alt;
            if (control) Modifiers = ConsoleModifiers.Control;
        }

        public int ScanCode;
        public ConsoleKey Key;
        public char KeyChar;
        public ConsoleModifiers Modifiers;
        public ConsoleKeyState KeyState;
    }
}