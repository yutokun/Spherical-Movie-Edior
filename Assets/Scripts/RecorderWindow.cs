using UnityEditor;
using UnityEngine;

namespace yutoVR.SphericalMovieEditor
{
    public class RecorderWindow : EditorWindow
    {
        static RecorderOptions options;

        [MenuItem("Movie/Export...")]
        public static void Create()
        {
            var window = GetWindow<RecorderWindow>("Spherical Movie Recorder");
            window.minSize = new Vector2(320, 254);
            window.maxSize = new Vector2(320, 254);
            window.autoRepaintOnSceneChange = true;

            options = AssetDatabase.LoadAssetAtPath<RecorderOptions>(PathProvider.OptionPath);

            if (options == null)
            {
                options = CreateInstance<RecorderOptions>();
                AssetDatabase.CreateAsset(options, PathProvider.OptionPath);
            }
        }

        void OnGUI()
        {
            if (options == null) options = AssetDatabase.LoadAssetAtPath<RecorderOptions>(PathProvider.OptionPath);

            EditorGUI.BeginChangeCheck();

            EditorGUILayout.LabelField("Image Settings", EditorStyles.boldLabel);
            var height = EditorGUILayout.IntField("Height", options.Height);
            var width = EditorGUILayout.IntField("Width", options.Width);
            var mapSize = EditorGUILayout.IntField("Map Size", options.MapSize);
            var renderStereo = EditorGUILayout.Toggle("Render Stereo", options.renderStereo);
            var stereoSeparation = EditorGUILayout.FloatField("Stereo Separation", options.StereoSeparation);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Encode Settings", EditorStyles.boldLabel);
            var codec = EditorGUILayout.EnumPopup("Codec", options.Codec);
            var crf = EditorGUILayout.IntSlider("CRF", options.Crf, 0, 51);
            var fileName = EditorGUILayout.TextField("File Name", options.FileName);

            if (EditorGUI.EndChangeCheck())
            {
                options.Height = height;
                options.Width = width;
                options.MapSize = mapSize;
                options.renderStereo = renderStereo;
                options.StereoSeparation = stereoSeparation;
                options.Codec = (Codec)codec;
                options.Crf = crf;
                options.FileName = fileName;
                EditorUtility.SetDirty(options);
            }

            EditorGUILayout.Space();
            if (GUILayout.Button("Capture and Encode"))
            {
                VideoRecorder.Export();
            }

            if (GUILayout.Button("Encode Only"))
            {
                VideoRecorder.Encode();
            }
        }

        void Update()
        {
            Repaint();
        }
    }
}
