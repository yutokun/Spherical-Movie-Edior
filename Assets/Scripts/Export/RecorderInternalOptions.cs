using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace yutoVR.SphericalMovieEditor
{
    [Serializable]
    public class RecorderInternalOptions : ScriptableObject
    {
        static RecorderInternalOptions options;

        public static void Load()
        {
            Directory.CreateDirectory(PathProvider.InternalOptionDir);
            options = AssetDatabase.LoadAssetAtPath<RecorderInternalOptions>(PathProvider.InternalOptionPath);
            if (options) return;

            options = CreateInstance<RecorderInternalOptions>();
            AssetDatabase.CreateAsset(options, PathProvider.InternalOptionPath);
            AssetDatabase.Refresh();
        }

        int presetId;
        public static int PresetId
        {
            get
            {
                if (!options) Load();
                return options.presetId;
            }
            set
            {
                if (!options) Load();
                options.presetId = value;
                EditorUtility.SetDirty(options);
            }
        }

        bool startRecordingOnEnterPlayMode;
        public static bool StartRecordingOnEnterPlayMode
        {
            get
            {
                if (!options) Load();
                return options.startRecordingOnEnterPlayMode;
            }
            set
            {
                if (!options) Load();
                options.startRecordingOnEnterPlayMode = value;
                EditorUtility.SetDirty(options);
            }
        }
    }
}
