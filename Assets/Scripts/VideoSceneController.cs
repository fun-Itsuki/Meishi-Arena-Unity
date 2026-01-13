using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public class VideoSceneController : MonoBehaviour
{
    [Header("Video")]
    [SerializeField] private VideoPlayer videoPlayer;

    [Header("Audio (Other sound)")]
    [SerializeField] private AudioSource bgmSource; // 別の音を流す

    [Header("Overlay")]
    [SerializeField] private GameObject blackPanel;

    [Header("Delay")]
    [SerializeField] private float delaySeconds = 0f;

    [Header("Next Scene")]
    [SerializeField] private string nextSceneName = "MainScene";

    private bool isSceneLoading = false;

    void Start()
    {
        if (blackPanel) blackPanel.SetActive(true);

        if (!videoPlayer)
        {
            Debug.LogError("VideoPlayer が設定されていません");
            return;
        }

        // ? 動画音を出さない（確実）
        videoPlayer.audioOutputMode = VideoAudioOutputMode.None;

        // ? 別音を流す
        if (bgmSource && !bgmSource.isPlaying)
            bgmSource.Play();

        videoPlayer.playOnAwake = false;
        videoPlayer.waitForFirstFrame = true;
        videoPlayer.skipOnDrop = true;

        videoPlayer.loopPointReached += _ => GoToNextScene();
        videoPlayer.prepareCompleted += _ => StartCoroutine(PlayAfterDelay());

        videoPlayer.Prepare();
    }

    void Update()
    {
        if (!isSceneLoading && Input.anyKeyDown)
            GoToNextScene();
    }

    private IEnumerator PlayAfterDelay()
    {
        if (delaySeconds > 0f)
            yield return new WaitForSeconds(delaySeconds);

        if (isSceneLoading) yield break;

        videoPlayer.Play();

        // 最初のフレームが出てから黒を消す（より確実）
        while (videoPlayer.frame < 0) yield return null;
        if (blackPanel) blackPanel.SetActive(false);
    }

    private void GoToNextScene()
    {
        if (isSceneLoading) return;
        isSceneLoading = true;

        if (videoPlayer && videoPlayer.isPlaying)
            videoPlayer.Stop();

        if (bgmSource && bgmSource.isPlaying)
            bgmSource.Stop();

        SceneManager.LoadScene(nextSceneName);
    }
}

