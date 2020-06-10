using System;
using System.Diagnostics;
using System.IO;
using Cysharp.Threading.Tasks;
using UnityEditor;
using UnityEditor.Recorder;
using UnityEditor.Recorder.Input;
using UnityEngine;
using UnityEngine.Video;
using Debug = UnityEngine.Debug;

public class VideoRecorder : MonoBehaviour
{
    enum Codec
    {
        H264,
        H265,
        H264_NVENC,
        H265_NVENC
    }

    RecorderController controller;
    ImageRecorderSettings image;

    long frame = 1, frameCount;
    bool nextFrameExists = true;

    public static string WorkDir => Path.Combine(Directory.GetCurrentDirectory(), "VideoRecorder");
    public static string DestinationDir => Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

    [SerializeField]
    VideoPlayer video;

    [SerializeField, Header("Image Settings")]
    int height = 4096;

    [SerializeField]
    int width = 4096;

    [SerializeField]
    int mapSize = 4096;

    [SerializeField]
    bool renderStereo = true;

    [SerializeField]
    float stereoSeparation = 0.065f;

    [SerializeField, Header("Encode Settings")]
    bool encodeOnFinish = true; // TODO 検出して自動化できそう

    [SerializeField]
    Codec codec = Codec.H265;

    [SerializeField, Range(0, 51)]
    int crf = 23;

    [SerializeField]
    string fileName = "encoded";

    void Start()
    {
        RemoveImages();
        PrepareToRecord();
    }

    void RemoveImages()
    {
        if (Directory.Exists(WorkDir))
        {
            Directory.Delete(WorkDir, true);
            Debug.Log("Removed old images.");
        }
    }

    void PrepareToRecord()
    {
        var settings = ScriptableObject.CreateInstance<RecorderControllerSettings>();
        settings.SetRecordModeToSingleFrame(0);

        image = ScriptableObject.CreateInstance<ImageRecorderSettings>();
        image.imageInputSettings = new Camera360InputSettings
        {
            Source = ImageSource.MainCamera,
            MapSize = mapSize,
            OutputHeight = height,
            OutputWidth = width,
            RenderStereo = renderStereo,
            StereoSeparation = stereoSeparation,
        };
        image.OutputFormat = ImageRecorderSettings.ImageRecorderOutputFormat.PNG;
        image.FileNameGenerator.Root = OutputPath.Root.Absolute;
        image.FileNameGenerator.Leaf = WorkDir;
        settings.AddRecorderSettings(image);

        controller = new RecorderController(settings);

        frameCount = (long)video.frameCount;
        video.sendFrameReadyEvents = true;
        video.started += VideoOnStarted;
        video.frameReady += VideoOnFrameReady;
        video.Play();
    }

    void VideoOnStarted(VideoPlayer source)
    {
        source.Pause();
        Debug.Log("Start Capturing");
    }

    async void VideoOnFrameReady(VideoPlayer source, long frameidx)
    {
        image.FileNameGenerator.FileName = $"image_{frame:0000}";
        controller.PrepareRecording();
        controller.StartRecording();
        await UniTask.WaitWhile(() => controller.IsRecording());
        if (nextFrameExists)
        {
            nextFrameExists = Next();
        }
        else
        {
            Debug.Log("Finish Capturing");
            if (encodeOnFinish)
            {
                var path = await VideoEncoder.ExtractAudio();
                EncodeToVideo(path);
            }

            EditorApplication.ExitPlaymode();
        }
    }

    /// <summary>
    /// Show next frame.
    /// </summary>
    /// <returns>Return true when another frame exists.</returns>
    bool Next()
    {
        video.frame = frame++;
        return frame <= frameCount;
    }

    void EncodeToVideo(string audioPath)
    {
        string codec;
        switch (this.codec)
        {
            case Codec.H264:
                codec = "libx264";
                break;

            case Codec.H265:
                codec = "hevc";
                break;

            case Codec.H264_NVENC:
                codec = "h264_nvenc";
                break;

            case Codec.H265_NVENC:
                codec = "hevc_nvenc";
                break;

            default:
                throw new ArgumentOutOfRangeException();
        }


        var destination = GetValidFilePath();
        var startInfo = new ProcessStartInfo
        {
            Arguments = $"-r {video.clip.frameRate.ToString()} -i image_%04d.png -i \"{audioPath}\" -vcodec {codec} -crf {crf.ToString()} -pix_fmt yuv420p \"{destination}\"",
            FileName = "ffmpeg",
            WorkingDirectory = WorkDir
        };
        var process = new Process { StartInfo = startInfo };
        process.Start();
        Debug.Log($"Creating video in {destination}");
    }

    string GetValidFilePath()
    {
        var path = Path.Combine(DestinationDir, $"{fileName}.mp4");
        if (!File.Exists(path)) return path;

        for (var i = 2; i < int.MaxValue; i++)
        {
            path = Path.Combine(DestinationDir, $"{fileName} {i.ToString()}.mp4");
            if (!File.Exists(path)) return path;
        }

        throw new Exception("Couldn't create valid file path.");
    }

    void OnApplicationQuit()
    {
        controller?.StopRecording();
    }

    void Reset()
    {
        if (!video) video = GetComponent<VideoPlayer>();
    }
}
