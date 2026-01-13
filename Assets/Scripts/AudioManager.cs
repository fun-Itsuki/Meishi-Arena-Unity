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
    [Tooltip("試合開始のゴング効果音（最初の2秒カウント開始時にのみ再生）")]
    public AudioClip battleGongSound;
    
    [Tooltip("1つ目のハートが減ったときの効果音（例: 「うっ！」）")]
    public AudioClip heartHurtSound1;

    [Tooltip("2つ目のハートが減ったときの効果音（例: 「ぐおっ！」）")]
    public AudioClip heartHurtSound2;
    
    [Tooltip("ライフが0になった時の効果音（例: 「ぎゃああーー！」）")]
    public AudioClip heartDeathSound;
    
    [Tooltip("正解時の効果音（例: クイズ正解5）")]
    public AudioClip correctAnswerSound;
    
    [Tooltip("役職が上がったときの効果音（例: 決定ボタンを押す10(1)）")]
    public AudioClip rankUpSound;

    [Tooltip("ダメージを受けた時のボイス（複数登録でランダム再生）")]
    public AudioClip[] damageVoices;

    [Tooltip("正解時に流す坂下ボイス（複数登録でランダム再生。例：うまい！、うまい～、お見事②、まだまだ、やるじゃないか）")]
    public AudioClip[] sakashitaCorrectVoices;

    [Header("BGM")]
    [Tooltip("背景音楽のクリップ")]
    public AudioClip bgmClip;
    [Tooltip("タイトルシーン用のBGM（例: 古代神殿_loop_free））")]
    public AudioClip titleBGM;
    [Tooltip("メインシーン用のBGM（例: main.mp3）")]
    public AudioClip mainBGM;
    [Tooltip("聖戦（バトル）用のBGM（例: seisen.mp3）")]
    public AudioClip seisenBGM;
    [Range(0f, 1f)]
    [Tooltip("BGMの音量（0～1）")]
    public float bgmVolume = 0.8f;
    [Range(0f, 1f)]
    [Tooltip("聖戦BGM（seisen）の音量（0～1）")]
    public float seisenBGMVolume = 0.5f;
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

    private void Start()
    {
        // 可能であれば、Inspectorで割り当てられていない場合はログで通知する
        if (titleBGM == null)
        {
            Debug.Log("AudioManager: titleBGM is not assigned. Assign '古代神殿_loop_free' in the inspector if you want automatic Title BGM.");
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
        
        // seisen の場合は seisenBGMVolume を使用、それ以外は bgmVolume を使用
        if (clipToPlay == seisenBGM)
        {
            bgmSource.volume = seisenBGMVolume;
            Debug.Log($"Playing BGM: {clipToPlay.name} with seisen volume: {seisenBGMVolume}");
        }
        else
        {
            bgmSource.volume = bgmVolume;
            Debug.Log($"Playing BGM: {clipToPlay.name} with volume: {bgmVolume}");
        }
        
        bgmSource.Play();
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

    /// <summary>
    /// 試合開始のゴングを再生（カウントダウン開始時に1回だけ）
    /// </summary>
    public void PlayBattleGong()
    {
        if (battleGongSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(battleGongSound);
            Debug.Log("Playing battle gong sound");
        }
        else
        {
            Debug.LogWarning("battleGongSound or AudioSource is not assigned!");
        }
    }

    /// <summary>
    /// ライフ0時の叫び効果音を再生
    /// </summary>
    public void PlayHeartDeath()
    {
        if (heartDeathSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(heartDeathSound);
            Debug.Log("Playing heart-death sound");
        }
        else
        {
            Debug.LogWarning("heartDeathSound or AudioSource is not assigned!");
        }
    }

    /// <summary>
    /// 1つ目のハート喪失時の効果音を再生
    /// </summary>
    public void PlayHeartHurt1()
    {
        if (heartHurtSound1 != null && audioSource != null)
        {
            audioSource.PlayOneShot(heartHurtSound1);
            Debug.Log("Playing heart-hurt-1 sound");
        }
        else
        {
            Debug.LogWarning("heartHurtSound1 or AudioSource is not assigned!");
        }
    }

    /// <summary>
    /// 2つ目のハート喪失時の効果音を再生
    /// </summary>
    public void PlayHeartHurt2()
    {
        if (heartHurtSound2 != null && audioSource != null)
        {
            audioSource.PlayOneShot(heartHurtSound2);
            Debug.Log("Playing heart-hurt-2 sound");
        }
        else
        {
            Debug.LogWarning("heartHurtSound2 or AudioSource is not assigned!");
        }
    }

    /// <summary>
    /// 正解（ハートが減らなかった）時の効果音を再生
    /// クイズ正解5 + 坂下ボイスをランダムに再生
    /// </summary>
    public void PlayCorrectSound()
    {
        if (correctAnswerSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(correctAnswerSound);
            Debug.Log("Playing correct-answer sound");
        }
        else
        {
            Debug.LogWarning("correctAnswerSound or AudioSource is not assigned!");
        }

        // 坂下ボイスをランダムに再生
        PlayRandomSakashitaVoice();
    }

    /// <summary>
    /// 坂下ボイスをランダムに再生
    /// </summary>
    private void PlayRandomSakashitaVoice()
    {
        if (sakashitaCorrectVoices != null && sakashitaCorrectVoices.Length > 0 && audioSource != null)
        {
            int index = Random.Range(0, sakashitaCorrectVoices.Length);
            if (sakashitaCorrectVoices[index] != null)
            {
                audioSource.PlayOneShot(sakashitaCorrectVoices[index]);
                Debug.Log($"Playing sakashita voice: {sakashitaCorrectVoices[index].name}");
            }
        }
        else
        {
            Debug.LogWarning("sakashitaCorrectVoices is not assigned or empty!");
        }
    }

    /// <summary>
    /// 役職が上がった時の効果音を再生
    /// </summary>
    public void PlayRankUp()
    {
        if (rankUpSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(rankUpSound);
            Debug.Log("Playing rank-up sound");
        }
        else
        {
            Debug.LogWarning("rankUpSound or AudioSource is not assigned!");
        }
    }

    /// <summary>
    /// ダメージボイスをランダムに再生
    /// </summary>
    public void PlayDamageVoice()
    {
        if (damageVoices != null && damageVoices.Length > 0 && audioSource != null)
        {
            int index = Random.Range(0, damageVoices.Length);
            if (damageVoices[index] != null)
            {
                audioSource.PlayOneShot(damageVoices[index]);
                Debug.Log($"Playing damage voice: {damageVoices[index].name}");
            }
        }
    }
}
