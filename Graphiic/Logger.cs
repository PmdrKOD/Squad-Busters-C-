using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace ReversedOfClans.Core;

public static class Logger
{
    public static void Banner()
    {
        string[] art = new string[]
        {
            " ____  ____          _   ____  _  __   _  _   ",
            "/ ___|| __ )  __   _/ | | ___|/ |/ /_ | || |  ",
            "\\___ \\|  _ \\  \\ \\ / / | |___ \\| | '_ \\| || |_ ",
            " ___) | |_) |  \\ V /| |_ ___) | | (_) |__   _|",
            "|____/|____/    \\_/ |_(_)____/|_|\\___(_) |_|  ",
            "                     by pmdrkdv : https://t.me/PodValPmdrK"
        };

        ConsoleColor[] colors = {
            ConsoleColor.Cyan,
            ConsoleColor.Green,
            ConsoleColor.Yellow,
            ConsoleColor.DarkYellow,
            ConsoleColor.Red
        };

        for (int i = 0; i < art.Length; i++)
        {
            Console.ForegroundColor = colors[i % colors.Length];
            Console.WriteLine(art[i]);
        }
        Console.ResetColor();
    }

    public static void Write(string message)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] {message}");
        Console.ResetColor();
    }

    public static void Error(string message)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] [ERROR] {message}");
        Console.ResetColor();
    }

    public static void Warn(string message)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] [WARN] {message}");
        Console.ResetColor();
    }

    public static void Print(string log,
        [CallerMemberName] string method = "",
        [CallerFilePath] string path = "",
        [CallerLineNumber] int line = 0)
    {
        var stackFrame = new StackFrame(1, false);
        var className = stackFrame.GetMethod()?.DeclaringType?.Name ?? "Unknown";
        Write($"{className}::{method} {log}");
    }

    public static void PacketIn(int PacketID)
    {
        Console.ForegroundColor = ConsoleColor.Blue;
        Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] [→] PacketID {PacketID} received");
        Console.ResetColor();
    }

    public static void PacketOut(int PacketID)
    {
        Console.ForegroundColor = ConsoleColor.Magenta;
        Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] [←] PacketID {PacketID} sent");
        Console.ResetColor();
    }

    public static void PacketNot(int PacketID)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] [?] PacketID {PacketID} not implemented");
        Console.ResetColor();
    }

    public static void Info(string message)
    {
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] [INFO] {message}");
        Console.ResetColor();
    }
}