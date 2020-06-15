using System.Linq;
using UnityEngine;
using UnityEngine.Playables;

namespace yutoVR.SphericalMovieEditor
{
    public class PlayingState : MonoBehaviour
    {
        static PlayableDirector directorCache;
        static PlayableDirector Director
        {
            get
            {
                if (directorCache) return directorCache;
                var directors = FindObjectsOfType<PlayableDirector>();
                directorCache = directors.First(d => d.GetComponent<TimelinePlayer>());
                return directorCache;
            }
        }


        public static bool IsPreviewingInPlayMode => Application.isPlaying && !RecorderOptions.CurrentOptions.startRecordingOnEnterPlayMode;
        public static bool IsRecording => Application.isPlaying && RecorderOptions.CurrentOptions.startRecordingOnEnterPlayMode;
        public static bool IsScrubbing => !Application.isPlaying && Director.playableGraph.IsValid() && !Director.playableGraph.IsPlaying();
        public static bool IsPlayingTimelineInEditor => !Application.isPlaying && Director.playableGraph.IsValid() && Director.playableGraph.IsPlaying();
    }
}
