using System;
using System.IO;
using Cysharp.Threading.Tasks;
using UnityEditor.Recorder;
using UnityEditor.Recorder.Input;
using UnityEngine;
using UnityEngine.Video;

public class VideoRecorder : MonoBehaviour
{
    RecorderController controller;
    ImageRecorderSettings image;

    long frame = 1, frameCount;
    bool nextFrameExists = true;

    [SerializeField]
    VideoPlayer video;

    // TODO オーバーライドできるように
    // [SerializeField]
    // float frameRate = 30f;

    [SerializeField, Header("Export Settings")]
    int height = 4096;

    [SerializeField]
    int width = 4096;

    [SerializeField]
    int mapSize = 4096;

    [SerializeField]
    bool renderStereo = true;

    [SerializeField]
    float stereoSeparation = 0.065f;

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
        if (nextFrameExists) nextFrameExists = Next();
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

    void OnApplicationQuit()
    {
        controller?.StopRecording();
    }
}
