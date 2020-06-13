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
        static float frameDelta;
        static long frame;
        static bool nextFrameExists = true;
        static float timePreviousCaptured;

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
            options = AssetDatabase.LoadAssetAtPath<RecorderOptions>(PathProvider.OptionPath);
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

            // TODO こいつらは Timeline 側で動くべきなんじゃねーの？ 通知してもらうだけでも良いは良いが。いや、isPlaying とかが取れればそれで良いはず
            frameDelta = 1f / video.frameRate;
            video.isLooping = false;
            video.sendFrameReadyEvents = true;
            video.prepareCompleted += VideoOnPrepared;
            video.frameReady += VideoOnFrameReady;
            video.Prepare();
        }

        static async void VideoOnPrepared(VideoPlayer source)
        {
            ProgressUI.Current.SetProgress(video.frame, video.frameCount);
            Debug.Log("Start Capturing");

            timePreviousCaptured = Time.unscaledTime;
            while (video.isPrepared)
            {
                var delay = UniTask.WaitUntil(() => (Time.unscaledTime - timePreviousCaptured) > 5f);
                var userAction = UniTask.WaitUntil(() => Input.GetKeyDown(KeyCode.N));
                await UniTask.WhenAny(delay, userAction);
                Next();
                Debug.Log($"Capturing is stuck. Trying to read next frame: {frame.ToString()}.");
                timePreviousCaptured = Time.unscaledTime;
            }
        }

        static async void VideoOnFrameReady(VideoPlayer source, long frameidx)
        {
            image.FileNameGenerator.FileName = $"image_{video.frame:0000000}";
            controller.PrepareRecording();
            controller.StartRecording();
            await UniTask.WaitWhile(() => controller.IsRecording());
            ProgressUI.Current.SetProgress(video.frame, video.frameCount);
            timePreviousCaptured = Time.unscaledTime;
            if (nextFrameExists)
            {
                nextFrameExists = Next();
            }

            if (!nextFrameExists)
            {
                Debug.Log("Finish Capturing");
                Encode();
                EditorApplication.ExitPlaymode();
            }
        }

        /// <summary>
        /// Show next frame.
        /// </summary>
        /// <returns>Return true when another frame exists.</returns>
        static bool Next()
        {
            video.frame = ++frame;
            return video.frame + 1 < (long)video.frameCount;
        }

        public static async void Encode()
        {
            var path = await VideoEncoder.ExtractAudio();
            if (string.IsNullOrEmpty(path)) return;
            LoadPrerequisites();
            VideoEncoder.EncodeImagesToVideo(video.clip, options.IntermediateFormat, options.Codec, options.FileName, options.Crf, path);
        }
    }
}
