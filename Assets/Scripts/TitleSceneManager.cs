using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleSceneManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject[] mainUIObjectsToHide; 
    [SerializeField] private UnityEngine.UI.Button mainContinueButton;
    [SerializeField] private GameObject slotSelectionPanel;
    [SerializeField] private UnityEngine.UI.Button slot1Button;
    [SerializeField] private UnityEngine.UI.Button slot2Button;

    [Header("Confirmation UI")]
    [SerializeField] private GameObject confirmPanel;
    [SerializeField] private TMPro.TMP_Text confirmText;
    [SerializeField] private UnityEngine.UI.Button confirmYesButton;    // はいボタン
    [SerializeField] private UnityEngine.UI.Button confirmNoButton;     // いいえボタン
    [SerializeField] private UnityEngine.UI.Button confirmBackButton;  // 戻る（閉じる）ボタン
    [SerializeField] private TMPro.TMP_Text statusText; 

    private enum SelectionMode { NewGame, Continue }
    private SelectionMode currentSelectionMode;
    private int pendingSlotIndex = -1; 

    private void Awake()
    {
        if (mainContinueButton != null && ScoreManager.Instance != null)
        {
            mainContinueButton.interactable = ScoreManager.Instance.HasAnySaveFile();
        }

        if (slotSelectionPanel != null) slotSelectionPanel.SetActive(false);
        if (confirmPanel != null) confirmPanel.SetActive(false);
        if (statusText != null) statusText.text = "";
    }

    public void OnNewGameMainClicked()
    {
        PlayClickSound();
        currentSelectionMode = SelectionMode.NewGame;
        OpenSlotSelection();
    }

    public void OnContinueMainClicked()
    {
        PlayClickSound();
        currentSelectionMode = SelectionMode.Continue;
        OpenSlotSelection();
    }

    private void OpenSlotSelection()
    {
        if (slotSelectionPanel != null)
        {
            slotSelectionPanel.SetActive(true);
            
            if (mainUIObjectsToHide != null)
            {
                foreach (var obj in mainUIObjectsToHide)
                {
                    if (obj != null) obj.SetActive(false);
                }
            }

            // 要望に合わせて：すべてのボタンを一旦有効にする（押してから「データなし」と出すため）
            if (slot1Button != null) slot1Button.interactable = true;
            if (slot2Button != null) slot2Button.interactable = true;
        }
    }

    public void OnSlotSelected(int slot)
    {
        Debug.Log($"[OnSlotSelected] Slot {slot} clicked. Mode: {currentSelectionMode}");
        PlayClickSound();

        if (ScoreManager.Instance == null)
        {
            Debug.LogError("[OnSlotSelected] ScoreManager is NULL! Confirm if the ScoreManager object is in the scene.");
            return;
        }

        bool hasData = ScoreManager.Instance.HasSaveFile(slot);
        Debug.Log($"[OnSlotSelected] Slot {slot} has data: {hasData}");
        
        string message = "";
        bool showYesNo = true; // はい・いいえを表示するか
        bool showBack = false; // 戻るを表示するか

        if (currentSelectionMode == SelectionMode.NewGame)
        {
            if (hasData)
                message = $"スロット {slot + 1} のデータを上書きして\n新しく始めますか？";
            else
                message = $"スロット {slot + 1} に新しいゲームデータを\n登録しますか？";
        }
        else // Continue
        {
            if (hasData)
                message = $"スロット {slot + 1} でゲームを再開しますか？";
            else
            {
                message = $"スロット {slot + 1} にセーブデータがありません。";
                showYesNo = false; 
                showBack = true;   // データがない時は「戻る」だけ出す
            }
        }

        Debug.Log($"[OnSlotSelected] Showing dialog. YesNo: {showYesNo}, Back: {showBack}");
        ShowConfirmDialog(slot, message, showYesNo, showBack);
    }

    private void ShowConfirmDialog(int slot, string message, bool showYesNo, bool showBackButton)
    {
        pendingSlotIndex = slot;
        if (confirmPanel != null)
        {
            if (confirmText != null) confirmText.text = message;
            
            // ボタンの出し分け
            if (confirmYesButton != null) confirmYesButton.gameObject.SetActive(showYesNo);
            if (confirmNoButton != null) confirmNoButton.gameObject.SetActive(showYesNo);
            if (confirmBackButton != null) confirmBackButton.gameObject.SetActive(showBackButton);
            
            confirmPanel.SetActive(true);
        }
    }

    public void OnConfirmOverwriteYes()
    {
        PlayClickSound();
        if (confirmPanel != null) confirmPanel.SetActive(false);
        ExecuteSlotSelection(pendingSlotIndex);
    }

    public void OnConfirmOverwriteNo()
    {
        PlayClickSound();
        if (confirmPanel != null) confirmPanel.SetActive(false);
        pendingSlotIndex = -1;
    }

    /// <summary>
    /// エラー時の「戻る（閉じる）」ボタン
    /// </summary>
    public void OnConfirmBack()
    {
        PlayClickSound();
        if (confirmPanel != null) confirmPanel.SetActive(false);
        pendingSlotIndex = -1;
    }

    private void ExecuteSlotSelection(int slot)
    {
        if (ScoreManager.Instance == null) return;

        if (currentSelectionMode == SelectionMode.NewGame)
        {
            ScoreManager.Instance.ClearSlot(slot);
        }

        ScoreManager.Instance.SetActiveSlot(slot);
        StartGame();
    }

    private void ShowStatusMessage(string msg)
    {
        if (statusText != null)
        {
            statusText.text = msg;
            // 数秒後に消すなどの処理を入れることも可能
        }
    }

    /// <summary>
    /// スロット選択をキャンセル
    /// </summary>
    public void OnCancelSlotSelection()
    {
        PlayClickSound();
        if (slotSelectionPanel != null)
        {
            slotSelectionPanel.SetActive(false);
        }

        // メインメニューのUIを再表示
        if (mainUIObjectsToHide != null)
        {
            foreach (var obj in mainUIObjectsToHide)
            {
                if (obj != null) obj.SetActive(true);
            }
        }
    }

    private void PlayClickSound()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayButtonClick();
        }
    }

    private void Start()
    {
        // タイトルシーン開始時にタイトル用BGMを再生
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayBGM(AudioManager.Instance.titleBGM);
        }
        else
        {
            Debug.LogWarning("AudioManager instance not found in scene. Title BGM will not play.");
        }
    }

    private void OnDisable()
    {
        // タイトルシーンを離れるときにはタイトルBGMを停止
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.StopBGM();
        }
    }

    private void StartGame()
    {
        string sceneName = "MainScene";
        Debug.Log($"[TitleSceneManager] Attempting to load scene: {sceneName}");
        
        // シーンが存在するかチェック（デバッグ用）
        try {
            SceneManager.LoadScene(sceneName);
        } catch (System.Exception e) {
            Debug.LogError($"[TitleSceneManager] Failed to load scene '{sceneName}': {e.Message}");
            if (statusText != null) statusText.text = $"エラー: シーン '{sceneName}' が見つかりません。";
        }
    }
}
