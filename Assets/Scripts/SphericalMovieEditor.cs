using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using UnityEngine.Video;

namespace yutoVR.SphericalMovieEditor
{
    public class SphericalMovieEditor : MonoBehaviour
    {
        public static SphericalMovieEditor Current { get; private set; }

        [SerializeField]
        VideoClip clip;

        [SerializeField]
        bool useProxy;

        VideoClip ProxyClip
        {
            get
            {
                var proxyPath = PathProvider.GetProxyPathRelative(clip);
                return AssetDatabase.LoadAssetAtPath<VideoClip>(proxyPath);
            }
        }

        VideoPlayer player;

        void Awake()
        {
            Current = this;
        }

        void OnValidate()
        {
            player = GetComponentInChildren<VideoPlayer>();
            if (!player) return;

            SetClip(player);
        }

        async void SetClip(VideoPlayer player)
        {
            if (clip == null)
            {
                player.clip = clip;
                return;
            }

            if (useProxy && !ProxyClip)
            {
                await VideoEncoder.EncodeProxy(clip);
                AssetDatabase.Refresh();
            }

            player.clip = useProxy ? ProxyClip : clip;

            SetTimelineClipLength();
        }

        void SetTimelineClipLength()
        {
            var director = GetComponentInChildren<PlayableDirector>();
            var timeline = director.playableAsset as TimelineAsset;
            var timelineClip = timeline.GetOutputTracks().First(t => t.GetType() == typeof(VideoPlayerTrackAsset))
                                       .GetClips().First(c => c.asset.GetType() == typeof(VideoPlayerPlayableAsset));
            timelineClip.duration = player.clip.length;
        }

        public void UseOriginalClip()
        {
            player.clip = clip;
        }
    }

    [CustomEditor(typeof(SphericalMovieEditor))]
    public class SphericalMovieEditorInspector : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            EditorGUILayout.Space();
            if (GUILayout.Button("Open Timeline"))
            {
                EditorApplication.ExecuteMenuItem("Window/Sequencing/Timeline");
                var directors = FindObjectsOfType<PlayableDirector>();
                Selection.activeGameObject = directors.First(d => d.GetComponent<TimelinePlayer>()).gameObject;
            }
        }
    }
}
