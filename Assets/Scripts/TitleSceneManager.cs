using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleSceneManager : MonoBehaviour
{
    /// <summary>
    /// 「始める」ボタンがクリックされたときに呼ばれるメソッド
    /// </summary>
    public void OnStartButtonClicked()
    {
        Debug.Log("[TitleSceneManager] 始めるボタンがクリックされました");
        
        // ボタンクリック音を再生
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayButtonClick();
        }
        
        SceneManager.LoadScene("MainScene");
    }
}
