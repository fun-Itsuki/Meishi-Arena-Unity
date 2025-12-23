using UnityEngine;
using TMPro;

public class BattleUIManager : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("人数表示用テキスト（例: 3/10人目）")]
    public TMP_Text exchangeCountText;
    
    [Tooltip("スコア表示用テキスト")]
    public TMP_Text scoreText;
    
    [Tooltip("残り時間表示用テキスト")]
    public TMP_Text timerText;
    
    [Tooltip("NPC役職表示用テキスト")]
    public TMP_Text npcRankText;

    [Header("Life UI")]
    public GameObject[] heartIcons; // ハートアイコンの配列(3つ)

    /// <summary>
    /// 交換回数を更新
    /// </summary>
    public void UpdateExchangeCount(int current)
    {
        if (exchangeCountText != null)
        {
            exchangeCountText.text = $"{current}人目";
        }
    }

    /// <summary>
    /// 残りライフを更新
    /// </summary>
    public void UpdateLives(int lives)
    {
        if (heartIcons != null)
        {
            for (int i = 0; i < heartIcons.Length; i++)
            {
                if (heartIcons[i] != null)
                {
                    heartIcons[i].SetActive(i < lives);
                }
            }
        }
    }

    /// <summary>
    /// スコアを更新
    /// </summary>
    public void UpdateScore(int score)
    {
        if (scoreText != null)
        {
            scoreText.text = $"Score: {score}";
        }
    }

    /// <summary>
    /// 残り時間を更新
    /// </summary>
    public void UpdateTimer(float remainingTime)
    {
        if (timerText != null)
        {
            // 0以下にならないようにクランプ
            remainingTime = Mathf.Max(0, remainingTime);
            timerText.text = $"残り時間: {remainingTime:F1}秒";
        }
    }

    /// <summary>
    /// NPC役職を更新
    /// </summary>
    public void UpdateNPCRank(CardBattleLogic.NPCRank rank)
    {
        if (npcRankText != null)
        {
            string rankText = "";
            switch (rank)
            {
                case CardBattleLogic.NPCRank.Top:
                    rankText = "上";
                    break;
                case CardBattleLogic.NPCRank.Middle:
                    rankText = "中";
                    break;
                case CardBattleLogic.NPCRank.Bottom:
                    rankText = "下";
                    break;
            }
            npcRankText.text = $"相手の役職: {rankText}";
        }
    }

    void Update()
    {
        // ScoreManagerからスコアを取得して更新
        if (ScoreManager.Instance != null)
        {
            UpdateScore(ScoreManager.Instance.Score);
        }
    }
}
