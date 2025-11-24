using UnityEngine;
using UnityEngine.SceneManagement;

public class MoveToCardScene : MonoBehaviour
{
    public string sceneName = "BusinessCardScene";

    private bool isNearNPC = false;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            Debug.Log($"[MoveToCardScene] F key pressed. isNearNPC: {isNearNPC}");
            if (isNearNPC)
            {
                Debug.Log($"[MoveToCardScene] Loading scene: {sceneName}");
                SceneManager.LoadScene(sceneName);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"[MoveToCardScene] OnTriggerEnter: {other.name}, Tag: {other.tag}");
        if (other.CompareTag("NPC"))
        {
            isNearNPC = true;
            Debug.Log("[MoveToCardScene] NPC detected (Tag match)");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        Debug.Log($"[MoveToCardScene] OnTriggerExit: {other.name}, Tag: {other.tag}");
        if (other.CompareTag("NPC"))
        {
            isNearNPC = false;
            Debug.Log("[MoveToCardScene] NPC left (Tag match)");
        }
    }
}
