using UnityEditor;
using UnityEngine;
using UnityEngine.SpatialTracking;

namespace yutoVR.SphericalMovieEditor
{
    public class CameraPoseDisabler : MonoBehaviour
    {
        void Awake()
        {
            var options = AssetDatabase.LoadAssetAtPath<RecorderOptions>(PathProvider.OptionPath);
            if (!options) return;

            if (options.startRecordingOnEnterPlayMode)
            {
                GetComponent<TrackedPoseDriver>().enabled = false;
            }
        }
    }
}
