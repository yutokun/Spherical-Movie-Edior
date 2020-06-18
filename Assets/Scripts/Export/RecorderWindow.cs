using System.Linq;
using UnityEditor;
using UnityEditor.Recorder;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace yutoVR.SphericalMovieEditor
{
    public class RecorderWindow : EditorWindow
    {
        [MenuItem("Movie/Export...")]
        public static void Create()
        {
            var window = GetWindow<RecorderWindow>("Spherical Movie Editor");
            window.minSize = new Vector2(400, 410);
            window.maxSize = new Vector2(400, 410);
            window.autoRepaintOnSceneChange = true;

            RecorderOptions.LoadPresets();
        }

        void OnProjectChange()
        {
            RecorderOptions.LoadPresets();
        }

        void OnGUI()
        {
            EditorGUILayout.LabelField("Preset", EditorStyles.boldLabel);
            var options = RecorderOptions.CurrentOptions;
            RecorderInternalOptions.PresetId = EditorGUILayout.Popup("Preset", RecorderInternalOptions.PresetId, RecorderOptions.PresetNames);

            EditorGUI.BeginChangeCheck();

            var lockEdit = EditorGUILayout.Toggle("Edit Lock", options.lockEdit);

            using (new EditorGUI.DisabledGroupScope(lockEdit))
            {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Image Settings", EditorStyles.boldLabel);

                GUILayout.BeginHorizontal();
                EditorGUILayout.PrefixLabel("Dimension");
                var width = EditorGUILayout.IntField(options.Width);
                EditorGUILayout.LabelField("x", GUILayout.Width(10));
                var height = EditorGUILayout.IntField(options.Height);
                GUILayout.EndHorizontal();

                string[] mapSizeNames = { "Low 1024", "Medium 2048", "High 4096", "Insane 8192" };
                int[] mapSizeVariation = { 1024, 2048, 4096, 8192 };
                var mapSize = EditorGUILayout.IntPopup("Map Size", options.MapSize, mapSizeNames, mapSizeVariation);
                var renderStereo = EditorGUILayout.Toggle("Render Stereo", options.renderStereo);
                var stereoSeparation = EditorGUILayout.FloatField("Stereo Separation", options.StereoSeparation);
                var intermediateFormat = EditorGUILayout.EnumPopup("Intermediate Format", options.IntermediateFormat);

                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Encoder Settings", EditorStyles.boldLabel);
                var codec = EditorGUILayout.EnumPopup("Codec", options.Codec);
                var crf = EditorGUILayout.IntSlider("CRF", options.Crf, 0, 51);
                var maxBitrate = EditorGUILayout.IntField("Max Bitrate (Mbps)", options.MaxBitRate);
                var bufSize = EditorGUILayout.IntField("Buffer Size (Mbps)", options.BufSize);
                var fastStart = EditorGUILayout.Toggle("Fast Start", options.fastStart);
                var fileName = EditorGUILayout.TextField("File Name", options.FileName);

                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Behaviour", EditorStyles.boldLabel);
                var frameTimeout = EditorGUILayout.FloatField("Frame Timeout", options.FrameTimeout);

                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(options, "Change Recording Option");
                    options.lockEdit = lockEdit;
                    options.Width = width;
                    options.Height = height;
                    options.MapSize = mapSize;
                    options.renderStereo = renderStereo;
                    options.StereoSeparation = stereoSeparation;
                    options.IntermediateFormat = (ImageRecorderSettings.ImageRecorderOutputFormat)intermediateFormat;
                    options.Codec = (Codec)codec;
                    options.Crf = crf;
                    options.MaxBitRate = maxBitrate;
                    options.BufSize = bufSize;
                    options.fastStart = fastStart;
                    options.FileName = fileName;
                    options.FrameTimeout = frameTimeout;
                    EditorUtility.SetDirty(options);
                }
            }

            EditorGUILayout.Space();
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Capture and Encode"))
                {
                    FrameCapturer.Export();
                }

                if (GUILayout.Button("Encode Only"))
                {
                    var director = FindObjectOfType<PlayableDirector>();
                    var tracks = (director.playableAsset as TimelineAsset)?.GetOutputTracks();
                    var track = tracks.First(t => t.GetType() == typeof(VideoPlayerTrackAsset));
                    var clip = track.GetClips().First(c => c.asset.GetType() == typeof(VideoPlayerPlayableAsset));
                    VideoEncoder.Encode(clip.start);
                }
            }
        }

        void Update()
        {
            Repaint();
        }
    }
}
