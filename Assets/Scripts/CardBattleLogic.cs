using UnityEngine;
using UnityEngine.SceneManagement;

public class CardBattleLogic : MonoBehaviour
{
    [Header("Card References")]
    public Transform playerCard;
    public Transform npcCard;
    
    public CardScrollController playerController; // プレイヤーの操作スクリプト

    [Header("NPC Settings")]
    public Animator npcAnimator; // NPCのアニメーター（あれば設定）
    public string animationTriggerName = "MoveCard"; // アニメーションのトリガー名
    private float npcMoveDuration = 1.0f; // NPCが動く時間
    private Vector3 npcInitialPosition; // NPCカードの初期位置

    [Header("Exchange Settings")]
    [Tooltip("総交換回数")]
    public int totalExchanges = 10;
    
    [Tooltip("1回の交換制限時間(秒)")]
    public float exchangeTimeLimit = 2.0f;
    
    [Tooltip("次の交換までの待機時間(秒)")]
    public float nextExchangeDelay = 1.0f;

    [Header("Score Settings")]
    [Tooltip("成功時のスコア")]
    public int successScore = 100;
    
    [Tooltip("失敗時のスコア")]
    public int failureScore = -100;

    [Header("Transition Settings")]
    [Tooltip("バトル開始演出のコントローラー")]
    public BattleStartTransition transitionController;
    
    [Tooltip("リザルトシーンの名前")]
    public string resultSceneName = "ResultScene";
    
    [Tooltip("リザルト画面に遷移するまでの待機時間(秒)")]
    public float resultTransitionDelay = 2.0f;

    [Header("UI Settings")]
    [Tooltip("バトルUI管理スクリプト")]
    public BattleUIManager uiManager;

    // NPC役職の定義
    public enum NPCRank { Top, Middle, Bottom }
    
    // 状態管理
    private enum BattleState { WaitingForTransition, PlayerTurn, Judging, NextExchange, AllComplete }
    private BattleState currentState = BattleState.WaitingForTransition;

    // 連続交換用の変数
    private int currentExchangeCount = 0; // 現在の交換回数（0〜9）
    private NPCRank currentNPCRank; // 現在のNPCの役職
    private float exchangeTimer = 0f; // 現在の交換の経過時間
    private float npcMoveTimer = 0f; // NPC移動タイマー
    private bool transitionCompleted = false;

    // 統計用
    private int successCount = 0;
    private int failureCount = 0;

    void Start()
    {
        // NPCカードの初期位置を保存
        if (npcCard != null)
        {
            npcInitialPosition = npcCard.position;
        }

        // プレイヤーコントローラーのイベントを登録
        if (playerController != null)
        {
            playerController.OnSubmit += OnPlayerSubmit;
        }

        // 最初の交換を開始
        StartNewExchange();
    }

    void Update()
    {
        switch (currentState)
        {
            case BattleState.WaitingForTransition:
                // 演出が完了するまで待機
                if (transitionController != null && !transitionCompleted)
                {
                    transitionCompleted = transitionController.IsTransitionComplete();
                    if (transitionCompleted)
                    {
                        // 演出完了後、プレイヤーターンに移行
                        currentState = BattleState.PlayerTurn;
                        exchangeTimer = 0f;
                        if (playerController != null) playerController.canMove = true;
                    }
                }
                break;

            case BattleState.PlayerTurn:
                // 制限時間のカウント
                exchangeTimer += Time.deltaTime;
                
                // UI更新
                if (uiManager != null)
                {
                    uiManager.UpdateTimer(exchangeTimeLimit - exchangeTimer);
                }

                // 制限時間切れ
                if (exchangeTimer >= exchangeTimeLimit)
                {
                    Debug.Log("Time's up! Auto-submit as failure.");
                    JudgeExchange(-1); // -1 = タイムアウト（失敗扱い）
                }
                break;

            case BattleState.Judging:
                // NPC移動アニメーション中
                npcMoveTimer += Time.deltaTime;

                // 簡易アニメーション（アニメーターがない場合）
                if (npcAnimator == null)
                {
                    npcCard.position -= new Vector3(0, 0.5f * Time.deltaTime, 0);
                }

                // 指定時間経過したら次の交換へ
                if (npcMoveTimer >= npcMoveDuration)
                {
                    currentState = BattleState.NextExchange;
                    Invoke(nameof(PrepareNextExchange), nextExchangeDelay);
                }
                break;

            case BattleState.NextExchange:
                // 次の交換準備中（Invokeで呼ばれるまで待機）
                break;

            case BattleState.AllComplete:
                // 全交換完了（リザルト画面遷移待ち）
                break;
        }
    }

