using UnityEngine;
using UnityEngine.Video;

public class VideoFrameGetter : MonoBehaviour
{
    [SerializeField]
    VideoPlayer video;

    long frame = 1, frameCount;
    bool isPlayable;

    void Start()
    {
        frameCount = (long)video.frameCount;
        video.sendFrameReadyEvents = true;
        video.started += VideoOnStarted;
        video.frameReady += VideoOnFrameReady;
        video.Prepare();
        video.Play();
    }

    void VideoOnStarted(VideoPlayer source)
    {
        Debug.Log("ON STARTED");
        source.Pause();
    }

    void VideoOnFrameReady(VideoPlayer source, long frameidx)
    {
        Debug.Log("ON FRAME READY");
        Next();
    }

    // void Update()
    // {
    //     if (isPlayable) Next();
    // }

    /// <summary>
    /// Show next frame.
    /// </summary>
    /// <returns>Return true when another frame exists.</returns>
    public bool Next()
    {
        video.frame = frame++;
        return frame <= frameCount;
    }
}
