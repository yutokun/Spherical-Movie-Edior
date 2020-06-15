using UnityEditor;
using UnityEngine;

namespace yutoVR.SphericalMovieEditor
{
    public class VideoRecorderBridge : MonoBehaviour
    {
        void Start()
        {
            if (RecorderOptions.CurrentOptions.startRecordingOnEnterPlayMode)
            {
                SphericalMovieEditor.Current.UseOriginalClip();
                FrameCapturer.StartCapturing();
            }
        }

        void OnApplicationQuit()
        {
            RecorderOptions.CurrentOptions.startRecordingOnEnterPlayMode = false;
            EditorUtility.SetDirty(RecorderOptions.CurrentOptions);
            AssetDatabase.SaveAssets();
        }
    }
}
