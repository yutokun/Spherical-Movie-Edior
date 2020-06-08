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
    Codec codec;

    void Start()
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
        image.FileNameGenerator.Leaf = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "image sequence");
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
            if (encodeOnFinish) EncodeToVideo();
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

    void EncodeToVideo()
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

        var startInfo = new ProcessStartInfo
        {
            Arguments = $"-r {video.clip.frameRate.ToString()} -i image_%04d.png -vcodec {codec} -pix_fmt yuv420p out.mp4",
            FileName = "ffmpeg",
            WorkingDirectory = image.FileNameGenerator.Leaf
        };
        var process = new Process { StartInfo = startInfo };
        process.Start();
    }

    void OnApplicationQuit()
    {
        controller?.StopRecording();
    }
}
