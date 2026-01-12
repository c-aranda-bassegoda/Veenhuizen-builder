using UnityEngine;
using UnityEngine.Playables;

public class TimelineTutorialController : MonoBehaviour
{

    public void PauseTimeline()
    {
        TimelineManager.Instance.Pause();
    }

    public void PlayTimeline()
    {
        TimelineManager.Instance.Play();
    }
}
