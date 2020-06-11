using System;
using System.IO;

namespace yutoVR.SphericalMovieEditor
{
    public static class PathProvider
    {
        public static string WorkDir => Path.Combine(Directory.GetCurrentDirectory(), "VideoRecorder");
        public static string DestinationDir => Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        public static string OptionPath = "Assets/Resources/RecorderOptions.asset";
    }
}
