using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
    [Header("Roots")]
    [SerializeField] private GameObject menuRoot;     // MenuPanel（EscでON/OFFするやつ）
    [SerializeField] private GameObject buttonsRoot;  // Buttons（3ボタンの親）
    [SerializeField] private GameObject rulesPanel;   // RulesPanel
    [SerializeField] private GameObject rolesPanel;   // RolesPanel

    // menuRootのON/OFF変化を検知するため
    private bool lastMenuRootActive = false;

    private void Start()
    {
        // 起動時に一旦初期化
        ResetToMainMenu();

        // 現在状態を記録（MenuPanelが既にONの場合でも次回判定が狂わないように）
        lastMenuRootActive = (menuRoot != null && menuRoot.activeSelf);
    }

    private void Update()
    {
        if (menuRoot == null) return;

        bool cur = menuRoot.activeSelf;

        // OFF → ON に変わった瞬間（= Escでメニューを開いた瞬間）に初期状態へ戻す
        if (!lastMenuRootActive && cur)
        {
            ResetToMainMenu();
        }

        lastMenuRootActive = cur;
    }

    // --- ボタン処理 ---

    // タイトルに戻る（Build Index 0がTitleの前提）
    public void BackToTitle()
    {
        Debug.Log("[Menu] Load Title by index 0");
        SceneManager.LoadScene(0);
    }

    // ルールを見る
    public void OpenRules()
    {
        Debug.Log("[Menu] OpenRules");
        if (rulesPanel != null) rulesPanel.SetActive(true);
        if (rolesPanel != null) rolesPanel.SetActive(false);
        ShowButtons(false);
    }

    public void CloseRules()
    {
        Debug.Log("[Menu] CloseRules");
        if (rulesPanel != null) rulesPanel.SetActive(false);
        ShowButtons(true);
    }

    // 役職確認
    public void OpenRoles()
    {
        Debug.Log("[Menu] OpenRoles");
        if (rolesPanel != null) rolesPanel.SetActive(true);
        if (rulesPanel != null) rulesPanel.SetActive(false);
        ShowButtons(false);
    }

    public void CloseRoles()
    {
        Debug.Log("[Menu] CloseRoles");
        if (rolesPanel != null) rolesPanel.SetActive(false);
        ShowButtons(true);
    }

    // --- 共通 ---

    // メニューを開いた時の「初期状態」
    public void ResetToMainMenu()
    {
        Debug.Log("[Menu] ResetToMainMenu");
        if (rulesPanel != null) rulesPanel.SetActive(false);
        if (rolesPanel != null) rolesPanel.SetActive(false);
        ShowButtons(true);
    }

    private void ShowButtons(bool show)
    {
        if (buttonsRoot != null) buttonsRoot.SetActive(show);
    }
}
