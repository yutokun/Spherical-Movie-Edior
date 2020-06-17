using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEditor;
using UnityEditor.Recorder;
using UnityEngine;
using UnityEngine.Video;
using Debug = UnityEngine.Debug;

namespace yutoVR.SphericalMovieEditor
{
    public class FfmpegExecutor
    {
        readonly StringBuilder arguments = new StringBuilder();
        readonly Process ffmpeg;

        public FfmpegExecutor()
        {
            var startInfo = new ProcessStartInfo { FileName = "ffmpeg" };
            ffmpeg = new Process { StartInfo = startInfo };
        }

        public void Add(string argument, string param = "")
        {
            arguments.Append($"{argument} ");
            if (param != "") arguments.Append($"{param} ");
        }

        public void SetWorkingDirectory(string directory) => ffmpeg.StartInfo.WorkingDirectory = directory;

        public bool HasVisibleWindow
        {
            set
            {
                ffmpeg.StartInfo.CreateNoWindow = !value;
                ffmpeg.StartInfo.UseShellExecute = value;
            }
        }

        public bool RedirectStandardError
        {
            set => ffmpeg.StartInfo.RedirectStandardError = value;
        }

        public void Execute()
        {
            ffmpeg.StartInfo.Arguments = arguments.ToString();
            ffmpeg.Start();
        }

        public void WaitForExit() => ffmpeg.WaitForExit();
        public Task<string> ReadAllErrorAsync() => ffmpeg.StandardError.ReadToEndAsync();

        public static implicit operator string(FfmpegExecutor s) => s.arguments.ToString();
    }

    public class VideoEncoder : MonoBehaviour
    {
        const string FfmpegMissingMessage = "Couldn't execute ffmpeg. Please install it from https://ffmpeg.org/download.html";

        public static async void Encode(double audioOffset)
        {
            var path = await ExtractAudio();
            if (string.IsNullOrEmpty(path)) return;
            var options = RecorderOptions.CurrentOptions;
            var video = FindObjectOfType<VideoPlayer>();
            EncodeImagesToVideo(video.clip, options, path, audioOffset);
        }

        static async Task<string> ExtractAudio()
        {
            if (!FfmpegIsAvailable)
            {
                Debug.Log(FfmpegMissingMessage);
                return null;
            }

            var videoPlayer = FindObjectOfType<VideoPlayer>(); // TODO SME で取るのが正しいのでは
            if (!videoPlayer)
            {
                Debug.Log("Couldn't find Video Player.");
                return null;
            }

            var clip = videoPlayer.clip;
            if (!clip)
            {
                Debug.Log("No video assigned.");
                return null;
            }

            // TODO source:URL にも対応
            var clipPathFromProject = AssetDatabase.GetAssetPath(clip);
            var path = Path.Combine(Directory.GetParent(Application.dataPath).FullName, clipPathFromProject);
            var extension = await GetSuitableAudioExtension(path);
            var destination = Path.Combine(PathProvider.WorkDir, $"audio.{extension}");

            var ffmpeg = new FfmpegExecutor();
            ffmpeg.Add("-y");
            ffmpeg.Add("-i", $"\"{path}\"");
            ffmpeg.Add("-vn");
            ffmpeg.Add("-acodec", "copy");
            ffmpeg.Add($"\"{destination}\"");
            ffmpeg.Execute();
            ffmpeg.WaitForExit();
            return destination;
        }

        static async Task<string> GetSuitableAudioExtension(string videoPath)
        {
            if (!FfmpegIsAvailable)
            {
                Debug.Log(FfmpegMissingMessage);
                return null;
            }

            var ffmpeg = new FfmpegExecutor();
            ffmpeg.Add($"-i \"{videoPath}\"");
            ffmpeg.HasVisibleWindow = false;
            ffmpeg.RedirectStandardError = true;
            ffmpeg.Execute();
            var result = await ffmpeg.ReadAllErrorAsync();
            var audioType = Regex.Match(result, "Audio: (?<type>.+?) ").Groups["type"].Value;

            var extension = "";
            switch (audioType)
            {
                case "aac":
                    extension = "aac";
                    break;

                case "vorbis":
                    extension = "ogg";
                    break;
            }

            if (string.IsNullOrEmpty(extension)) throw new Exception($"Couldn't specify suitable extension. AudioType is {audioType}");

            return extension;
        }

