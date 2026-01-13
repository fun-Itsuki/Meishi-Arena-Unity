using UnityEngine;
using UnityEngine.Video;

public class VideoControlUI : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private VideoPlayer videoPlayer;
    [SerializeField] private GameObject videoImage;    // VideoRawImage
    [SerializeField] private GameObject playButton;    // BtnPlay

    [Header("Hide while playing")]
    [SerializeField] private GameObject pngOverlayRoot; // ← PngOverlay を入れる

    [Header("BGM to resume (IMPORTANT)")]
    [SerializeField] private AudioClip resumeBgmClip;  // titleBGM/mainBGM を入れる

    private bool bgmStoppedByVideo = false;

    void Reset()
    {
        videoPlayer = GetComponentInChildren<VideoPlayer>();
    }

    void OnEnable()
    {
        // Panelを開いた瞬間：BGMは触らない
        if (videoPlayer != null)
        {
            videoPlayer.Stop();
            videoPlayer.time = 0;

            videoPlayer.loopPointReached -= OnVideoFinished;
            videoPlayer.loopPointReached += OnVideoFinished;
        }

        if (videoImage != null) videoImage.SetActive(false);
        if (playButton != null) playButton.SetActive(true);

        // PNGは通常表示
        if (pngOverlayRoot != null) pngOverlayRoot.SetActive(true);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ClosePanel();
        }
    }

    void OnDisable()
    {
        StopVideoAndResumeBgm();
    }

    public void PlayFromStart()
    {
        if (videoPlayer == null) return;

        // 動画再生中はPNGを消す
        if (pngOverlayRoot != null) pngOverlayRoot.SetActive(false);

        // 動画表示＆ボタン非表示
        if (videoImage != null) videoImage.SetActive(true);
        if (playButton != null) playButton.SetActive(false);

        // BGM停止（ここでだけ）
        if (!bgmStoppedByVideo && AudioManager.Instance != null)
        {
            AudioManager.Instance.StopBGM();
            bgmStoppedByVideo = true;
        }

        videoPlayer.Stop();
        videoPlayer.time = 0;
        videoPlayer.Play();
    }

    public void ClosePanel()
    {
        StopVideoAndResumeBgm();
        gameObject.SetActive(false);
    }

    private void OnVideoFinished(VideoPlayer vp)
    {
        StopVideoAndResumeBgm();
    }

    private void StopVideoAndResumeBgm()
    {
        // 動画停止
        if (videoPlayer != null)
        {
            videoPlayer.Stop();
            videoPlayer.time = 0;
        }

        // 動画非表示＆ボタン復帰
        if (videoImage != null) videoImage.SetActive(false);
        if (playButton != null) playButton.SetActive(true);

        // PNGを戻す
        if (pngOverlayRoot != null) pngOverlayRoot.SetActive(true);

        // BGM復帰
        if (bgmStoppedByVideo && AudioManager.Instance != null)
        {
            if (resumeBgmClip != null) AudioManager.Instance.PlayBGM(resumeBgmClip);
            else Debug.LogWarning("VideoControlUI: resumeBgmClip is NOT assigned!");

            bgmStoppedByVideo = false;
        }
    }
}
