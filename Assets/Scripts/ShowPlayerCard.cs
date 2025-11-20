using UnityEngine;

public class ShowPlayerCard : MonoBehaviour
{
    public GameObject playerCard;  // 表示/非表示する名刺（3D）

    void Start()
    {
        // ゲーム開始時は名刺を非表示
        if (playerCard != null)
            playerCard.SetActive(false);
    }

    void Update()
    {
        // Fキーを押したらオン/オフ切り替え
        if (Input.GetKeyDown(KeyCode.F))
        {
            if (playerCard != null)
                playerCard.SetActive(!playerCard.activeSelf);
        }
    }
}
