using UnityEngine;
using UnityEngine.Video;

[RequireComponent(typeof(VideoPlayer))]
public class VideoTransporter : MonoBehaviour
{
    [SerializeField]
    Material mat;

    VideoPlayer video;

    void Start()
    {
        video = GetComponent<VideoPlayer>();
    }

    void Update()
    {
        mat.mainTexture = video.texture;
    }
}
