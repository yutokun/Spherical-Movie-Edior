using System;
using System.IO;
using UnityEditor;
using UnityEditor.Recorder;
using UnityEditor.Recorder.Input;
using UnityEngine;
using UnityEngine.Video;

public class VideoRecorder : MonoBehaviour
{
    RecorderController controller;

    [SerializeField]
    VideoPlayer video;

    [SerializeField, Header("Export Settings")]
    bool isImage;

    // TODO オーバーライドできるように
    // [SerializeField]
    // float frameRate = 30f;

    [SerializeField]
    int height = 4096;

    [SerializeField]
    int width = 4096;

    [SerializeField]
    bool renderStereo = true;

    [SerializeField]
    float stereoSeparation = 0.065f;

    void Start()
    {
        var settings = ScriptableObject.CreateInstance<RecorderControllerSettings>();
        settings.FrameRate = (float)video.clip.frameRate;
        settings.CapFrameRate = true;
        settings.FrameRatePlayback = FrameRatePlayback.Constant;

        if (isImage)
        {
            var image = ScriptableObject.CreateInstance<ImageRecorderSettings>();
            image.imageInputSettings = new Camera360InputSettings
            {
                Source = ImageSource.MainCamera,
                MapSize = 4096,
                OutputHeight = height,
                OutputWidth = width,
                RenderStereo = renderStereo,
                StereoSeparation = stereoSeparation,
            };
            image.OutputFormat = ImageRecorderSettings.ImageRecorderOutputFormat.PNG;
            image.FileNameGenerator.Root = OutputPath.Root.Absolute;
            image.FileNameGenerator.Leaf = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "image sequence");
            image.FileNameGenerator.FileName = "image_<Frame>";
            settings.AddRecorderSettings(image);
        }
        else
        {
            var movie = ScriptableObject.CreateInstance<MovieRecorderSettings>();
            movie.OutputFormat = MovieRecorderSettings.VideoRecorderOutputFormat.MP4;
            movie.ImageInputSettings = new Camera360InputSettings
            {
                Source = ImageSource.MainCamera,
                MapSize = 4096,
                OutputHeight = height,
                OutputWidth = width,
                RenderStereo = renderStereo,
                StereoSeparation = stereoSeparation,
            };
            movie.VideoBitRateMode = VideoBitrateMode.High;
            movie.OutputFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "movie");
            settings.AddRecorderSettings(movie);
        }

        controller = new RecorderController(settings);
        controller.PrepareRecording();
        controller.StartRecording();
    }

    void OnApplicationQuit()
    {
        controller.StopRecording();
    }
}
