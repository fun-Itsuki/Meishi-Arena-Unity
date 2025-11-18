using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;       // プレイヤー
    public float distance = 3f;    // プレイヤーとの距離
    public float offsetY = 1.5f;   // カメラ高さ
    public float mouseSensitivity = 2f;

    float rotationX = 0f; // 上下回転
    float rotationY = 0f; // 左右回転

    void Start()
    {
        Vector3 angles = transform.eulerAngles;
        rotationX = angles.x;
        rotationY = angles.y;
    }

    void LateUpdate()
    {
        if (target == null) return;

        // マウス入力
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        // 左右回転
        rotationY += mouseX;

        // 上下回転（制限付き）
        rotationX -= mouseY;
        rotationX = Mathf.Clamp(rotationX, -30f, 50f);

        // 回転を適用（pivot 自体を回転する）
        transform.rotation = Quaternion.Euler(rotationX, rotationY, 0f);

        // カメラ位置
        Vector3 pivotPos = target.position + Vector3.up * offsetY;
        transform.position = pivotPos - transform.forward * distance;
    }
}
