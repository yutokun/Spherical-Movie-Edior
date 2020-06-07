using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Video;

[RequireComponent(typeof(PlayableDirector))]
public class TimelinePlayer : MonoBehaviour
{
    [SerializeField]
    VideoPlayer video;

    PlayableDirector director;

    void Start()
    {
        director = GetComponent<PlayableDirector>();
    }

    void Update()
    {
        director.time = video.time;
        director.Evaluate();
    }
}
