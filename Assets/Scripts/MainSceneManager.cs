using UnityEngine;

/// <summary>
/// MainScene用の管理クラス
/// シーン読み込み時にメインBGMを再生する
/// </summary>
public class MainSceneManager : MonoBehaviour
{
    private void Start()
    {
        // AudioManager のインスタンスが存在することを確認
        if (AudioManager.Instance != null)
        {
            // mainBGM が設定されていればそれを再生
            if (AudioManager.Instance.mainBGM != null)
            {
                AudioManager.Instance.PlayBGM(AudioManager.Instance.mainBGM);
                Debug.Log("MainScene BGM (main) started playing with loop.");
            }
            else
            {
                Debug.LogWarning("MainSceneManager: mainBGM is not assigned in AudioManager!");
            }
        }
        else
        {
            Debug.LogError("MainSceneManager: AudioManager instance not found!");
        }
    }
}
