using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Recorder;
using UnityEngine;

namespace yutoVR.SphericalMovieEditor
{
    [Serializable]
    public class RecorderOptions : ScriptableObject
    {
        // TODO presetId くらいは保存しようかな？
        public static List<RecorderOptions> Presets { get; private set; } = new List<RecorderOptions>();
        public static string[] PresetNames { get; private set; }
        public static int presetId;
        public static RecorderOptions CurrentOptions
        {
            get
            {
                if (presetId < Presets.Count) return Presets[presetId];

                LoadPresets();
                if (Presets.Count > 0)
                {
                    presetId = 0;
                    return Presets[0];
                }

                CreateDefaultOptions();
                LoadPresets();
                return Presets[0];
            }
        }

        public static void LoadPresets()
        {
            Presets.Clear();
            Directory.CreateDirectory(PathProvider.OptionDir);
            var assets = Directory.GetFiles(PathProvider.OptionDir);
            foreach (var asset in assets)
            {
                var preset = AssetDatabase.LoadAssetAtPath<RecorderOptions>(asset);
                if (preset == null) continue;
                Presets.Add(preset);
            }

            if (Presets.Count == 0)
            {
                var options = CreateDefaultOptions();
                Presets.Add(options);
            }

            PresetNames = Presets.Select(o => o.name).ToArray();
        }

        public static RecorderOptions CreateDefaultOptions()
        {
            Directory.CreateDirectory(PathProvider.OptionDir);
            var options = CreateInstance<RecorderOptions>();
            AssetDatabase.CreateAsset(options, PathProvider.DefaultOptionPath);
            AssetDatabase.Refresh();
            return AssetDatabase.LoadAssetAtPath<RecorderOptions>(PathProvider.DefaultOptionPath);
        }

        [HideInInspector]
        public bool startRecordingOnEnterPlayMode;

        [SerializeField, Header("Image Settings")]
        int height = 4096;

        public int Height
        {
            get => height;
            set => height = value;
        }

        [SerializeField]
        int width = 4096;

        public int Width
        {
            get => width;
            set => width = value;
        }

        [SerializeField]
        int mapSize = 4096;

        public int MapSize
        {
            get => mapSize;
            set => mapSize = value;
        }

        public bool renderStereo = true;

        [SerializeField]
        float stereoSeparation = 0.065f;

        public float StereoSeparation
        {
            get => stereoSeparation;
            set => stereoSeparation = value;
        }

        [SerializeField]
        ImageRecorderSettings.ImageRecorderOutputFormat intermediateFormat = ImageRecorderSettings.ImageRecorderOutputFormat.JPEG;

        public ImageRecorderSettings.ImageRecorderOutputFormat IntermediateFormat
        {
            get => intermediateFormat;
            set => intermediateFormat = value;
        }

        [SerializeField, Header("Encode Settings")]
        Codec codec;

        public Codec Codec
        {
            get => codec;
            set => codec = value;
        }

        [SerializeField, Range(0, 51)]
        int crf = 23;

        public int Crf
        {
            get => crf;
            set => crf = value;
        }

        [SerializeField]
        string fileName = "encoded";

        public string FileName
        {
            get => fileName;
            set => fileName = value;
        }

        [SerializeField, Header("Behaviour")]
        float frameTimeout = 5f;

        public float FrameTimeout
        {
            get => frameTimeout;
            set => frameTimeout = value;
        }
    }
}
