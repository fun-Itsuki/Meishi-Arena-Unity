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

    [Header("Audio Source")]
    private AudioSource audioSource;

    void Awake()
    {
        // シングルトンパターンの実装
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            // AudioSourceコンポーネントを取得または追加
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
        }
        else
        {
            Destroy(gameObject);
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
