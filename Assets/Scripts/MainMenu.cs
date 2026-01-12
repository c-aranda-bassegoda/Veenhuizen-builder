using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public class MainMenu : MonoBehaviour
{
    [SerializeField] VideoPlayer videoPlayer;
    bool videoPlaying;
    private void Start()
    {
        videoPlayer.loopPointReached += OnVideoFinished;
    }
    public void PlayGame()
    {
        videoPlayer.gameObject.SetActive(true);
        videoPlaying = true;
        videoPlayer.Play();
        //SceneManager.LoadSceneAsync("TutorialScene");
    }

    void OnVideoFinished(VideoPlayer vp)
    {
        SceneManager.LoadSceneAsync("TutorialScene");
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    void OnDestroy()
    {
        videoPlayer.loopPointReached -= OnVideoFinished;
    }
}
