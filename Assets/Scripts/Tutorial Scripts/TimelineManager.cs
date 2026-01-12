using UnityEngine;
using UnityEngine.Playables;

public class TimelineManager : MonoBehaviour
{
    public static TimelineManager Instance;
    public PlayableDirector director;
    public GridBuildingSystem buildingSystem;

    private void Awake()
    {
        Instance = this;
        Debug.Log($"TimelineManager Awake on {gameObject.name}", this);
        buildingSystem.isTutorial = true;

        if (director == null)
            Debug.LogError($"Director NOT assigned on {gameObject.name}", this);
    }

    public void Play()
    {
        director.Play();
    }

    public void Pause()
    {
        director.Pause();
    }

    public void Resume()
    {
        director.Play();
    }
}
