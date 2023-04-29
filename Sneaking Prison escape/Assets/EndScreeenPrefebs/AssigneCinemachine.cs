using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using UnityEngine.Timeline;
using UnityEngine.Playables;

public class AssigneCinemachine : MonoBehaviour
{
    private CinemachineBrain _cinemachineBrain;
    private PlayableDirector _director;

    public string TimelineName_inhecricy,MainCamera_inhecricy;


    // Start is called before the first frame update
    void Start()
    {
        _cinemachineBrain = GameObject.Find(MainCamera_inhecricy).GetComponent<CinemachineBrain>();
        _director = GameObject.Find(TimelineName_inhecricy).GetComponent<PlayableDirector>();
        
        SetCMBrain();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetCMBrain()
    {
        var timelineAsset = _director.playableAsset as TimelineAsset;
        var trackList = timelineAsset.GetOutputTracks();
        foreach (var track in trackList)
        {
            if (track.name == "Cinemachine Track")
                _director.SetGenericBinding(track, _cinemachineBrain);
        }
    }
}
