using UnityEngine;

public class FPSLook : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] Transform playerBody;

    [Header("Settings")]
    [SerializeField] float mouseSensitivity = 16000f;
    [SerializeField] float maxPitch = 89f;

    [Header("Initial View (Fixed)")]
    [SerializeField] float initialYaw = 0f;   // 体の向き（Y軸）
    [SerializeField] float initialPitch = 0f; // 視点の上下（X軸）

    float pitch = 0f;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // ✅ 初期視点を固定（リセット）
        pitch = Mathf.Clamp(initialPitch, -maxPitch, maxPitch);
        transform.localRotation = Quaternion.Euler(pitch, 0f, 0f);

        if (playerBody != null)
            playerBody.rotation = Quaternion.Euler(0f, initialYaw, 0f);
    }

    void Update()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        pitch -= mouseY;
        pitch = Mathf.Clamp(pitch, -maxPitch, maxPitch);
        transform.localRotation = Quaternion.Euler(pitch, 0f, 0f);

        if (playerBody != null)
            playerBody.Rotate(Vector3.up * mouseX);
    }
}


