using UnityEditor;
using UnityEngine;

namespace yutoVR.SphericalMovieEditor
{
    public class VideoRecorderBridge : MonoBehaviour
    {
        RecorderOptions options;

        void Start()
        {
            options = AssetDatabase.LoadAssetAtPath<RecorderOptions>(PathProvider.OptionPath);
            if (!options) return;

            if (options.startRecordingOnEnterPlayMode)
            {
                SphericalMovieEditor.Current.UseOriginalClip();
                FrameCapturer.StartCapturing();
            }
        }
        
        // TODO Timeline が終わっているかどうかを教えてあげられるようにする

        void OnApplicationQuit()
        {
            if (options)
            {
                options.startRecordingOnEnterPlayMode = false;
                EditorUtility.SetDirty(options);
                AssetDatabase.SaveAssets();
            }
        }
    }
}
