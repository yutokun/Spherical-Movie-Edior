using System;
using UnityEngine;

namespace yutoVR.SphericalMovieEditor
{
    [Serializable]
    public class RecorderOptions : ScriptableObject
    {
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
    }
}
