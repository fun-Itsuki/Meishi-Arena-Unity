using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class BattleUIManager : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("人数表示用テキスト（例: 3/10人目）")]
    public TMP_Text exchangeCountText;
    
    [Tooltip("スコア表示用テキスト")]
    public TMP_Text scoreText;
    
    [Tooltip("残り時間表示用テキスト")]
    public TMP_Text timerText;
    
    [Tooltip("残り時間をバーで表示（Image コンポーネント。Fill Amount で表示）")]
    public Image timerBarImage;

    [Tooltip("タイマーバーの最大時間（秒）")]
    public float maxTimerDuration = 1.5f;
    
    [Tooltip("NPC役職表示用テキスト")]
    public TMP_Text npcRankText;

    [Tooltip("プレイヤー役職表示用テキスト")]
    public TMP_Text playerTitleText;

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

        // プログレスバーを更新
        if (timerBarImage != null)
        {
            // remainingTime が最大時間に対して何割か計算
            float fillAmount = Mathf.Clamp01(remainingTime / maxTimerDuration);
            timerBarImage.fillAmount = fillAmount;
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
                case CardBattleLogic.NPCRank.Employee:
                    rankText = "一般社員";
                    break;
                case CardBattleLogic.NPCRank.Shunin:
                    rankText = "主任";
                    break;
                case CardBattleLogic.NPCRank.Keicho:
                    rankText = "係長";
                    break;
                case CardBattleLogic.NPCRank.Kacho:
                    rankText = "課長";
                    break;
                case CardBattleLogic.NPCRank.Jicho:
                    rankText = "次長";
                    break;
                case CardBattleLogic.NPCRank.Bucho:
                    rankText = "部長";
                    break;
                case CardBattleLogic.NPCRank.Honbucho:
                    rankText = "本部長";
                    break;
                case CardBattleLogic.NPCRank.Jomu:
                    rankText = "常務";
                    break;
                case CardBattleLogic.NPCRank.Senmu:
                    rankText = "専務";
                    break;
                case CardBattleLogic.NPCRank.Fukushacho:
                    rankText = "副社長";
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
            
            // プレイヤーの役職を更新
            if (playerTitleText != null)
            {
                playerTitleText.text = $"{ScoreManager.Instance.PlayerName} さんの役職: {ScoreManager.Instance.GetPlayerTitle()}";
            }
        }
    }
}
