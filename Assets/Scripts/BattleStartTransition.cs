using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class BattleStartTransition : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("暗転用の黒いパネル")]
    public Image darkOverlay;
    
    [Tooltip("「名刺交換開始」テキスト")]
    public TextMeshProUGUI battleStartText;

    [Header("Timing Settings")]
    [Tooltip("テキストを表示する時間(秒)")]
    public float textDisplayDuration = 2.0f;
    
    [Tooltip("テキストがフェードアウトする時間(秒)")]
    public float textFadeDuration = 1.0f;
    
    [Tooltip("画面全体のフェードイン時間(秒)")]
    public float screenFadeDuration = 3.0f;

    [Header("Visual Settings")]
    [Tooltip("初期の暗さ(0=完全に明るい, 1=完全に暗い)")]
    [Range(0f, 1f)]
    public float initialDarkness = 0.7f;

    private float elapsedTime = 0f;
    private bool transitionComplete = false;

    // 演出完了時のコールバック
    public System.Action OnTransitionComplete;

    void Start()
    {
        // バトル開始音を再生
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayBattleStart();
        }
        
        // 初期状態の設定
        if (darkOverlay != null)
        {
            Color overlayColor = darkOverlay.color;
            overlayColor.a = initialDarkness;
            darkOverlay.color = overlayColor;
        }

        if (battleStartText != null)
        {
            Color textColor = battleStartText.color;
            textColor.a = 1f;
            battleStartText.color = textColor;
        }
    }

    void Update()
    {
        if (transitionComplete) return;

        elapsedTime += Time.deltaTime;

        // テキストのフェードアウト(2秒後から1秒かけて)
        if (battleStartText != null)
        {
            float textAlpha = 1f;
            
            if (elapsedTime > textDisplayDuration)
            {
                float fadeProgress = (elapsedTime - textDisplayDuration) / textFadeDuration;
                textAlpha = Mathf.Lerp(1f, 0f, fadeProgress);
            }

            Color textColor = battleStartText.color;
            textColor.a = textAlpha;
            battleStartText.color = textColor;
        }

        // 画面の明るさフェードイン(0秒から3秒かけて)
        if (darkOverlay != null)
        {
            float fadeProgress = elapsedTime / screenFadeDuration;
            float overlayAlpha = Mathf.Lerp(initialDarkness, 0f, fadeProgress);

            Color overlayColor = darkOverlay.color;
            overlayColor.a = overlayAlpha;
            darkOverlay.color = overlayColor;
        }

        // 演出完了チェック
        if (elapsedTime >= Mathf.Max(textDisplayDuration + textFadeDuration, screenFadeDuration))
        {
            transitionComplete = true;
            
            // UI要素を非表示にする
            if (darkOverlay != null)
                darkOverlay.gameObject.SetActive(false);
            
            if (battleStartText != null)
                battleStartText.gameObject.SetActive(false);

            // コールバック実行
            OnTransitionComplete?.Invoke();
            
            Debug.Log("Battle start transition complete!");
        }
    }

    /// <summary>
    /// 演出が完了したかどうかを取得
    /// </summary>
    public bool IsTransitionComplete()
    {
        return transitionComplete;
    }
}
