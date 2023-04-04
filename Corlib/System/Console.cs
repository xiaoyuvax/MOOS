#define ASCII

using System.Drawing;
using System.Runtime.InteropServices;

namespace System
{
    public static unsafe partial class Console
    {
        #region Imports

        [DllImport("GetCursorPositionX")]
        public static extern int GetCursorPositionX();

        [DllImport("GetCursorPositionY")]
        public static extern int GetCursorPositionY();

        [DllImport("ConsoleReadKey")]
        public static extern ConsoleKeyInfo ReadKey(bool intercept = false);

        [DllImport("SetCursorPosition")]
        public static extern void SetCursorPosition(int x, int y);

        [DllImport("ConsoleWrite")]
        public static extern void Write(string s);

        [DllImport("ConsoleWriteChar")]
        public static extern void Write(char c, bool dontInvoke = false);

        [DllImport("ConsoleWriteLine")]
        public static extern void WriteLine(string s = null);

        [DllImport("ACPITimerSleep")]
        private static extern void ACPITimerSleep(ulong milliseconds);

        [DllImport("ClearFramebuffer")]
        private static extern void ClearFramebuffer();

        [DllImport("GetFramebufferHeight")]
        private static extern ushort GetFramebufferHeight();

        [DllImport("GetFramebufferWidth")]
        private static extern ushort GetFramebufferWidth();

        [DllImport("GetTimerTicks")]
        private static extern ulong GetTimerTicks();

        [DllImport("InvokeOnWriteHanlder")]
        private static extern void InvokeOnWriteHanlder(char chr);

        [DllImport("KeyboardCleanKeyInfo")]
        private static extern void KeyboardCleanKeyInfo(bool noModifiers);

        [DllImport("KeyboardGetKeyInfo")]
        private static extern ConsoleKeyInfo KeyboardGetKeyInfo();

        [DllImport("MoveUpFramebuffer")]
        private static extern void MoveUpFramebuffer();

        [DllImport("NativeHlt")]
        private static extern IntPtr NativeHlt();

        [DllImport("UpdateCursorFramebuffer")]
        private static extern void UpdateCursorFramebuffer();

        [DllImport("WriteFrameBuffer")]
        private static extern void WriteFramebuffer(char chr);

        #endregion Imports

        public static ConsoleColor ForegroundColor;
        public static char LastKeyChar;
        public static char ThisKeyChar;

        public delegate void OnWriteHandler(char chr);

        public static event OnWriteHandler OnWrite;

        public static int CursorX => GetCursorPositionX();

        public static int CursorY => GetCursorPositionY();
        public static int Height { get => GetFramebufferHeight() / 16; }

        public static unsafe bool KeyAvailable
        {
            get
            {
                ConsoleKeyInfo keyInfo = KeyboardGetKeyInfo();
                return keyInfo.KeyState.HasFlag(ConsoleKeyState.Pressed) && keyInfo.KeyChar != '\0';
            }
        }

        public static int Width { get => GetFramebufferWidth() / 8; }

        public static void Clear()
        {
            SetCursorPosition(0, 0);
            ClearFramebuffer();
        }


        private static void UpdateCursor()
        {
            UpdateCursorFramebuffer();
        }
    }
}