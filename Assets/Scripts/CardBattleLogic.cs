using UnityEngine;
using UnityEngine.SceneManagement;

public class CardBattleLogic : MonoBehaviour
{
    public Transform playerCard;
    public Transform npcCard;
    
    [Tooltip("NPCの真下と判定する水平距離の許容範囲")]
    public float horizontalThreshold = 0.5f;
    
    public CardScrollController playerController; // プレイヤーの操作スクリプト

    [Tooltip("加算するスコア")]
    public int scoreAmount = 100;
    
    [Tooltip("失敗時のスコア")]
    public int failureScore = -999;

    public Animator npcAnimator; // NPCのアニメーター（あれば設定）
    public string animationTriggerName = "MoveCard"; // アニメーションのトリガー名

    private enum BattleState { Waiting, NPCMoving, Finished }
    private BattleState currentState = BattleState.Waiting;

    private float npcMoveTimer = 0f;
    private float npcMoveDuration = 1.0f; // NPCが動く時間

    [Tooltip("アニメーション開始までの待機時間(秒)")]
    public float timeToStart = 3.0f;
    private float startTimer = 0f;

    [Tooltip("バトル開始演出のコントローラー")]
    public BattleStartTransition transitionController;
    private bool transitionCompleted = false;

    [Header("Result Scene")]
    [Tooltip("リザルトシーンの名前")]
    public string resultSceneName = "ResultScene";
    
    [Tooltip("リザルト画面に遷移するまでの待機時間(秒)")]
    public float resultTransitionDelay = 2.0f;

    void Update()
    {
        if (playerCard == null || npcCard == null) return;

        switch (currentState)
        {
            case BattleState.Waiting:
                // 演出が完了するまで待機
                if (transitionController != null && !transitionCompleted)
                {
                    transitionCompleted = transitionController.IsTransitionComplete();
                    if (!transitionCompleted)
                        break; // 演出中は何もしない
                }
                
                // タイマーを加算
                startTimer += Time.deltaTime;
                
                // 指定時間を経過したら開始
                if (startTimer >= timeToStart)
                {
                    StartNPCMove();
                }
                break;
            case BattleState.NPCMoving:
                AnimateNPC();
                break;
            case BattleState.Finished:
                // 何もしない（リセット待ちなど）
                break;
        }
    }

    void StartNPCMove()
    {
        currentState = BattleState.NPCMoving;
        
        // プレイヤーの操作をロック
        if (playerController != null) playerController.canMove = false;
        
        // アニメーターがあればトリガーをセット
        if (npcAnimator != null)
        {
            Debug.Log($"Animator Object: {npcAnimator.gameObject.name} (Active: {npcAnimator.gameObject.activeInHierarchy})");
            if (npcAnimator.runtimeAnimatorController != null)
            {
                Debug.Log($"Controller assigned: {npcAnimator.runtimeAnimatorController.name}");
            }
            else
            {
                Debug.LogError("Controller is NULL at runtime! Please check if the assignment was saved or if this is the correct object.");
            }

            npcAnimator.SetTrigger(animationTriggerName);
        }
        else
        {
            Debug.LogWarning("No Animator assigned! Using fallback script movement.");
        }
        
        Debug.Log("Player ready! NPC turn start.");
    }

    void AnimateNPC()
    {
        npcMoveTimer += Time.deltaTime;

        // 【簡易アニメーション】
        // アニメーターがない場合は、スクリプトで少し下に動かす
        if (npcAnimator == null)
        {
            npcCard.position -= new Vector3(0, 0.5f * Time.deltaTime, 0); 
        }

        // 指定時間経過したら終了
        if (npcMoveTimer >= npcMoveDuration)
        {
            FinishBattle();
        }
    }

    void FinishBattle()
    {
        currentState = BattleState.Finished;

        // 水平距離（X, Z）を計算
        float distance = Vector2.Distance(
            new Vector2(playerCard.position.x, playerCard.position.z),
            new Vector2(npcCard.position.x, npcCard.position.z)
        );

        int battleScore;
        string result;

        // 最終判定
        // 1. NPCの「真下」（水平距離が近い）
        // 2. プレイヤーの方が低い（Y座標が小さい）
        if (distance <= horizontalThreshold && playerCard.position.y < npcCard.position.y)
        {
            battleScore = scoreAmount;
            result = "Success";
            ScoreManager.Instance.AddScore(scoreAmount);
            ScoreManager.Instance.ShowResult("Success!");
            Debug.Log("Success! Score Added.");
        }
        // 失敗：位置が悪い、または高さが高い
        else
        {
            battleScore = failureScore;
            result = "Failure";
            ScoreManager.Instance.AddScore(failureScore);
            ScoreManager.Instance.ShowResult("You die");
            Debug.Log("Failure! Rude placement.");
        }

        // バトル結果を保存
        string currentSceneName = SceneManager.GetActiveScene().name;
        ScoreManager.Instance.SaveBattleResult(battleScore, result, currentSceneName);

        // リザルト画面へ遷移
        Invoke(nameof(LoadResultScene), resultTransitionDelay);
    }

    void LoadResultScene()
    {
        Debug.Log($"Loading Result Scene: {resultSceneName}");
        SceneManager.LoadScene(resultSceneName);
    }
}
