using UnityEngine;

public class CardScrollController : MonoBehaviour
{
    [Header("Position Settings")]
    [Tooltip("名刺の「上」位置")]
    public Vector3 topPosition = new Vector3(-12f, 4.5f, 1.63f);
    
    [Tooltip("名刺の「中」位置")]
    public Vector3 middlePosition = new Vector3(-12f, 4.3f, 1.63f);
    
    [Tooltip("名刺の「下」位置")]
    public Vector3 bottomPosition = new Vector3(-12f, 4.1f, 1.63f);
    
    [Tooltip("名刺提出時のZ座標")]
    public float submitZPosition = 4f;

    [Header("Control Settings")]
    public bool canMove = true; // 外部から操作可能かを制御するフラグ

    // 現在の名刺位置（0=上, 1=中, 2=下）
    private int currentPositionIndex = 1; // デフォルトは「中」
    private bool isSubmitted = false; // 提出済みフラグ

    // スペースキー押下イベント
    public System.Action OnSubmit;

    void Start()
    {
        // 初期位置を「中」に設定
        transform.position = middlePosition;
    }

    void Update()
    {
        if (!canMove || isSubmitted)
        {
            // デバッグ: 動けない理由をログ出力
            if (!canMove && Input.anyKeyDown)
            {
                Debug.Log("CardScrollController: canMove is false. Cannot move card.");
            }
            if (isSubmitted && Input.anyKeyDown)
            {
                Debug.Log("CardScrollController: Card already submitted. Cannot move.");
            }
            return;
        }

        // Wキー: 上に移動
        if (Input.GetKeyDown(KeyCode.W))
        {
            Debug.Log("W key pressed - Moving UP");
            currentPositionIndex = Mathf.Max(0, currentPositionIndex - 1);
            UpdatePosition();
        }

        // Sキー: 下に移動
        if (Input.GetKeyDown(KeyCode.S))
        {
            Debug.Log("S key pressed - Moving DOWN");
            currentPositionIndex = Mathf.Min(2, currentPositionIndex + 1);
            UpdatePosition();
        }

        // スペースキー: 名刺を提出
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("Space key pressed - Submitting card");
            SubmitCard();
        }
    }

    /// <summary>
    /// 現在の位置インデックスに応じて名刺の位置を更新
    /// </summary>
    void UpdatePosition()
    {
        Vector3 targetPosition = middlePosition;

        switch (currentPositionIndex)
        {
            case 0:
                targetPosition = topPosition;
                break;
            case 1:
                targetPosition = middlePosition;
                break;
            case 2:
                targetPosition = bottomPosition;
                break;
        }

        transform.position = targetPosition;
        Debug.Log($"Card position updated to index {currentPositionIndex}: {targetPosition}");
    }

    /// <summary>
    /// 名刺を提出する（X座標を変更してイベントを発火）
    /// </summary>
    void SubmitCard()
    {
        isSubmitted = true;
        
        // Z座標を提出位置に変更
        Vector3 beforePosition = transform.position;
        Vector3 submitPosition = transform.position;
        submitPosition.z = submitZPosition;
        transform.position = submitPosition;

        // 提出イベントを発火
        OnSubmit?.Invoke();
        
        Debug.Log($"Card submitted! Position index: {currentPositionIndex}, Before: {beforePosition}, After: {submitPosition}");
    }

    /// <summary>
    /// 次の交換のために状態をリセット
    /// </summary>
    public void ResetForNextExchange()
    {
        isSubmitted = false;
        currentPositionIndex = 1; // 中央にリセット
        transform.position = middlePosition;
    }

    /// <summary>
    /// 現在の位置インデックスを取得（0=上, 1=中, 2=下）
    /// </summary>
    public int GetCurrentPositionIndex()
    {
        return currentPositionIndex;
    }
}
