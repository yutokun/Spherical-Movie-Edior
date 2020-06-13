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

        void Start()
        {
            director = GetComponent<PlayableDirector>();
            director.timeUpdateMode = DirectorUpdateMode.Manual;
        }

        void Update()
        {
            // Debug.Log($"isPlaying:{video.isPlaying.ToString()} isPaused:{video.isPaused.ToString()} isPrepared:{video.isPrepared.ToString()}");

            if (PlayingState.IsPreviewingInPlayMode)
            {
                if (video.isPlaying)
                {
                    // TODO キャッシュ
                    var tracks = (director.playableAsset as TimelineAsset)?.GetOutputTracks();
                    var track = tracks.First(t => t.name == "Video Preview");
                    var clip = track.GetClips().First(c => c.displayName == "VideoPlayerPlayableAsset");
                    var startTime = clip.start;

                    director.time = startTime + video.time;
                    director.Evaluate();
                }
                else if (director.time <= director.duration)
                {
                    // TODO ビデオ領域外の時のタイムライン駆動・DeferredEval のほうが良いとか？ある？
                    var frameDelta = 1f / video.frameRate;
                    director.time += frameDelta;
                    director.Evaluate();
                }
            }
        }
    }
}
