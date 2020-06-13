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

        public override void OnBehaviourPlay(Playable playable, FrameData info)
        {
            if (video == null) return;

            if (PlayingState.IsPreviewingInPlayMode || PlayingState.IsScrubbing || PlayingState.IsPlayingTimelineInEditor) video.Play();
        }

        public override void OnBehaviourPause(Playable playable, FrameData info)
        {
            if (video == null) return;

            if (PlayingState.IsPreviewingInPlayMode || PlayingState.IsScrubbing || PlayingState.IsPlayingTimelineInEditor) video.Stop();
        }

        public override async void PrepareFrame(Playable playable, FrameData info)
        {
            if (video == null) return;

            if (PlayingState.IsPlayingTimelineInEditor && !video.isPlaying)
            {
                video.time = playable.GetTime();
                video.Play();
            }

            if (PlayingState.IsScrubbing) video.time = playable.GetTime();

            mat.mainTexture = video.texture;

            if (PlayingState.IsScrubbing)
            {
                video.Play();
                await UniTask.Delay(300);
                video.Pause();
            }
        }
    }
}
