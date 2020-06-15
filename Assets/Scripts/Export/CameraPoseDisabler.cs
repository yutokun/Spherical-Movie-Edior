using UnityEngine;
using UnityEngine.SpatialTracking;

namespace yutoVR.SphericalMovieEditor
{
    public class CameraPoseDisabler : MonoBehaviour
    {
        void Awake()
        {
            if (RecorderOptions.CurrentOptions.startRecordingOnEnterPlayMode)
            {
                GetComponent<TrackedPoseDriver>().enabled = false;
            }
        }
    }
}
