using UnityEngine;

public class PresidentAudioManager : MonoBehaviour
{
    public static PresidentAudioManager Instance { get; private set; }

    [Header("BGM")]
    [Tooltip("社長戦：専用BGM")]
    public AudioClip presidentBGM;
    [Tooltip("社長戦：3連Perfect達成時の勝利BGM")]
    public AudioClip presidentVictoryBGM;

    [Header("SFX")]
    [Tooltip("社長戦：Space成功時の効果音")]
    public AudioClip presidentSuccessSound;
    [Tooltip("社長戦：Space失敗時の効果音")]
    public AudioClip presidentFailureSound;

    private AudioSource bgmSource;
    private AudioSource sfxSource;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // BGM用のAudioSource
        bgmSource = gameObject.AddComponent<AudioSource>();
        bgmSource.loop = true;
        bgmSource.playOnAwake = false;

        // SFX用のAudioSource
        sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.playOnAwake = false;
    }

    public void PlayBGM(AudioClip clip)
    {
        if (clip == null) return;
        if (bgmSource.clip == clip && bgmSource.isPlaying) return;

        bgmSource.clip = clip;
        bgmSource.Play();
    }

    public void StopBGM()
    {
        bgmSource.Stop();
    }

    public void PlaySFX(AudioClip clip)
    {
        if (clip == null) return;
        sfxSource.PlayOneShot(clip);
    }
}
