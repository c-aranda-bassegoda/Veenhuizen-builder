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
        if (buildingSystem != null) buildingSystem.isTutorial = true;
        else Debug.LogWarning("building system null");

        if (director == null)
            Debug.LogError($"Director NOT assigned on {gameObject.name}", this);
    }

    public void Play()
    {
        Debug.Log("Play Director");
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
