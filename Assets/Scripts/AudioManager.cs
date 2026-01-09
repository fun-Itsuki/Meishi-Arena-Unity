using UnityEngine;

/// <summary>
/// シングルトンパターンで音声を一元管理するクラス
/// </summary>
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Clips")]
    [Tooltip("ボタンクリック時の効果音")]
    public AudioClip buttonClickSound;
    
    [Tooltip("バトル開始時の効果音")]
    public AudioClip battleStartSound;

    [Header("BGM")]
    [Tooltip("背景音楽のクリップ")]
    public AudioClip bgmClip;
    private AudioSource bgmSource;
    private AudioSource audioSource;

    void Awake()
    {
        // シングルトンパターンの実装
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            // AudioSourceコンポーネントを取得または追加（効果音用）
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }

            // BGM用のAudioSourceを追加
            bgmSource = gameObject.AddComponent<AudioSource>();
            bgmSource.loop = true;
            bgmSource.playOnAwake = false;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// BGMを再生
    /// </summary>
    /// <param name="clip">再生するクリップ。nullの場合はデフォルトのbgmClipを再生</param>
    public void PlayBGM(AudioClip clip = null)
    {
        AudioClip clipToPlay = clip != null ? clip : bgmClip;
        
        if (clipToPlay == null)
        {
            Debug.LogWarning("BGM clip is not assigned!");
            return;
        }

        if (bgmSource.clip == clipToPlay && bgmSource.isPlaying)
        {
            return; // 既に同じ曲が再生中なら何もしない
        }

        bgmSource.clip = clipToPlay;
        bgmSource.Play();
        Debug.Log($"Playing BGM: {clipToPlay.name}");
    }

    /// <summary>
    /// BGMを停止
    /// </summary>
    public void StopBGM()
    {
        if (bgmSource != null && bgmSource.isPlaying)
        {
            bgmSource.Stop();
            Debug.Log("BGM stopped");
        }
    }

    /// <summary>
    /// ボタンクリック音を再生
    /// </summary>
    public void PlayButtonClick()
    {
        if (buttonClickSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(buttonClickSound);
            Debug.Log("Playing button click sound");
        }
        else
        {
            Debug.LogWarning("Button click sound or AudioSource is not assigned!");
        }
    }

    /// <summary>
    /// バトル開始音を再生
    /// </summary>
    public void PlayBattleStart()
    {
        if (battleStartSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(battleStartSound);
            Debug.Log("Playing battle start sound");
        }
        else
        {
            Debug.LogWarning("Battle start sound or AudioSource is not assigned!");
        }
    }
}
