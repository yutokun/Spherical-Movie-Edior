using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Video;

public class VideoPlayerPlayableBehaviour : PlayableBehaviour
{
    public VideoPlayer video;
    public Material mat;

    public override void OnBehaviourPlay(Playable playable, FrameData info)
    {
        if (Application.isPlaying || video == null) return;

        video.Play();
        video.Pause();
    }

    public override void OnBehaviourPause(Playable playable, FrameData info)
    {
        if (Application.isPlaying) return;

        if (video != null) video.Stop();
    }

    public override void PrepareFrame(Playable playable, FrameData info)
    {
        if (Application.isPlaying || video == null) return;

        video.time = playable.GetTime();
        mat.mainTexture = video.texture;
    }
}
