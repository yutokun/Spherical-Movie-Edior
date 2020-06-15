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
            window.minSize = new Vector2(320, 374);
            window.maxSize = new Vector2(320, 374);
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
            RecorderInternalOptions.PresetId = EditorGUILayout.Popup("Preset", RecorderInternalOptions.PresetId, RecorderOptions.PresetNames);
            var options = RecorderOptions.CurrentOptions;

            EditorGUI.BeginChangeCheck();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Image Settings", EditorStyles.boldLabel);
            var width = EditorGUILayout.IntField("Width", options.Width);
            var height = EditorGUILayout.IntField("Height", options.Height);
            var mapSize = EditorGUILayout.IntField("Map Size", options.MapSize);
            var renderStereo = EditorGUILayout.Toggle("Render Stereo", options.renderStereo);
            var stereoSeparation = EditorGUILayout.FloatField("Stereo Separation", options.StereoSeparation);
            var intermediateFormat = EditorGUILayout.EnumPopup("Intermediate Format", options.IntermediateFormat);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Encoder Settings", EditorStyles.boldLabel);
            var codec = EditorGUILayout.EnumPopup("Codec", options.Codec);
            var crf = EditorGUILayout.IntSlider("CRF", options.Crf, 0, 51);
            var maxBitrate = EditorGUILayout.FloatField("Max Bitrate (Mbps)", options.MaxBitRate);
            var fileName = EditorGUILayout.TextField("File Name", options.FileName);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Behaviour", EditorStyles.boldLabel);
            var frameTimeout = EditorGUILayout.FloatField("Frame Timeout", options.FrameTimeout);

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(options, "Change Recording Option");
                options.Width = width;
                options.Height = height;
                options.MapSize = mapSize;
                options.renderStereo = renderStereo;
                options.StereoSeparation = stereoSeparation;
                options.IntermediateFormat = (ImageRecorderSettings.ImageRecorderOutputFormat)intermediateFormat;
                options.Codec = (Codec)codec;
                options.Crf = crf;
                options.MaxBitRate = maxBitrate;
                options.FileName = fileName;
                options.FrameTimeout = frameTimeout;
                EditorUtility.SetDirty(options);
            }

            EditorGUILayout.Space();
            if (GUILayout.Button("Capture and Encode"))
            {
                FrameCapturer.Export();
            }

            if (GUILayout.Button("Encode Only"))
            {
                var director = FindObjectOfType<PlayableDirector>();
                var tracks = (director.playableAsset as TimelineAsset)?.GetOutputTracks();
                var track = tracks.First(t => t.name == "Video Preview");
                var clip = track.GetClips().First(c => c.displayName == "VideoPlayerPlayableAsset");
                VideoEncoder.Encode(clip.start);
            }
        }

        void Update()
        {
            Repaint();
        }
    }
}
