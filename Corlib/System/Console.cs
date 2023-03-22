#define ASCII

using System.Drawing;
using System.Runtime.InteropServices;

namespace System
{
    public static unsafe partial class Console
    {
        [DllImport("GetFramebufferWidth")]
        private static extern ushort GetFramebufferWidth();

        [DllImport("GetFramebufferHeight")]
        private static extern ushort GetFramebufferHeight();

        [DllImport("WriteFrameBuffer")]
        private static extern void WriteFramebuffer(char chr);

        [DllImport("KeyboardCleanKeyInfo")]
        private static extern void KeyboardCleanKeyInfo(bool noModifiers);

        [DllImport("KeyboardGetKeyInfo")]
        private static extern ConsoleKeyInfo KeyboardGetKeyInfo();

        [DllImport("ClearFramebuffer")]
        private static extern void ClearFramebuffer();

        [DllImport("InvokeOnWriteHanlder")]
        private static extern void InvokeOnWriteHanlder(char chr);

        public static int Width { get => GetFramebufferWidth() / 8; }
        public static int Height { get => GetFramebufferHeight() / 16; }

        public static int CursorX = 0;
        public static int CursorY = 0;

        public delegate void OnWriteHandler(char chr);

        public static event OnWriteHandler OnWrite;

        private static uint[] ColorsFB;

        public static ConsoleColor ForegroundColor;

        public static char LastKeyChar;
        public static char ThisKeyChar;

        public static unsafe bool KeyAvailable
        {
            get
            {
                ConsoleKeyInfo keyInfo = KeyboardGetKeyInfo();
                return keyInfo.KeyState.HasFlag(ConsoleKeyState.Pressed) && keyInfo.KeyChar != '\0';
            }
        }

        public static void SetCursorPosition(int x, int y)
        {
            CursorX = x;
            CursorY = y;
            UpdateCursor();
        }

        internal static void Setup()
        {
            OnWrite += InvokeOnWriteHanlder;

            Clear();

            ColorsFB = new uint[16]
            {
                Color.Black.ToArgb(),
                Color.Blue.ToArgb(),
                Color.Green.ToArgb(),
                Color.Cyan.ToArgb(),
                Color.Red.ToArgb(),
                Color.Purple.ToArgb(),
                Color.Brown.ToArgb(),
                Color.Gray.ToArgb(),
                Color.DarkGray.ToArgb(),
                Color.LightBlue.ToArgb(),
                Color.LightGreen.ToArgb(),
                Color.LightCyan.ToArgb(),
                Color.MediumVioletRed.ToArgb(),
                Color.MediumPurple.ToArgb(),
                Color.Yellow.ToArgb(),
                Color.White.ToArgb(),
            };

            ForegroundColor = ConsoleColor.White;
        }

        public static void Wait(ref bool b)
        {
            int phase = 0;
            while (!b)
            {
                switch (phase)
                {
                    case 0:
                        Write('/', true);
                        break;

                    case 1:
                        Write('-', true);
                        break;

                    case 2:
                        Write('\\', true);
                        break;

                    case 3:
                        Write('|', true);
                        break;

                    case 4:
                        Write('/', true);
                        break;

                    case 5:
                        Write('-', true);
                        break;

                    case 6:
                        Write('\\', true);
                        break;

                    case 7:
                        Write('|', true);
                        break;
                }
                phase++;
                phase %= 8;
                CursorX--;
                ACPITimerSleep(100000);
            }
        }

        public static void Wait(uint* provider, int bit)
        {
            int phase = 0;
            while (!BitHelpers.IsBitSet(*provider, bit))
            {
                switch (phase)
                {
                    case 0:
                        Write('/', true);
                        break;

                    case 1:
                        Write('-', true);
                        break;

                    case 2:
                        Write('\\', true);
                        break;

                    case 3:
                        Write('|', true);
                        break;

                    case 4:
                        Write('/', true);
                        break;

                    case 5:
                        Write('-', true);
                        break;

                    case 6:
                        Write('\\', true);
                        break;

                    case 7:
                        Write('|', true);
                        break;
                }
                phase++;
                phase %= 8;
                CursorX--;
                ACPITimerSleep(100000);
            }
        }

        [DllImport("ACPITimerSleep")]
        private static extern void ACPITimerSleep(ulong milliseconds);

        [DllImport("GetTimerTicks")]
        private static extern ulong GetTimerTicks();

