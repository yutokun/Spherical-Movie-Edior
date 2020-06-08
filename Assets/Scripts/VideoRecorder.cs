﻿using System;
using System.Collections;
using System.IO;
using UnityEditor.Recorder;
using UnityEditor.Recorder.Input;
using UnityEngine;
using UnityEngine.Video;

public class VideoRecorder : MonoBehaviour
{
    RecorderController controller;

    long frame = 1, frameCount;
    bool nextFrameExists = true;

    [SerializeField]
    VideoPlayer video;

    // TODO オーバーライドできるように
    // [SerializeField]
    // float frameRate = 30f;

    [SerializeField, Header("Export Settings")]
    int height = 4096;

    [SerializeField]
    int width = 4096;

    [SerializeField]
    bool renderStereo = true;

    [SerializeField]
    float stereoSeparation = 0.065f;

    [SerializeField, Header("Debug")]
    float interval = 0.5f;

    void Start()
    {
        var settings = ScriptableObject.CreateInstance<RecorderControllerSettings>();
        settings.FrameRate = (float)video.clip.frameRate;
        settings.CapFrameRate = true;
        settings.FrameRatePlayback = FrameRatePlayback.Constant;

        var image = ScriptableObject.CreateInstance<ImageRecorderSettings>();
        image.imageInputSettings = new Camera360InputSettings
        {
            Source = ImageSource.MainCamera,
            MapSize = 4096,
            OutputHeight = height,
            OutputWidth = width,
            RenderStereo = renderStereo,
            StereoSeparation = stereoSeparation,
        };
        image.OutputFormat = ImageRecorderSettings.ImageRecorderOutputFormat.PNG;
        image.FileNameGenerator.Root = OutputPath.Root.Absolute;
        image.FileNameGenerator.Leaf = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "image sequence");
        image.FileNameGenerator.FileName = "image_<Frame>";
        settings.AddRecorderSettings(image);

        controller = new RecorderController(settings);

        frameCount = (long)video.frameCount;
        video.sendFrameReadyEvents = true;
        video.started += VideoOnStarted;
        video.frameReady += VideoOnFrameReady;
        video.Play();
    }

    void VideoOnStarted(VideoPlayer source)
    {
        source.Pause();
    }

    void VideoOnFrameReady(VideoPlayer source, long frameidx)
    {
        StartCoroutine(RecordFrame((int)frameidx));
    }

    IEnumerator RecordFrame(int frameId)
    {
        controller.Settings.SetRecordModeToSingleFrame(frameId);
        Debug.Log($"Frame ID: {frame.ToString()}");
        controller.PrepareRecording();
        controller.StartRecording();
        yield return new WaitForSeconds(interval);
        controller.StopRecording();
        if (nextFrameExists) nextFrameExists = Next();
    }

    /// <summary>
    /// Show next frame.
    /// </summary>
    /// <returns>Return true when another frame exists.</returns>
    bool Next()
    {
        video.frame = frame++;
        return frame <= frameCount;
    }

    void OnApplicationQuit()
    {
        controller?.StopRecording();
    }
}
