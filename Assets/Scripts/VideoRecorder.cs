using System.IO;
using Cysharp.Threading.Tasks;
using UnityEditor;
using UnityEditor.Recorder;
using UnityEditor.Recorder.Input;
using UnityEngine;
using UnityEngine.Video;
using Debug = UnityEngine.Debug;

namespace yutoVR.SphericalMovieEditor
{
    public enum Codec
    {
        H264,
        H265,
        H264_NVENC,
        H265_NVENC
    }

    public class VideoRecorder : MonoBehaviour
    {
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

        static void RemoveImages()
        {
            if (Directory.Exists(PathProvider.WorkDir))
            {
                Directory.Delete(PathProvider.WorkDir, true);
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
            image.FileNameGenerator.Leaf = PathProvider.WorkDir;
            settings.AddRecorderSettings(image);

            controller = new RecorderController(settings);

            frameCount = (long)video.frameCount;
            video.sendFrameReadyEvents = true;
            video.started += VideoOnStarted;
            video.frameReady += VideoOnFrameReady;
            video.Play();
        }

        static void VideoOnStarted(VideoPlayer source)
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
                    VideoEncoder.EncodeToVideo(video.clip, codec, fileName, crf, path);
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

        void OnApplicationQuit()
        {
            controller?.StopRecording();
        }

        void Reset()
        {
            if (!video) video = GetComponent<VideoPlayer>();
        }
    }
}
