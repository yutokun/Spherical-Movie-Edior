using UnityEditor;
using UnityEngine;

namespace yutoVR.SphericalMovieEditor
{
    public class VideoRecorderBridge : MonoBehaviour
    {
        void Start()
        {
            if (RecorderOptions.Options.startRecordingOnEnterPlayMode)
            {
                SphericalMovieEditor.Current.UseOriginalClip();
                FrameCapturer.StartCapturing();
            }
        }

        // TODO Timeline が終わっているかどうかを教えてあげられるようにする

        void OnApplicationQuit()
        {
            RecorderOptions.Options.startRecordingOnEnterPlayMode = false;
            EditorUtility.SetDirty(RecorderOptions.Options);
            AssetDatabase.SaveAssets();
        }
    }
}
