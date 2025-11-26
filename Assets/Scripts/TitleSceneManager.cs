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
        SceneManager.LoadScene("MainScene");
    }
}
