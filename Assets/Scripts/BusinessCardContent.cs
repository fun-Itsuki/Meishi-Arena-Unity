using UnityEngine;
using TMPro;

public class BusinessCardContent : MonoBehaviour
{
    [Header("UI / Text References")]
    [SerializeField] private TMP_Text companyNameText;
    [SerializeField] private TMP_Text playerNameText;

    void Start()
    {
        Debug.Log($"[BusinessCardContent] Start called on {gameObject.name}");
        UpdateCardDisplay();
    }

    void OnEnable()
    {
        Debug.Log($"[BusinessCardContent] OnEnable called on {gameObject.name}");
        UpdateCardDisplay();
    }

    /// <summary>
    /// ScoreManager から現在の設定を取得して表示を更新する
    /// </summary>
    public void UpdateCardDisplay()
    {
        // Instanceがない場合、シーン内を一度だけ検索してみる
        if (ScoreManager.Instance == null)
        {
            var sm = FindFirstObjectByType<ScoreManager>();
            if (sm != null)
            {
                Debug.Log("[BusinessCardContent] ScoreManager found in scene via fallback search.");
                // ScoreManager.AwakeでInstanceがセットされるはずだが、念のためこのフレームでも使えるようにする
            }
            else
            {
                Debug.LogWarning("[BusinessCardContent] ScoreManager not found in scene or instance. Using default values.");
                if (companyNameText != null) companyNameText.text = "フリーランス";
                if (playerNameText != null) playerNameText.text = "名無し";
                return;
            }
        }

        // ここまで来れば ScoreManager.Instance は有効なはず
        if (companyNameText != null)
        {
            companyNameText.text = ScoreManager.Instance.CompanyName;
            Debug.Log($"[BusinessCardContent] Updated Company: {ScoreManager.Instance.CompanyName}");
        }
        else
        {
            Debug.LogWarning("[BusinessCardContent] companyNameText is NOT assigned in the Inspector!");
        }

        if (playerNameText != null)
        {
            playerNameText.text = ScoreManager.Instance.PlayerName;
            Debug.Log($"[BusinessCardContent] Updated Player Name: {ScoreManager.Instance.PlayerName}");
        }
        else
        {
            Debug.LogWarning("[BusinessCardContent] playerNameText is NOT assigned in the Inspector!");
        }
    }
}