    /// <summary>
    /// 新しい交換を開始
    /// </summary>
    void StartNewExchange()
    {
        // NPCカードを初期位置にリセット
        if (npcCard != null)
        {
            npcCard.position = npcInitialPosition;
        }

        // Animatorの状態をリセット（念のため）
        if (npcAnimator != null)
        {
            npcAnimator.Rebind();
            npcAnimator.Update(0f);
        }

        // NPC役職をランダムに決定
        currentNPCRank = (NPCRank)Random.Range(0, 3);
        
        Debug.Log($"Exchange {currentExchangeCount + 1}/{totalExchanges} started. NPC Rank: {currentNPCRank}");

        // UI更新
        if (uiManager != null)
        {
            uiManager.UpdateExchangeCount(currentExchangeCount + 1, totalExchanges);
            uiManager.UpdateNPCRank(currentNPCRank);
            uiManager.UpdateTimer(exchangeTimeLimit);
        }

        // 初期状態設定
        exchangeTimer = 0f;
        npcMoveTimer = 0f;

        // プレイヤーコントローラーをリセット
        if (playerController != null)
        {
            playerController.ResetForNextExchange();
            playerController.canMove = false; // 演出完了まで待機
        }

        // 演出がある場合は待機、ない場合は即座にプレイヤーターンへ
        if (transitionController != null && currentExchangeCount == 0)
        {
            currentState = BattleState.WaitingForTransition;
        }
        else
        {
            currentState = BattleState.PlayerTurn;
            if (playerController != null) playerController.canMove = true;
        }
    }

    /// <summary>
    /// プレイヤーがスペースキーを押したときの処理
    /// </summary>
    void OnPlayerSubmit()
    {
        if (currentState != BattleState.PlayerTurn) return;

        // プレイヤーの位置を取得
        int playerPosition = playerController.GetCurrentPositionIndex();
        
        Debug.Log($"Player submitted. Position: {playerPosition}, NPC Rank: {currentNPCRank}");

        // 判定実行
        JudgeExchange(playerPosition);
    }

    /// <summary>
    /// 交換の判定を行う
    /// </summary>
    /// <param name="playerPosition">プレイヤーの位置（0=上, 1=中, 2=下, -1=タイムアウト）</param>
    void JudgeExchange(int playerPosition)
    {
        currentState = BattleState.Judging;

        // プレイヤーの操作をロック
        if (playerController != null) playerController.canMove = false;

        bool isSuccess = false;

        // タイムアウトでない場合のみ判定
        if (playerPosition != -1)
        {
            // 判定ロジック
            // NPC「上」→ プレイヤー「下」(2) で成功
            // NPC「中」→ プレイヤー「中」(1) で成功
            // NPC「下」→ プレイヤー「上」(0) で成功
            switch (currentNPCRank)
            {
                case NPCRank.Top:
                    isSuccess = (playerPosition == 2); // 下
                    break;
                case NPCRank.Middle:
                    isSuccess = (playerPosition == 1); // 中
                    break;
                case NPCRank.Bottom:
                    isSuccess = (playerPosition == 0); // 上
                    break;
            }
        }

        // スコア加算
        int scoreToAdd = isSuccess ? successScore : failureScore;
        ScoreManager.Instance.AddScore(scoreToAdd);

        // 統計更新
        if (isSuccess)
        {
            successCount++;
            ScoreManager.Instance.ShowResult("Success!");
            Debug.Log("Success! +100 points");
        }
        else
        {
            failureCount++;
            ScoreManager.Instance.ShowResult("Failed!");
            Debug.Log("Failed! -100 points");
        }

        // NPCアニメーション開始
        if (npcAnimator != null)
        {
            npcAnimator.SetTrigger(animationTriggerName);
        }
    }

    /// <summary>
    /// 次の交換の準備
    /// </summary>
    void PrepareNextExchange()
    {
        currentExchangeCount++;

        // 全交換完了チェック
        if (currentExchangeCount >= totalExchanges)
        {
            FinishAllExchanges();
        }
        else
        {
            // 次の交換を開始
            StartNewExchange();
        }
    }

    /// <summary>
    /// 全交換完了処理
    /// </summary>
    void FinishAllExchanges()
    {
        currentState = BattleState.AllComplete;

        Debug.Log($"All exchanges complete! Success: {successCount}, Failure: {failureCount}");

        // 最終結果を保存
        string currentSceneName = SceneManager.GetActiveScene().name;
        ScoreManager.Instance.SaveBattleResult(ScoreManager.Instance.Score, 
            $"Success: {successCount}, Failure: {failureCount}", currentSceneName);
        
        // ScoreManagerに統計情報を保存
        ScoreManager.Instance.CurrentExchangeNumber = totalExchanges;
        ScoreManager.Instance.SuccessCount = successCount;
        ScoreManager.Instance.FailureCount = failureCount;

        // リザルト画面へ遷移
        Invoke(nameof(LoadResultScene), resultTransitionDelay);
    }

    /// <summary>
    /// リザルトシーンをロード
    /// </summary>
    void LoadResultScene()
    {
        Debug.Log($"Loading Result Scene: {resultSceneName}");
        SceneManager.LoadScene(resultSceneName);
    }

    void OnDestroy()
    {
        // イベント登録解除
        if (playerController != null)
        {
            playerController.OnSubmit -= OnPlayerSubmit;
        }
    }
}
