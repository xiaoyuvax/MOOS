using MOOS.App;
using System;

namespace Snake;

internal unsafe struct FrameBuffer : IDisposable
{
    public FrameBuffer() { }

    public const int Width = 40;
    public const int Height = 20;
    public const int Area = Width * Height;

    private char[] _chars = new char[Area];

    public void SetPixel(int x, int y, char character)
    {
        _chars[y * Width + x] = character;
    }

    public void Clear()
    {
        for (int i = 0; i < Area; i++) _chars[i] = ' ';
    }

    public readonly void Render()
    {
        const ConsoleColor snakeColor = ConsoleColor.Green;
        Interop.DebugWrite($"Render->CursorPos={Console.CursorX},{Console.CursorY}");
        Console.ForegroundColor = snakeColor;

        for (int i = 0; i < Area; i++)
        {
            Interop.ALock();
            if (i % Width == 0)
            {
                Console.SetCursorPosition(0, i / Width);
            }

            char c = _chars[i];

            if (c == '*' || (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z'))
            {
                Console.ForegroundColor = c == '*' ? ConsoleColor.Red : ConsoleColor.White;
                Console.Write(c);
                Console.ForegroundColor = snakeColor;
            }
            else
                Console.Write(c);
            Interop.UnLock();
        }
    }
}