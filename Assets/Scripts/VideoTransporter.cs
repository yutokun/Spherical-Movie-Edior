using UnityEngine;
using UnityEngine.Timeline;
using UnityEngine.Video;

public class VideoTransporter : MonoBehaviour, ITimeControl
{
    [SerializeField]
    Material mat;

    [SerializeField]
    VideoPlayer video;

    void Update()
    {
        mat.mainTexture = video.texture;
    }

    public void SetTime(double time)
    {
        if (Application.isPlaying) return;

        video.time = time;
        mat.mainTexture = video.texture;
    }

    public void OnControlTimeStart()
    {
        if (Application.isPlaying) return;

        video.Play();
        video.Pause();
    }

    public void OnControlTimeStop()
    {
        if (Application.isPlaying) return;

        if (video != null) video.Stop();
    }
}
