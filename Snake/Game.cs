﻿using MOOS.App;
using System;
using Thread = System.Threading.Thread;

namespace Snake;

internal class Game
{
    private enum Result
    {
        Win, Loss
    }

    private Random _random;
    private static bool isRunning = true;

    private Game(uint randomSeed)
    {
        _random = new Random(randomSeed);
    }

    //~Game()
    //{
    //    isRunning = false;
    //    _random.Dispose();
    //}

    private Result Run(ref FrameBuffer fb)
    {
        Snake s = new Snake(
            (byte)(_random.Next() % FrameBuffer.Width),
            (byte)(_random.Next() % FrameBuffer.Height),
            (Snake.Direction)(_random.Next() % 4));

        MakeFood(s, out byte foodX, out byte foodY);

        Result result;
        while (true)
        {
            fb.Clear();

            if (!s.Update())
            {
                s.Draw(ref fb);
                result = Result.Loss;
                break;
            }

            s.Draw(ref fb);

            if (Console.KeyAvailable)
            {
                ConsoleKeyInfo ki = Console.ReadKey(intercept: true);
                Interop.DebugWrite($"ConsoleKeyInfo:{ki.Key}");
                switch (ki.Key)
                {
                    case ConsoleKey.Up:
                        s.Course = Snake.Direction.Up; break;
                    case ConsoleKey.Down:
                        s.Course = Snake.Direction.Down; break;
                    case ConsoleKey.Left:
                        s.Course = Snake.Direction.Left; break;
                    case ConsoleKey.Right:
                        s.Course = Snake.Direction.Right; break;
                }
            }

            if (s.HitTest(foodX, foodY))
            {
                if (s.Extend()) MakeFood(s, out foodX, out foodY);
                else
                {
                    result = Result.Win;
                    break;
                }
            }

            fb.SetPixel(foodX, foodY, '*');

            fb.Render();

            Thread.Sleep(2000);
        }

        s.Dispose();
        return result;
    }

    private void MakeFood(in Snake snake, out byte foodX, out byte foodY)
    {
        do
        {
            foodX = (byte)(_random.Next() % FrameBuffer.Width);
            foodY = (byte)(_random.Next() % FrameBuffer.Height);
        }
        while (snake.HitTest(foodX, foodY));
    }

    public static void SnakeMain()
    {

        FrameBuffer fb = new FrameBuffer();
        while (isRunning)
        {

            Game g = new Game(100);
            Result result = g.Run(ref fb);
            Interop.WriteString("SnakeMain.4");
            string message = result == Result.Win ? "You win" : "You lose";
            result.Dispose();
            Interop.WriteString("SnakeMain.5");
            int position = (FrameBuffer.Width - message.Length) / 2;
            for (int i = 0; i < message.Length; i++)
            {
                fb.SetPixel(position + i, FrameBuffer.Height / 2, message[i]);
            }
            Interop.WriteString("SnakeMain.6");
            fb.Render();
            Interop.WriteString("SnakeMain.7");
            //Console.ReadKey(intercept: true);
        }
        fb.Dispose();
    }
}
