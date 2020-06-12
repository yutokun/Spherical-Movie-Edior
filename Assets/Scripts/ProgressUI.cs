using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using yutoVR.SphericalMovieEditor;

public class ProgressUI : MonoBehaviour
{
    public static ProgressUI Current { get; private set; }

    [SerializeField]
    Canvas canvas;

    [SerializeField]
    Slider progressBar;

    [SerializeField]
    Text frames;

    [SerializeField]
    Text remaining;

    float previousFrameTime;
    readonly List<float> deltaTimes = new List<float>();

    void Awake()
    {
        Current = this;

        var options = AssetDatabase.LoadAssetAtPath<RecorderOptions>(PathProvider.OptionPath);
        if (!options)
        {
            canvas.enabled = false;
            return;
        }

        canvas.enabled = options.startRecordingOnEnterPlayMode;
    }

    public void SetProgress(long frame, ulong frameCount)
    {
        var progressRate = frame / (float)frameCount;
        progressBar.value = progressRate;

        frames.text = $"Frame {(frame + 1).ToString()}/{frameCount.ToString()}";

        var deltaTime = Time.unscaledTime - previousFrameTime;
        previousFrameTime = Time.unscaledTime;
        deltaTimes.Add(deltaTime);
        if (deltaTimes.Count > 20) deltaTimes.RemoveAt(0);
        var avgDeltaTime = deltaTimes.Average();
        var remainingTime = TimeSpan.FromSeconds((frameCount - (ulong)frame) * avgDeltaTime);
        remaining.text = $"Capture Remaining {remainingTime.ToString(@"hh\:mm\:ss")}";
    }
}
