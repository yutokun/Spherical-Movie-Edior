using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Video;

[System.Serializable]
public class VideoPlayerPlayableAsset : PlayableAsset
{
    public ExposedReference<VideoPlayer> videoPlayer;
    public Material mat;

    VideoClip clip;

    public override Playable CreatePlayable(PlayableGraph graph, GameObject go)
    {
        var video = videoPlayer.Resolve(graph.GetResolver());
        clip = video.clip;
        var behaviour = new VideoPlayerPlayableBehaviour { video = video, mat = mat };
        return ScriptPlayable<VideoPlayerPlayableBehaviour>.Create(graph, behaviour);
    }

    public override double duration => Duration;
    double Duration => clip != null ? clip.length : 5;
}
