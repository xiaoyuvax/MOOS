using MOOS.App;
using System;
using System.Diagnostics;
using System.Runtime;

namespace Snake
{
    internal static unsafe class Program
    {
        [RuntimeExport("Main")]
        public static void Main()
        {
            Interop.DebugWrite($"{Environment.NewLine}Snake Entry");
            Console.WriteLine("Snake Game");

            Game.SnakeMain();
            for (; ; );
        }
    }
}