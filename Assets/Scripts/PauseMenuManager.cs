using UnityEngine;

public class PauseMenuManager : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject menuPanel;

    [Header("Option")]
    [SerializeField] private bool pauseGameWithTimeScale = false; // trueにするとTime.timeScale=0で停止
    [SerializeField] private MonoBehaviour[] disableWhileMenuOpen; // 表示中に止めたい操作スクリプト（移動/視点など）

    public bool IsOpen => menuPanel != null && menuPanel.activeSelf;

    private void Start()
    {
        // 念のため開始時は閉じる
        if (menuPanel != null) menuPanel.SetActive(false);
        ApplyCursorState(false);
        if (pauseGameWithTimeScale) Time.timeScale = 1f;
    }

    private void Update()
    {
        // ★ここがEsc検知（旧Inputでも動く）
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Toggle();
        }
    }

    public void Toggle()
    {
        if (IsOpen) Close();
        else Open();
    }

    public void Open()
    {
        if (menuPanel == null) return;

        menuPanel.SetActive(true);

        // 操作停止（任意）
        if (disableWhileMenuOpen != null)
        {
            foreach (var b in disableWhileMenuOpen)
                if (b != null) b.enabled = false;
        }

        // ゲーム停止（任意）
        if (pauseGameWithTimeScale) Time.timeScale = 0f;

        ApplyCursorState(true);
    }

    public void Close()
    {
        if (menuPanel == null) return;

        menuPanel.SetActive(false);

        if (disableWhileMenuOpen != null)
        {
            foreach (var b in disableWhileMenuOpen)
                if (b != null) b.enabled = true;
        }

        if (pauseGameWithTimeScale) Time.timeScale = 1f;

        ApplyCursorState(false);
    }

    private void ApplyCursorState(bool show)
    {
        Cursor.visible = show;
        Cursor.lockState = show ? CursorLockMode.None : CursorLockMode.Locked;
    }
}
