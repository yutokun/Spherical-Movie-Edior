using System;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Video;
using Debug = UnityEngine.Debug;

public class VideoEncoder : MonoBehaviour
{
    public static async Task<string> ExtractAudio()
    {
        var videoPlayer = FindObjectOfType<VideoPlayer>();
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
        var path = Path.Combine(Directory.GetParent(Application.dataPath).FullName, clip.originalPath);
        var extension = await GetSuitableAudioExtension(path);
        var destination = Path.Combine(PathProvider.WorkDir, $"audio.{extension}");

        var startInfo = new ProcessStartInfo
        {
            Arguments = $"-i \"{path}\" -vn -acodec copy \"{destination}\"",
            FileName = "ffmpeg",
            CreateNoWindow = true,
            UseShellExecute = false,
            RedirectStandardError = true
        };
        var extractor = new Process { StartInfo = startInfo };
        extractor.Start();
        extractor.WaitForExit();
        return destination;
    }

    static async Task<string> GetSuitableAudioExtension(string videoPath)
    {
        var startInfo = new ProcessStartInfo
        {
            Arguments = $"-i \"{videoPath}\"",
            FileName = "ffmpeg",
            CreateNoWindow = true,
            UseShellExecute = false,
            RedirectStandardError = true
        };
        var infoReader = new Process { StartInfo = startInfo };
        infoReader.Start();
        var result = await infoReader.StandardError.ReadToEndAsync();
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
}
