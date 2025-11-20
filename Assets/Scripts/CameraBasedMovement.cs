using UnityEngine;

public class CameraBasedMovement : MonoBehaviour
{
    public float moveSpeed = 5f;
    public Transform cameraTransform;

    void Update()
    {
        if (cameraTransform == null) return;

        float h = Input.GetAxis("Horizontal"); // A/D
        float v = Input.GetAxis("Vertical");   // W/S

        // カメラ方向を取得（上下成分をカット）
        Vector3 forward = cameraTransform.forward;
        Vector3 right = cameraTransform.right;

        forward.y = 0;
        right.y = 0;

        forward.Normalize();
        right.Normalize();

        // キャラクターの回転なしで移動
        Vector3 move = forward * v + right * h;
        transform.position += move * moveSpeed * Time.deltaTime;
    }
}
