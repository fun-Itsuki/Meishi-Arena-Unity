using UnityEngine;

public class CursorUnlocker : MonoBehaviour
{
    void Awake()
    {
        Cursor.lockState = CursorLockMode.None; // ƒƒbƒN‰ğœ
        Cursor.visible = true;                 // •\¦
    }
}

