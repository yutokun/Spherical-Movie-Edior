using UnityEngine;
using UnityEngine.Video;

namespace yutoVR.SphericalMovieEditor
{
    public class VideoTransporter : MonoBehaviour
    {
        [SerializeField]
        Material mat;

        [SerializeField]
        VideoPlayer video;

        void Update()
        {
            mat.mainTexture = video.texture;
        }
    }
}
