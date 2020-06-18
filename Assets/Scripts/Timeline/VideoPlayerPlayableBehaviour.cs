using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Video;

namespace yutoVR.SphericalMovieEditor
{
    public class VideoPlayerPlayableBehaviour : PlayableBehaviour
    {
        public VideoPlayer video;
        public Material mat;

        public static event Action OnVideoFrameIsRendered;
        public static bool isInVideoTime;

        public override void OnBehaviourPlay(Playable playable, FrameData info)
        {
            if (video == null) return;

            if (PlayingState.IsPreviewingInPlayMode || PlayingState.IsScrubbing || PlayingState.IsPlayingTimelineInEditor) video.Play();

            if (PlayingState.IsRecording)
            {
                isInVideoTime = true;
                video.sendFrameReadyEvents = true;
                video.frameReady += FrameReady;
                video.Prepare();
            }
        }

        public override void OnBehaviourPause(Playable playable, FrameData info)
        {
            if (video == null) return;

            if (PlayingState.IsPreviewingInPlayMode || PlayingState.IsPlayingTimelineInEditor) video.Stop();

            if (PlayingState.IsScrubbing)
            {
                var duringVideo = (playable.GetTime() / playable.GetDuration()) < 1d;
                if (duringVideo)
                {
                    video.Pause();
                }
                else
                {
                    video.Stop();
                }
            }

            if (PlayingState.IsRecording)
            {
                video.Stop();
                video.frameReady -= FrameReady;
                isInVideoTime = false;
            }
        }

        // TODO 整理
        public override async void PrepareFrame(Playable playable, FrameData info)
        {
            if (video == null) return;

            if (PlayingState.IsPlayingTimelineInEditor && !video.isPlaying)
            {
                video.time = playable.GetTime();
                video.Play();
            }

            if (PlayingState.IsScrubbing) video.time = playable.GetTime();
            if (PlayingState.IsRecording && video.isPrepared) video.time = playable.GetTime();

            mat.mainTexture = video.texture;

            if (PlayingState.IsScrubbing)
            {
                video.Play();
                await UniTask.Delay(300);
                video.Pause();
            }
        }

        void FrameReady(VideoPlayer source, long frameidx)
        {
            mat.mainTexture = video.texture;
            OnVideoFrameIsRendered?.Invoke();
        }
    }
}
