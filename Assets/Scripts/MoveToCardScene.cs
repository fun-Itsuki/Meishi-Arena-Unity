using UnityEngine;
using UnityEngine.SceneManagement;

public class MoveToCardScene : MonoBehaviour
{
    public string sceneName = "BusinessCardScene";

    private bool isNearNPC = false;

    void Update()
    {
        if (isNearNPC && Input.GetKeyDown(KeyCode.F))
        {
            SceneManager.LoadScene(sceneName);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("NPC"))
        {
            isNearNPC = true;
            Debug.Log("NPC detected");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("NPC"))
        {
            isNearNPC = false;
            Debug.Log("NPC left");
        }
    }
}
