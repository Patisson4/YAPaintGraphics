using System;
using System.Diagnostics;
using System.IO;

namespace YAPaint.Tools;

public static class FileLogger
{
    public const string Filepath = @"..\..\..\log.txt";

    public static Stopwatch SharedTimer { get; } = new Stopwatch();
    
    public static void Log(string level, string message)
    {
        File.AppendAllText(Filepath, $"{DateTime.Now} [{level}] {message}\n");
    }
}
