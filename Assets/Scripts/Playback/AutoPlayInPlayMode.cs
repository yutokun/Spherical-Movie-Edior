using UnityEditor;
using UnityEngine;
using UnityEngine.Video;

namespace yutoVR.SphericalMovieEditor
{
    public class AutoPlayInPlayMode : MonoBehaviour
    {
        void Start()
        {
            var options = AssetDatabase.LoadAssetAtPath<RecorderOptions>(PathProvider.OptionPath);
            if (!options) return;

            if (!options.startRecordingOnEnterPlayMode)
            {
                GetComponent<VideoPlayer>().Play();
            }
        }
    }
}
