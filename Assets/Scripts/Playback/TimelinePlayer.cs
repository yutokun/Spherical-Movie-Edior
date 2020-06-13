using System.Linq;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using UnityEngine.Video;

namespace yutoVR.SphericalMovieEditor
{
    [RequireComponent(typeof(PlayableDirector))]
    public class TimelinePlayer : MonoBehaviour
    {
        [SerializeField]
        VideoPlayer video;

        PlayableDirector director;
        TimelineClip clip;
        float frameDelta;

        void Start()
        {
            director = GetComponent<PlayableDirector>();
            director.timeUpdateMode = DirectorUpdateMode.Manual;

            var tracks = (director.playableAsset as TimelineAsset)?.GetOutputTracks();
            var track = tracks.First(t => t.name == "Video Preview");
            clip = track.GetClips().First(c => c.displayName == "VideoPlayerPlayableAsset");

            frameDelta = 1f / video.frameRate;
        }

        void Update()
        {
            // Debug.Log($"isPlaying:{video.isPlaying.ToString()} isPaused:{video.isPaused.ToString()} isPrepared:{video.isPrepared.ToString()}");

            if (PlayingState.IsPreviewingInPlayMode)
            {
                if (video.isPlaying)
                {
                    director.time = clip.start + video.time;
                }
                else if (director.time <= director.duration)
                {
                    director.time += frameDelta;
                }

                director.Evaluate();
            }
        }
    }
}
