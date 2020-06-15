using UnityEditor;
using UnityEngine;

namespace yutoVR.SphericalMovieEditor
{
    public class VideoRecorderBridge : MonoBehaviour
    {
        void Start()
        {
            if (RecorderInternalOptions.StartRecordingOnEnterPlayMode)
            {
                SphericalMovieEditor.Current.UseOriginalClip();
                FrameCapturer.StartCapturing();
            }
        }

        void OnApplicationQuit()
        {
            RecorderInternalOptions.StartRecordingOnEnterPlayMode = false;
            EditorUtility.SetDirty(RecorderOptions.CurrentOptions);
            AssetDatabase.SaveAssets();
        }
    }
}
