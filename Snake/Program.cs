using System;
using System.Runtime;
using System.Runtime.InteropServices;

namespace Snake
{
    internal static unsafe class Program
    {
        [RuntimeExport("malloc")]
        public static nint malloc(ulong size) => Allocate(size);

        [DllImport("Allocate")]
        public static extern nint Allocate(ulong size);

        [RuntimeExport("free")]
        public static ulong free(nint ptr) => AFree(ptr);

        [DllImport("Free")]
        public static extern ulong AFree(nint ptr);

        [DllImport("WriteLine")]
        public static extern void WriteLine();

        [DllImport("ReadAllBytes")]
        public static extern void ReadAllBytes(string name, out ulong size, out byte* data);

        [DllImport("Write")]
        public static extern void Write(char c);

        [RuntimeExport("Main")]
        public static void Main()
        {
            Console.Setup();
            Console.WriteLine("TEXT");

            //Game.SnakeMain();
            for (; ; );
        }

    }
}