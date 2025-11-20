using UnityEngine;

public class CardScrollController : MonoBehaviour
{
    public float depthSpeed = 0.1f; // 1スクロール(1.0)につき0.1動く
    public float moveSpeed = 0.1f;  // マウスドラッグによる上下左右移動速度

    public bool canMove = true; // 外部から操作可能かを制御するフラグ

    void Update()
    {
        if (!canMove) return; // 動かせない時は何もしない

        // 1. スクロールで奥行き（Z軸）操作
        float scroll = Input.mouseScrollDelta.y;
        if (Mathf.Abs(scroll) > 0.001f)
        {
            // 入力値の大きさに関わらず、方向（+1 or -1）だけを取得して一定量動かす
            float direction = Mathf.Sign(scroll);
            transform.position += new Vector3(0, 0, direction * depthSpeed);
        }

        // 2. 左クリック中にマウス移動で上下左右（X, Y軸）操作
        if (Input.GetMouseButton(0))
        {
            float mouseX = Input.GetAxis("Mouse X");
            float mouseY = Input.GetAxis("Mouse Y");

            // マウスの移動量をワールド座標の移動に変換
            Vector3 move = new Vector3(mouseX, mouseY, 0) * moveSpeed;
            transform.position += move;
        }
    }
}
