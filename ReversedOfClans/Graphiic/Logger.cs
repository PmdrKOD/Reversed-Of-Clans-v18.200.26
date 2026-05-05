using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics.Arm;

namespace ReversedOfClans.Core;

public static class Logger
{
    

    public static void Banner()
    {
        string[] art = new string[]
     {
            " ____        ____         _  ___   ____   ___   ___  ",
            "|  _ \\ ___  / ___| __   _/ |( _ ) |___ \\ / _ \\ / _ \\ ",
            "| |_) / _ \\| |     \\ \\ / / |/ _ \\   __) | | | | | | |",
            "|  _ < (_) | |___   \\ V /| | (_) | / __/| |_| | |_| |",
            "|_| \\_\\___/ \\____|   \\_/ |_|\\___(_)_____|\\___/ \\___/ ",
            "                     build : 26(Source by pmdrkdv)"
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





    public  static void Write(string message)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine(message);
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
        var stackFrame = new StackFrame(1, false);
        var className = stackFrame.GetMethod()?.DeclaringType?.Name ?? "Unknown";
        Write($"MessageManager::SendMessage  >> PackerID {PacketID} send!");
    }

    public static void PacketNot(int PacketID)
    {
        var stackFrame = new StackFrame(1, false);
        var className = stackFrame.GetMethod()?.DeclaringType?.Name ?? "Unknown";
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"MessageManager::SendMessage << PaketID {PacketID} not found");
    }


}
