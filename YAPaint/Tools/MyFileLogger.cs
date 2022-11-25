using System;
using System.Diagnostics;
using System.IO;

namespace YAPaint.Tools;

public static class MyFileLogger
{
    public static string Filepath { get; set; } = @"..\..\..\log.txt";

    public static Stopwatch SharedTimer { get; } = new Stopwatch();
    
    public static void Log(string level, string message)
    {
        File.AppendAllText(Filepath, $"{DateTime.Now} [{level}] {message}");
    }
}
