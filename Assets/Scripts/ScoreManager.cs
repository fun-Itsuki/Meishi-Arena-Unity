using UnityEngine;
using TMPro;
using System.IO;

[System.Serializable]
public class SaveData
{
    public int totalExchangedCount = 0;
    public int highScore = 0;
}

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

    private SaveData currentSaveData = new SaveData();
    private string baseSaveFileName = "save_data";
    private int activeSlotIndex = 0;

    public int TotalExchangedCount => currentSaveData.totalExchangedCount;

    /// <summary>
    /// 指定したスロットにセーブファイルが存在するか
    /// </summary>
    public bool HasSaveFile(int slot)
    {
        return File.Exists(GetSaveFilePath(slot));
    }

    /// <summary>
    /// いずれかのスロットにセーブファイルが存在するか
    /// </summary>
    public bool HasAnySaveFile()
    {
        return HasSaveFile(0) || HasSaveFile(1);
    }

    private string GetSaveFilePath(int slot)
    {
        return Path.Combine(Application.persistentDataPath, $"{baseSaveFileName}_{slot}.json");
    }

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
        return GetPlayerRankLevel(Score);
    }

    /// <summary>
    /// 指定されたスコアに基づいた役職レベル（0～7）を返す
    /// </summary>
    public int GetPlayerRankLevel(int score)
    {
        if (score >= 12000) return 7; // 常務
        if (score >= 9000) return 6;  // 本部長
        if (score >= 6500) return 5;  // 部長
        if (score >= 4500) return 4;  // 次長
        if (score >= 3000) return 3;  // 課長
        if (score >= 2000) return 2;  // 係長
        if (score >= 1000) return 1;  // 主任
        return 0; // 一般社員
    }

    /// <summary>
    /// 現在のスコアに基づいた役職名を返す
    /// </summary>
    public string GetPlayerTitle()
    {
        return GetPlayerTitle(Score);
    }

    /// <summary>
    /// 指定されたスコアに基づいた役職名を返す
    /// </summary>
    public string GetPlayerTitle(int score)
    {
        switch (GetPlayerRankLevel(score))
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

    /// <summary>
    /// 操作対象のスロットを設定し、データをロードする
    /// </summary>
    public void SetActiveSlot(int slot)
    {
        activeSlotIndex = slot;
        Load();
    }

    public void AddTotalExchangedCount(int amount)
    {
        currentSaveData.totalExchangedCount += amount;
        Save();
    }

    private void Save()
    {
        try
        {
            string path = GetSaveFilePath(activeSlotIndex);
            string json = JsonUtility.ToJson(currentSaveData, true);
            File.WriteAllText(path, json);
            Debug.Log($"Data saved to slot {activeSlotIndex}: {path}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to save data: {e.Message}");
        }
    }

    private void Load()
    {
        try
        {
            string path = GetSaveFilePath(activeSlotIndex);
            if (File.Exists(path))
            {
                string json = File.ReadAllText(path);
                currentSaveData = JsonUtility.FromJson<SaveData>(json);
                Debug.Log($"Data loaded from slot {activeSlotIndex}.");
            }
            else
            {
                currentSaveData = new SaveData();
                Debug.Log($"No save file for slot {activeSlotIndex}. Created new SaveData.");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to load data from slot {activeSlotIndex}: {e.Message}");
            currentSaveData = new SaveData();
        }
    }

    /// <summary>
    /// 指定スロットのセーブデータを削除し、初期状態に戻す
    /// </summary>
    public void ClearSlot(int slot)
    {
        string path = GetSaveFilePath(slot);
        if (File.Exists(path))
        {
            File.Delete(path);
            Debug.Log($"Save file for slot {slot} deleted.");
        }
        
        if (slot == activeSlotIndex)
        {
            currentSaveData = new SaveData();
            ResetScore();
        }
    }

    /// <summary>
    /// 現在のスロットのデータをリセットする
    /// </summary>
    [System.Obsolete("Use ClearSlot instead")]
    public void ClearSaveData()
    {
        ClearSlot(activeSlotIndex);
    }
}
