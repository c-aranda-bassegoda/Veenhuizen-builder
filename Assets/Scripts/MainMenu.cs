using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public class MainMenu : MonoBehaviour
{
    [SerializeField] VideoPlayer videoPlayer;
    [SerializeField] GameObject skipButton;   

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
        StartCoroutine(EnableSkipButton());
        //SceneManager.LoadSceneAsync("TutorialScene");
    }

    void OnVideoFinished(VideoPlayer vp)
    {
        SceneManager.LoadSceneAsync("TutorialScene");
    }

    public void SkipIntro()
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

    IEnumerator EnableSkipButton()
    {
        yield return new WaitForSeconds(3);
        skipButton.SetActive(true);
    }
}
