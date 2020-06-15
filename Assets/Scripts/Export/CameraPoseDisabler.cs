using UnityEngine;
using UnityEngine.SpatialTracking;

namespace yutoVR.SphericalMovieEditor
{
    public class CameraPoseDisabler : MonoBehaviour
    {
        void Awake()
        {
            if (RecorderInternalOptions.StartRecordingOnEnterPlayMode)
            {
                GetComponent<TrackedPoseDriver>().enabled = false;
            }
        }
    }
}