        static async void EncodeImagesToVideo(VideoClip clip, RecorderOptions options, string audioPath, double audioOffset)
        {
            if (!FfmpegIsAvailable)
            {
                Debug.Log(FfmpegMissingMessage);
                return;
            }

            string codecStr;
            switch (options.Codec)
            {
                case Codec.H264:
                    codecStr = "libx264";
                    break;

                case Codec.H265:
                    codecStr = "hevc";
                    break;

                case Codec.H264_NVENC:
                    codecStr = "h264_nvenc";
                    break;

                case Codec.H265_NVENC:
                    codecStr = "hevc_nvenc";
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }

            var destination = GetValidFilePath(options.FileName);
            var extension = options.IntermediateFormat == ImageRecorderSettings.ImageRecorderOutputFormat.JPEG ? "jpg" : options.IntermediateFormat.ToString().ToLower();
            var ffmpeg = new FfmpegExecutor();
            ffmpeg.Add("-r", clip.frameRate.ToString());
            ffmpeg.Add("-i", $"image_%07d.{extension}");
            ffmpeg.Add("-itsoffset", audioOffset.ToString());
            ffmpeg.Add("-i", $"\"{audioPath}\"");
            ffmpeg.Add("-vcodec", $"{codecStr}");
            ffmpeg.Add("-acodec", "copy");
            ffmpeg.Add("-crf", $"{options.Crf.ToString()}");
            ffmpeg.Add("-maxrate", options.MaxBitRate.ToString());
            ffmpeg.Add("-pix_fmt", "yuv420p");
            if (options.fastStart) ffmpeg.Add("-movflags", "faststart");
            ffmpeg.Add($"\"{destination}\"");
            ffmpeg.SetWorkingDirectory(PathProvider.WorkDir);
            ffmpeg.Execute();
            Debug.Log($"Creating video in {destination}");
            await UniTask.DelayFrame(2); // Wait for log.
            ffmpeg.WaitForExit();
            Debug.Log("Video Created");
        }

        public static async UniTask EncodeProxy(VideoClip clip)
        {
            if (!FfmpegIsAvailable)
            {
                Debug.Log(FfmpegMissingMessage);
                return;
            }

            var destination = PathProvider.GetProxyPath(clip);
            Directory.CreateDirectory(PathProvider.ProxyDir);
            if (File.Exists(destination)) File.Delete(destination);

            var clipPathFromProject = AssetDatabase.GetAssetPath(clip);
            var clipPath = Path.Combine(Directory.GetParent(Application.dataPath).FullName, clipPathFromProject);

            var ffmpeg = new FfmpegExecutor();
            ffmpeg.Add("-r", $"{clip.frameRate.ToString()}");
            ffmpeg.Add("-i", $"\"{clipPath}\"");
            ffmpeg.Add("-s", "512x512");
            ffmpeg.Add("-vcodec", "libx264");
            ffmpeg.Add("-acodec", "copy");
            ffmpeg.Add("-pix_fmt", "yuv420p");
            ffmpeg.Add($"\"{destination}\"");
            ffmpeg.SetWorkingDirectory(PathProvider.WorkDir);
            ffmpeg.Execute();
            Debug.Log("Creating Proxy...");
            await UniTask.DelayFrame(2); // Wait for render log.
            ffmpeg.WaitForExit();
            Debug.Log("Proxy Created");
        }

        static string GetValidFilePath(string fileName)
        {
            var path = Path.Combine(PathProvider.DestinationDir, $"{fileName}.mp4");
            if (!File.Exists(path)) return path;

            for (var i = 2; i < int.MaxValue; i++)
            {
                path = Path.Combine(PathProvider.DestinationDir, $"{fileName} {i.ToString()}.mp4");
                if (!File.Exists(path)) return path;
            }

            throw new Exception("Couldn't create valid file path.");
        }

        // TODO Executor を Ffmpeg にして統合
        static bool FfmpegIsAvailable
        {
            get
            {
                try
                {
                    var startInfo = new ProcessStartInfo { FileName = "ffmpeg" };
                    var p = new Process { StartInfo = startInfo };
                    p.Start();
                    return true;
                }
                catch
                {
                    return false;
                }
            }
        }
    }
}
