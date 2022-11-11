using Avalonia;
using Avalonia.ReactiveUI;
using System;
using System.Diagnostics;
using Avalonia.Logging;

namespace YAPaint;

class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        Trace.Listeners.Add(new TextWriterTraceListener(Console.Out));

        try
        {
            BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
        }
        catch (Exception e)
        {
            Logger.Sink?.Log(LogEventLevel.Error, "All", e, e.ToString());
        }
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
                     .UsePlatformDetect()
                     .LogToTrace()
                     .UseReactiveUI();
}
