using MOOS.Driver;
using MOOS.FS;
using MOOS.Misc;
using System;
using System.Drawing;
using System.Runtime;

#if BFLAT
using System.Runtime.CompilerServices;
#else

using Internal.Runtime.CompilerServices;

#endif

#if HasGUI

using MOOS.GUI;

#endif

namespace MOOS
{
    public static unsafe class API
    {
        #region Lock

        [RuntimeExport("Lock")]
        public static void API_Lock()
        {
            if (ThreadPool.CanLock)
            {
                if (!ThreadPool.Locked)
                {
                    ThreadPool.Lock();
                }
            }
        }

        [RuntimeExport("Unlock")]
        public static void API_Unlock()
        {
            if (ThreadPool.CanLock)
            {
                if (ThreadPool.Locked)
                {
                    if (ThreadPool.Locker == SMP.ThisCPU)
                    {
                        ThreadPool.UnLock();
                    }
                }
            }
        }

        #endregion Lock

        public static unsafe void* HandleSystemCall(string name)
        {
            //Serial.WriteLine(nameof(API) + ":" + name + "=>\t");
            void* api = name switch
            {
                "SayHello" => (delegate*<void>)&SayHello,
                "WriteLine" => (delegate*<void>)&API_WriteLine,
                "Allocate" => (delegate*<ulong, nint>)&API_Allocate,
                "Reallocate" => (delegate*<nint, ulong, nint>)&API_Reallocate,
                "Free" => (delegate*<nint, ulong>)&API_Free,
                "Sleep" => (delegate*<ulong, void>)&API_Sleep,
                "GetTick" => (delegate*<ulong>)&API_GetTick,
                "ReadAllBytes" => (delegate*<string, ulong*, byte**, void>)&API_ReadAllBytes,
                "Write" => (delegate*<char, void>)&API_Write,
                "SwitchToConsoleMode" => (delegate*<void>)&API_SwitchToConsoleMode,
                "DrawPoint" => (delegate*<int, int, uint, void>)&API_DrawPoint,
                "Lock" => (delegate*<void>)&API_Lock,
                "Unlock" => (delegate*<void>)&API_Unlock,
                "Clear" => (delegate*<uint, void>)&API_Clear,
                "Update" => (delegate*<void>)&API_Update,
                "Width" => (delegate*<uint>)&API_Width,
                "Height" => (delegate*<uint>)&API_Height,
                "WriteString" => (delegate*<string, void>)&API_WriteString,
                "GetTime" => (delegate*<ulong>)&API_GetTime,
                "DrawImage" => (delegate*<int, int, Image, void>)&API_DrawImage,
                "Error" => (delegate*<string, bool, void>)&API_Error,
                "StartThread" => (delegate*<delegate*<void>, void>)&API_StartThread,
                "Calloc" => (delegate*<ulong, ulong, void*>)&API_Calloc,
                "SndWrite" => (delegate*<byte*, int, int>)&API_SndWrite,

#if Kernel && HasGUI
                "CreateWindow" => (delegate*<int, int, int, int, string, IntPtr>)&API_CreateWindow,
                "GetWindowScreenBuf" => (delegate*<IntPtr, IntPtr>)&API_GetWindowScreenBuf,
                "BindOnKeyChangedHandler" => (delegate*<EventHandler<ConsoleKeyInfo>, void>)&API_BindOnKeyChangedHandler,
#endif

                #region Debugger

                "DebugWrite" => (delegate*<string, void>)&API_DebugWrite,

                #endregion Debugger

                #region System.Console

                "WriteFrameBuffer" => (delegate*<char, void>)&API_WriteFramebuffer,
                "KeyboardCleanKeyInfo" => (delegate*<bool, void>)&API_KeyboardCleanKeyInfo,
                "KeyboardGetKeyInfo" => (delegate*<ConsoleKeyInfo>)&API_KeyboardGetKeyInfo,
                "MoveUpFramebuffer" => (delegate*<void>)&API_MoveUpFramebuffer,
                "UpdateCursorFramebuffer" => (delegate*<void>)&API_UpdateCursorFramebuffer,
                "ClearFramebuffer" => (delegate*<void>)&API_ClearFramebuffer,
                "GetFramebufferWidth" => (delegate*<ushort>)&API_GetFramebufferWidth,
                "GetFramebufferHeight" => (delegate*<ushort>)&API_GetFramebufferHeight,
                "ACPITimerSleep" => (delegate*<ulong, void>)&API_ACPITimerSleep,
                "GetTimerTicks" => (delegate*<ulong>)&API_GetTimerTicks,
                "NativeHlt" => (delegate*<void>)&API_NativeHlt,
                "InvokeOnWriteHanlder" => (delegate*<char, void>)&API_InvokeOnWriteHanlder,
                "SetCursorPosition" => (delegate*<int, int, void>)&API_SetCursorPosition,
                "ConsoleWrite" => (delegate*<string, void>)&API_ConsoleWrite,
                "ConsoleWriteChar" => (delegate*<char, bool, void>)&API_ConsoleWriteChar,
                "ConsoleWriteLine" => (delegate*<string, void>)&API_ConsoleWriteLine,
                "GetCursorPositionX" => (delegate*<int>)&API_GetCursorPositionX,
                "GetCursorPositionY" => (delegate*<int>)&API_GetCursorPositionY,
                "ConsoleReadKey" => (delegate*<bool, ConsoleKeyInfo>)&API_ConsoleReadKey,

                #endregion System.Console

                _ => null
            };

            if (api == null) Panic.Error($"System call \"{name}\" is not found");

            return api;
        }

