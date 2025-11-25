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
}
