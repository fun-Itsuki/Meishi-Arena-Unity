using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    public int Score { get; private set; } = 0;
    [SerializeField] private TMP_Text scoreText; // Canvas上のTextMeshProオブジェクトをアサイン
    [SerializeField] private TMP_Text resultText; // 結果表示用（You dieなど）

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
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

    void UpdateUI()
    {
        if (scoreText != null)
            scoreText.text = $"Score: {Score}";
    }
}
