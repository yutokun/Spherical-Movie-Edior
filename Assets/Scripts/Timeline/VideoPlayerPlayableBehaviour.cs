using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Video;

public class VideoPlayerPlayableBehaviour : PlayableBehaviour
{
    public VideoPlayer video;
    public Material mat;

    public override void OnBehaviourPlay(Playable playable, FrameData info)
    {
        if (video == null) return;

        video.Play();
    }

    public override void OnBehaviourPause(Playable playable, FrameData info)
    {
        if (video == null) return;

        video.Stop();
    }

    public override void PrepareFrame(Playable playable, FrameData info)
    {
        if (video == null) return;

        if (!Application.isPlaying) video.time = playable.GetTime();
        mat.mainTexture = video.texture;
    }
}