        public static bool Wait(delegate*<bool> func, int timeOutMS = -1)
        {
            ulong prev = GetTimerTicks();
            ;

            int phase = 0;
            while (!func())
            {
                if (timeOutMS >= 0 && GetTimerTicks() > (prev + (uint)timeOutMS))
                {
                    return false;
                }
                switch (phase)
                {
                    case 0:
                        Write('/', true);
                        break;

                    case 1:
                        Write('-', true);
                        break;

                    case 2:
                        Write('\\', true);
                        break;

                    case 3:
                        Write('|', true);
                        break;

                    case 4:
                        Write('/', true);
                        break;

                    case 5:
                        Write('-', true);
                        break;

                    case 6:
                        Write('\\', true);
                        break;

                    case 7:
                        Write('|', true);
                        break;
                }
                phase++;
                phase %= 8;
                CursorX--;
                ACPITimerSleep(100000);
            }
            return true;
        }

        public static void Write(string s)
        {
            ConsoleColor col = ForegroundColor;
            for (byte i = 0; i < s.Length; i++)
            {
                if (s[i] == '[')
                {
                    ForegroundColor = ConsoleColor.Yellow;
                }
                Write(s[i]);
                if (s[i] == ']')
                {
                    ForegroundColor = col;
                }
            }
            s.Dispose();
        }

        public static void Back()
        {
            if (CursorX == 0) return;
            WriteFramebuffer(' ');
            CursorX--;
            WriteFramebuffer(' ');
            UpdateCursor();
        }

        public static void Write(char chr, bool dontInvoke = false)
        {
            if (chr == '\n')
            {
                WriteLine();
                return;
            }
#if ASCII
            if (chr >= 0x20 && chr <= 0x7E)
#else
            unsafe
#endif
            {
                if (!dontInvoke)
                {
                    OnWrite?.Invoke(chr);
                }

                WriteFramebuffer(chr);

                CursorX++;
                if (CursorX == Width)
                {
                    CursorX = 0;
                    CursorY++;
                }
                MoveUp();
                UpdateCursor();
            }
        }

        public static ConsoleKeyInfo ReadKey(bool intercept = false)
        {
            KeyboardCleanKeyInfo(true);
            ConsoleKeyInfo keyInfo;
            while ((keyInfo = KeyboardGetKeyInfo()).KeyChar == '\0') NativeHlt();

            if (!intercept)
            {
                switch (keyInfo.Key)
                {
                    case ConsoleKey.Enter:
                        WriteLine();
                        break;

                    case ConsoleKey.Delete:
                    case ConsoleKey.Backspace:
                        Back();
                        break;

                    default:
                        Write(keyInfo.KeyChar);
                        break;
                }
            }
            return keyInfo;
        }

        public static string ReadLine()
        {
            string s = string.Empty;
            ConsoleKeyInfo key;
            while ((key = ReadKey()).Key != ConsoleKey.Enter)
            {
                switch (key.Key)
                {
                    case ConsoleKey.Delete:
                    case ConsoleKey.Backspace:
                        if (s.Length == 0) continue;
                        s.Length -= 1;
                        break;

                    default:
                        string cache1 = key.KeyChar.ToString();
                        string cache2 = s + cache1;
                        s.Dispose();
                        cache1.Dispose();
                        s = cache2;
                        break;
                }
                NativeHlt();
            }
            return s;
        }

        [DllImport("NativeHlt")]
        private static extern IntPtr NativeHlt();

        private static void MoveUp()
        {
            if (CursorY >= Height - 1)
            {
                MoveUpFramebuffer();
                CursorY--;
            }
        }

        [DllImport("MoveUpFramebuffer")]
        private static extern void MoveUpFramebuffer();

        private static void UpdateCursor()
        {
            UpdateCursorFramebuffer();
        }

        [DllImport("UpdateCursorFramebuffer")]
        private static extern void UpdateCursorFramebuffer();

        public static void WriteLine(string s)
        {
            Write(s);
            OnWrite?.Invoke('\n');
            WriteFramebuffer(' ');
            CursorX = 0;
            CursorY++;
            MoveUp();
            UpdateCursor();
            s.Dispose();
        }

        public static void WriteLine()
        {
            OnWrite?.Invoke('\n');
            WriteFramebuffer(' ');
            CursorX = 0;
            CursorY++;
            MoveUp();
            UpdateCursor();
        }

        public static void Clear()
        {
            CursorX = 0;
            CursorY = 0;
            ClearFramebuffer();
        }
    }
}