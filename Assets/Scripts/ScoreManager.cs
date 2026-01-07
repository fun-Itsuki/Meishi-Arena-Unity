using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    public int Score { get; private set; } = 0;
    
    // リザルト画面用のデータ
    public int LastBattleScore { get; private set; } = 0;
    public string LastBattleResult { get; private set; } = "";
    public string LastSceneName { get; private set; } = "";
    
    // 連続交換の統計情報
    public int CurrentExchangeNumber { get; set; } = 0;
    public int SuccessCount { get; set; } = 0;
    public int FailureCount { get; set; } = 0;
    
    [SerializeField] private TMP_Text scoreText; // Canvas上のTextMeshProオブジェクトをアサイン
    [SerializeField] private TMP_Text resultText; // 結果表示用（You dieなど）

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject); // シーン遷移時も保持
    }

    private void Start()
    {
        UpdateUI();
        if (resultText != null) resultText.text = ""; // 最初は非表示
    }

    public void AddScore(int amount)
    {
        Score += amount;
        UpdateUI();
    }

    public void ShowResult(string message)
    {
        if (resultText != null)
        {
            resultText.text = message;
            resultText.gameObject.SetActive(true);
        }
    }

    /// <summary>
    /// バトル結果を保存(リザルト画面で使用)
    /// </summary>
    public void SaveBattleResult(int battleScore, string result, string sceneName)
    {
        LastBattleScore = battleScore;
        LastBattleResult = result;
        LastSceneName = sceneName;
        Debug.Log($"Battle result saved: Score={battleScore}, Result={result}, Scene={sceneName}");
    }

    /// <summary>
    /// スコアをリセット(リトライ時などに使用)
    /// </summary>
    public void ResetScore()
    {
        Score = 0;
        UpdateUI();
    }

    void UpdateUI()
    {
        if (scoreText != null)
            scoreText.text = $"Score: {Score}";
    }

    /// <summary>
    /// 現在のスコアに基づいた役職レベル（0～7）を返す
    /// </summary>
    public int GetPlayerRankLevel()
    {
        if (Score >= 12000) return 7; // 常務
        if (Score >= 9000) return 6;  // 本部長
        if (Score >= 6500) return 5;  // 部長
        if (Score >= 4500) return 4;  // 次長
        if (Score >= 3000) return 3;  // 課長
        if (Score >= 2000) return 2;  // 係長
        if (Score >= 1000) return 1;  // 主任
        return 0; // 一般社員
    }

    /// <summary>
    /// 現在のスコアに基づいた役職名を返す
    /// </summary>
    public string GetPlayerTitle()
    {
        switch (GetPlayerRankLevel())
        {
            case 7: return "常務";
            case 6: return "本部長";
            case 5: return "部長";
            case 4: return "次長";
            case 3: return "課長";
            case 2: return "係長";
            case 1: return "主任";
            default: return "一般社員";
        }
    }
}
