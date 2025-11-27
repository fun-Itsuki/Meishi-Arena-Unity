using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class ResultManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI rankText;
    [SerializeField] private TextMeshProUGUI scoreText;

    [Header("Scene Names")]
    [SerializeField] private string mainSceneName = "MainScene";

    private void Start()
    {
        Debug.Log("ResultManager: Start() called");
        DisplayResult();
        
        // ビルド設定のシーンを確認
        int sceneCount = SceneManager.sceneCountInBuildSettings;
        Debug.Log($"Scenes in Build Settings: {sceneCount}");
        for (int i = 0; i < sceneCount; i++)
        {
            string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
            Debug.Log($"  Scene {i}: {scenePath}");
        }
    }

    void DisplayResult()
    {
        if (ScoreManager.Instance == null)
        {
            Debug.LogError("ScoreManager instance not found!");
            return;
        }

        // バトル結果を取得
        int battleScore = ScoreManager.Instance.LastBattleScore;
        string result = ScoreManager.Instance.LastBattleResult;

        // ランク判定
        string rank = GetRank(battleScore);

        // UI更新
        if (rankText != null)
        {
            rankText.text = $"Rank: {rank}";
        }

        if (scoreText != null)
        {
            scoreText.text = $"Battle Score: {battleScore}";
        }

        Debug.Log($"Result: {result}, Rank: {rank}, Battle Score: {battleScore}");
    }

    string GetRank(int score)
    {
        // スコアに基づいてランクを判定
        if (score >= 100)
        {
            return "S";
        }
        else if (score <= -999)
        {
            return "F";
        }
        else
        {
            // 将来的に他のランクを追加する場合
            return "C";
        }
    }

    /// <summary>
    /// 次のステージへ(MainSceneに戻る)
    /// </summary>
    public void OnNextStageButtonClicked()
    {
        Debug.Log($"[ResultManager] Next Stage button clicked! Attempting to load: {mainSceneName}");
        
        // ボタンクリック音を再生
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayButtonClick();
        }
        
        try
        {
            SceneManager.LoadScene(mainSceneName);
            Debug.Log($"[ResultManager] LoadScene called successfully for: {mainSceneName}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[ResultManager] Failed to load scene '{mainSceneName}': {e.Message}");
        }
    }

    /// <summary>
    /// リトライ(前回のバトルシーンに戻る)
    /// </summary>
    public void OnRetryButtonClicked()
    {
        Debug.Log("[ResultManager] Retry button clicked!");
        
        // ボタンクリック音を再生
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayButtonClick();
        }
        
        if (ScoreManager.Instance != null && !string.IsNullOrEmpty(ScoreManager.Instance.LastSceneName))
        {
            string sceneName = ScoreManager.Instance.LastSceneName;
            Debug.Log($"[ResultManager] Retrying scene: {sceneName}");
            
            // リトライ時はスコアをリセット(オプション)
            // ScoreManager.Instance.ResetScore();
            
            try
            {
                SceneManager.LoadScene(sceneName);
                Debug.Log($"[ResultManager] LoadScene called successfully for: {sceneName}");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[ResultManager] Failed to load scene '{sceneName}': {e.Message}");
            }
        }
        else
        {
            Debug.LogWarning("[ResultManager] No previous scene to retry. Loading Main Scene instead.");
            
            try
            {
                SceneManager.LoadScene(mainSceneName);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[ResultManager] Failed to load main scene '{mainSceneName}': {e.Message}");
            }
        }
    }
}
