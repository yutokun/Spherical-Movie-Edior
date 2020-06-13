using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using UnityEngine.Video;

namespace yutoVR.SphericalMovieEditor
{
    [RequireComponent(typeof(PlayableDirector))]
    public class TimelinePlayer : MonoBehaviour
    {
        public static TimelinePlayer Current { get; private set; }

        [SerializeField]
        VideoPlayer video;

        PlayableDirector director;
        TimelineClip clip;
        float frameDelta;
        bool videoFrameIsRendered;
        float timePreviousCaptured;
        bool timedUp;

        void Awake()
        {
            Current = this;
        }

        void Start()
        {
            director = GetComponent<PlayableDirector>();
            director.timeUpdateMode = DirectorUpdateMode.Manual;

            var tracks = (director.playableAsset as TimelineAsset)?.GetOutputTracks();
            var track = tracks.First(t => t.name == "Video Preview");
            clip = track.GetClips().First(c => c.displayName == "VideoPlayerPlayableAsset");

            frameDelta = 1f / video.frameRate;
        }

        async void Update()
        {
            // Debug.Log($"isPlaying:{video.isPlaying.ToString()} isPaused:{video.isPaused.ToString()} isPrepared:{video.isPrepared.ToString()}");

            if (PlayingState.IsPreviewingInPlayMode)
            {
                if (video.isPlaying)
                {
                    director.time = clip.start + video.time;
                }
                else if (director.time <= director.duration)
                {
                    director.time += frameDelta;
                }

                director.Evaluate();
            }

            if (PlayingState.IsRecording)
            {
                await UniTask.WaitUntil(() => (Time.unscaledTime - timePreviousCaptured) > 5f);
                timedUp = true;
            }
        }

        // TODO もしかすると別のクラスが良いかも？ ログも FrameCapturer に戻したい。
        public async void PlayFrameByFrame()
        {
            // TODO とりあえず Timeline をフレーム単位で終わるまで再生して、キャプチャしてみよう
            Debug.Log("Start Capturing");
            director = GetComponent<PlayableDirector>();
            director.timeUpdateMode = DirectorUpdateMode.Manual;
            VideoPlayerPlayableBehaviour.OnVideoFrameIsRendered += VideoFrameIsRendered;
            frameDelta = 1f / video.frameRate;
            var frameCount = (ulong)(director.duration / frameDelta);
            ulong frame = 0;
            timePreviousCaptured = Time.unscaledTime;
            while (frame < frameCount)
            {
                ProgressUI.Current.SetProgress(frame, frameCount);
                director.Evaluate();
                if (VideoPlayerPlayableBehaviour.isInVideoTime)
                {
                    var timedUpTask = UniTask.WaitUntil(() => timedUp);
                    var rendered = UniTask.WaitUntil(() => videoFrameIsRendered);
                    await UniTask.WhenAny(timedUpTask, rendered);
                    videoFrameIsRendered = false;
                }

                if (timedUp)
                {
                    Debug.Log($"Capturing has stuck. Trying to read next frame: {(frame + 2).ToString()}.");
                }
                else
                {
                    await FrameCapturer.CaptureFrame();
                }

                timePreviousCaptured = Time.unscaledTime;
                timedUp = false;
                ++frame;
                director.time = frameDelta * frame;
            }

            VideoPlayerPlayableBehaviour.OnVideoFrameIsRendered -= VideoFrameIsRendered;
            Debug.Log("Finish Capturing");
            VideoEncoder.Encode();
            EditorApplication.ExitPlaymode();
        }

        void VideoFrameIsRendered() => videoFrameIsRendered = true;
    }
}