        public static nint API_Allocate(ulong size)
        {
            //Debug.WriteLine($"API_Allocate {size}");
            return Allocator.Allocate(size);
        }

        public static void* API_Calloc(ulong num, ulong size)
        {
            return stdlib.calloc(num, size);
        }

        public static void API_Clear(uint color) => Framebuffer.Graphics.Clear(color);

        public static void API_DebugWrite(string msg) => ComDebugger.DebugWrite(msg);

        public static void API_DrawImage(int X, int Y, Image image)
        {
            Framebuffer.Graphics.DrawImage(X, Y, image, false);
        }

        public static void API_DrawPoint(int x, int y, uint color)
        {
            Framebuffer.Graphics.DrawPoint(x, y, color);
        }

        public static void API_Error(string s, bool skippable)
        {
            Panic.Error(s, skippable);
        }

        public static ulong API_Free(nint ptr)
        {
            //Debug.WriteLine($"API_Free 0x{((ulong)ptr).ToString("x2")}");
            return Allocator.Free(ptr);
        }

        public static ulong API_GetTick()
        {
            return Timer.Ticks;
        }

        public static ulong API_GetTime()
        {
            ulong century = RTC.Century;
            ulong year = RTC.Year;
            ulong month = RTC.Month;
            ulong day = RTC.Day;
            ulong hour = RTC.Hour;
            ulong minute = RTC.Minute;
            ulong second = RTC.Second;
            ulong time = 0;

            time |= century << 56;
            time |= year << 48;
            time |= month << 40;
            time |= day << 32;
            time |= hour << 24;
            time |= minute << 16;
            time |= second << 8;

            return time;
        }

        public static uint API_Height() => Framebuffer.Height;

        public static void API_ReadAllBytes(string name, ulong* length, byte** data)
        {
            byte[] buffer = File.ReadAllBytes(name);

            *data = (byte*)Allocator.Allocate((ulong)buffer.Length);
            *length = (ulong)buffer.Length;
            fixed (byte* p = buffer) NativeCS.Movsb(*data, p, *length);

            buffer.Dispose();
        }

        public static nint API_Reallocate(nint intPtr, ulong size)
        {
            return Allocator.Reallocate(intPtr, size);
        }

        public static void API_Sleep(ulong ms)
        {
            Thread.Sleep(ms);
        }

        public static int API_SndWrite(byte* buffer, int len)
        {
            return Audio.snd_write(buffer, len);
        }

        public static void API_StartThread(delegate*<void> func)
        {
            new Thread(func).Start();
        }

        public static void API_SwitchToConsoleMode()
        {
            Framebuffer.TripleBuffered = false;
        }

        public static void API_Update()
        {
            Framebuffer.Update();
        }

        public static uint API_Width() => Framebuffer.Width;

        public static void API_Write(char c)
        {
            Console.Write(c);
        }

        public static void API_WriteLine()
        {
            Console.WriteLine();
        }

        public static void API_WriteString(string s)
        {
            Console.Write(s);
            ComDebugger.Info(nameof(API_WriteString), s);
            s.Dispose();
        }

        #region System.Console

        public static void API_ACPITimerSleep(ulong ms) => ACPITimer.Sleep(ms);

        public static void API_BindOnKeyChangedHandler(EventHandler<ConsoleKeyInfo> handler) => Keyboard.OnKeyChanged += handler;

        public static void API_ClearFramebuffer() => Console.ClearFramebuffer();

        public static ConsoleKeyInfo API_ConsoleReadKey(bool intercept = false) => Console.ReadKey(intercept);

        public static void API_ConsoleWrite(string s) => Console.Write(s);

        public static void API_ConsoleWriteChar(char c, bool dontInvoke = false) => Console.Write(c, dontInvoke);

        public static void API_ConsoleWriteLine(string s) => Console.WriteLine(s);

        public static int API_GetCursorPositionX() => Console.CursorX;

        public static int API_GetCursorPositionY() => Console.CursorY;

        public static ushort API_GetFramebufferHeight() => Framebuffer.Height;

        public static ushort API_GetFramebufferWidth() => Framebuffer.Width;

        public static ulong API_GetTimerTicks() => Timer.Ticks;

        public static void API_InvokeOnWriteHanlder(char chr) => Console.InvokeOnWriteHanlder(chr);

        public static void API_KeyboardCleanKeyInfo(bool noModifiers) => Keyboard.CleanKeyInfo(noModifiers);

        public static ConsoleKeyInfo API_KeyboardGetKeyInfo() => Keyboard.KeyInfo;

        public static void API_MoveUpFramebuffer() => Console.MoveUpFramebuffer();

        public static void API_NativeHlt() => Native.Hlt();

        public static void API_SetCursorPosition(int X, int Y) => Console.SetCursorPosition(X, Y);

        public static void API_UpdateCursorFramebuffer() => Console.UpdateCursorFramebuffer();

        public static void API_WriteFramebuffer(char chr) => Console.WriteFramebuffer(chr);

        #endregion System.Console

        #region Kernel && HasGUI

#if Kernel && HasGUI

        public static IntPtr API_CreateWindow(int X, int Y, int Width, int Height, string Title)
        {
            PortableApp papp = new PortableApp(X, Y, Width, Height);
            papp.Title = Title;
            return papp;
        }

        public static IntPtr API_GetWindowScreenBuf(IntPtr handle)
        {
            PortableApp papp = Unsafe.As<IntPtr, PortableApp>(ref handle);
            return papp.ScreenBuf;
        }

#endif

        #endregion Kernel && HasGUI

        public static void SayHello()
        {
            Console.WriteLine("Hello from exe!");
        }
    }
}