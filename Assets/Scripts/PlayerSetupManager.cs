using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PlayerSetupManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_InputField companyNameInput;
    [SerializeField] private TMP_InputField playerNameInput;
    [SerializeField] private Button startButton;

    private void Start()
    {
        if (startButton != null)
        {
            startButton.onClick.AddListener(OnStartButtonClicked);
        }
    }

    private void OnStartButtonClicked()
    {
        if (ScoreManager.Instance == null)
        {
            Debug.LogError("[PlayerSetupManager] ScoreManager not found!");
            SceneManager.LoadScene("MainScene");
            return;
        }

        string company = companyNameInput != null ? companyNameInput.text : "";
        string name = playerNameInput != null ? playerNameInput.text : "";

        // 設定を保存
        ScoreManager.Instance.UpdatePlayerProfile(name, company);

        Debug.Log($"[PlayerSetupManager] Profile Saved: {company} / {name}. Transitioning to MainScene.");

        // 効果音があれば再生
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayButtonClick();
        }

        // 本編へ遷移
        SceneManager.LoadScene("MainScene");
    }
}
