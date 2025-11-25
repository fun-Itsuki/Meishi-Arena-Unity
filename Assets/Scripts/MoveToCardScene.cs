using UnityEngine;
using UnityEngine.SceneManagement;

public class MoveToCardScene : MonoBehaviour
{
    public string sceneName = "BusinessCardScene";
    public GameObject interactionUI; // UI to show when near NPC

    private bool isNearNPC = false;

    void Start()
    {
        if (interactionUI != null)
        {
            interactionUI.SetActive(false);
        }
    }

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
            if (interactionUI != null)
            {
                interactionUI.SetActive(true);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        Debug.Log($"[MoveToCardScene] OnTriggerExit: {other.name}, Tag: {other.tag}");
        if (other.CompareTag("NPC"))
        {
            isNearNPC = false;
            Debug.Log("[MoveToCardScene] NPC left (Tag match)");
            if (interactionUI != null)
            {
                interactionUI.SetActive(false);
            }
        }
    }
}
