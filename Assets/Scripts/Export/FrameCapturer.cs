using System.IO;
using Cysharp.Threading.Tasks;
using UnityEditor;
using UnityEditor.Recorder;
using UnityEditor.Recorder.Input;
using UnityEngine;
using UnityEngine.Video;

namespace yutoVR.SphericalMovieEditor
{
    public enum Codec
    {
        H264,
        H265,
        H264_NVENC,
        H265_NVENC
    }

    public class FrameCapturer : EditorWindow
    {
        static RecorderOptions options;
        static VideoPlayer video;
        static ImageRecorderSettings image;
        static RecorderController controller;
        static ulong captureId;

        public static void Export()
        {
            RemoveImages();
            PrepareToRecord();
        }

        static void RemoveImages()
        {
            if (Directory.Exists(PathProvider.WorkDir))
            {
                try
                {
                    Directory.Delete(PathProvider.WorkDir, true);
                    Debug.Log("Removed old images.");
                }
                catch (IOException)
                {
                    Debug.LogError("Maybe other application is using working directory. See an exception below.");
                    throw;
                }
            }
        }

        static void PrepareToRecord()
        {
            LoadPrerequisites();
            options.startRecordingOnEnterPlayMode = true;
            EditorUtility.SetDirty(options);
            AssetDatabase.SaveAssets();
            EditorApplication.EnterPlaymode();
            // ここでシーンに配置した VideoRecorderBridge が StartCapturing を叩く。
            // なぜならプレイモードに入るタイミングで、おそらくドメインがリロードされて実行が停止してしまうからだ。
        }

        static void LoadPrerequisites()
        {
            options = RecorderOptions.Options;
            video = FindObjectOfType<VideoPlayer>();
        }

        public static void StartCapturing()
        {
            LoadPrerequisites();
            var settings = CreateInstance<RecorderControllerSettings>();
            settings.SetRecordModeToSingleFrame(0);

            image = CreateInstance<ImageRecorderSettings>();
            image.imageInputSettings = new Camera360InputSettings
            {
                Source = ImageSource.MainCamera,
                MapSize = options.MapSize,
                OutputHeight = options.Height,
                OutputWidth = options.Width,
                RenderStereo = options.renderStereo,
                StereoSeparation = options.StereoSeparation,
            };
            image.OutputFormat = options.IntermediateFormat;
            image.FileNameGenerator.Root = OutputPath.Root.Absolute;
            image.FileNameGenerator.Leaf = PathProvider.WorkDir;
            settings.AddRecorderSettings(image);

            controller = new RecorderController(settings);

            captureId = 0;
            TimelinePlayer.Current.PlayFrameByFrame();
        }

        public static async UniTask CaptureFrame()
        {
            image.FileNameGenerator.FileName = $"image_{captureId:0000000}";
            ++captureId;
            controller.PrepareRecording();
            controller.StartRecording();
            await UniTask.WaitWhile(() => controller.IsRecording());
        }

        // TODO Encoder に移す
        public static async void Encode()
        {
            var path = await VideoEncoder.ExtractAudio();
            if (string.IsNullOrEmpty(path)) return;
            LoadPrerequisites();
            VideoEncoder.EncodeImagesToVideo(video.clip, options.IntermediateFormat, options.Codec, options.FileName, options.Crf, path);
        }
    }
}
