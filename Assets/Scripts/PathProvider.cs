using System;
using System.IO;

public static class PathProvider
{
    public static string WorkDir => Path.Combine(Directory.GetCurrentDirectory(), "VideoRecorder");
    public static string DestinationDir => Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
}
