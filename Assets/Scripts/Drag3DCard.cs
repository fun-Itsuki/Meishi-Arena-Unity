using UnityEngine;

public class Drag3DCard : MonoBehaviour
{
    public float moveSpeed = 0.002f;

    void Update()
    {
        float mouseY = Input.GetAxis("Mouse Y");
        Vector3 pos = transform.localPosition;
        pos.y += mouseY * moveSpeed;
        transform.localPosition = pos;
    }
}
