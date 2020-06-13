using UnityEditor;
using UnityEngine;
using UnityEngine.Video;

namespace yutoVR.SphericalMovieEditor
{
    public class VideoRecorderBridge : MonoBehaviour
    {
        RecorderOptions options;
        VideoPlayer player;

        void Start()
        {
            options = AssetDatabase.LoadAssetAtPath<RecorderOptions>(PathProvider.OptionPath);
            if (!options) return;

            if (options.startRecordingOnEnterPlayMode)
            {
                SphericalMovieEditor.Current.UseOriginalClip();
                player = GetComponent<VideoPlayer>();
                VideoRecorder.StartRecording();
            }
        }

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
