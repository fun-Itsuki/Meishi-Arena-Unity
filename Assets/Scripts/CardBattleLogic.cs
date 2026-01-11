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

    [Header("Life Settings")]
    [Tooltip("最大ライフ数")]
    public int maxLives = 3;
    
    [Tooltip("1回の交換制限時間(秒)")]
    public float exchangeTimeLimit = 1.5f;
    
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
    public enum NPCRank 
    { 
        Employee,   // 一般社員 (0)
        Shunin,     // 主任 (1)
        Keicho,     // 係長 (2)
        Kacho,      // 課長 (3)
        Jicho,      // 次長 (4)
        Bucho,      // 部長 (5)
        Honbucho,   // 本部長 (6)
        Jomu,       // 常務 (7)
        Senmu,      // 専務 (8)
        Fukushacho  // 副社長 (9)
    }
    
    // 状態管理
    private enum BattleState { WaitingForTransition, PlayerTurn, Judging, NextExchange, AllComplete }
    private BattleState currentState = BattleState.WaitingForTransition;

    // ライフシステム用の変数
    private int remainingLives; // 残りライフ数
    private int totalExchangeCount = 0; // 総交換回数（統計用）
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

        // ライフを初期化
        remainingLives = maxLives;

        // スコアをリセット
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.ResetScore();
        }

        // プレイヤーコントローラーのイベントを登録
        if (playerController != null)
        {
            playerController.OnSubmit += OnPlayerSubmit;
        }

        // BGMを再生
        if (AudioManager.Instance != null)
        {
            // 聖戦用のBGMが指定されていればそれを使用し、無ければデフォルトを再生
            if (AudioManager.Instance.seisenBGM != null)
            {
                AudioManager.Instance.PlayBGM(AudioManager.Instance.seisenBGM);
            }
            else
            {
                AudioManager.Instance.PlayBGM();
            }
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

        // プレイヤーの現在の役職レベルを取得
        int playerLevel = ScoreManager.Instance != null ? ScoreManager.Instance.GetPlayerRankLevel() : 0;
        
        // プレイヤーの役職の上下ひとつずつ（-1, 0, +1）の範囲でランダムに選ぶ
        int npcLevel = playerLevel + Random.Range(-1, 2);
        
        // 範囲 (0～9) に制限
        npcLevel = Mathf.Clamp(npcLevel, 0, 9);
        
        currentNPCRank = (NPCRank)npcLevel;
        
        Debug.Log($"Exchange {totalExchangeCount + 1} started. NPC Rank: {currentNPCRank}, Remaining Lives: {remainingLives}");

        // UI更新
        if (uiManager != null)
        {
            uiManager.UpdateExchangeCount(totalExchangeCount + 1); // 総交換回数のみ表示
            uiManager.UpdateNPCRank(currentNPCRank);
            uiManager.UpdateTimer(exchangeTimeLimit);
            uiManager.UpdateLives(remainingLives); // ライフ表示を更新
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
        if (transitionController != null && totalExchangeCount == 0)
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
            // プレイヤーのランクレベル (0～7)
            int playerLevel = ScoreManager.Instance.GetPlayerRankLevel();
            // NPCのランクレベル (0～9)
            int npcLevel = (int)currentNPCRank;

            if (playerLevel == npcLevel)
            {
                // 同じ役職なら「中」(1) で成功
                isSuccess = (playerPosition == 1);
            }
            else if (playerLevel < npcLevel)
            {
                // プレイヤーが格下なら「下」(2) で成功
                isSuccess = (playerPosition == 2);
            }
            else
            {
                // プレイヤーが格上なら「上」(0) で成功
                isSuccess = (playerPosition == 0);
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
            // ハートが減らなかった（成功）場合の効果音を再生
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayCorrectSound();
            }
        }
        else
        {
            failureCount++;
            int prevLives = remainingLives;
            remainingLives--; // ライフを減らす
            ScoreManager.Instance.ShowResult("Failed!");
            Debug.Log($"Failed! -100 points. Remaining Lives: {remainingLives}");
            
            // ライフUI更新
            if (uiManager != null)
            {
                uiManager.UpdateLives(remainingLives);
            }

            // 効果音再生: 1つ目/2つ目/0(死亡)の順で分岐
            if (AudioManager.Instance != null)
            {
                if (remainingLives <= 0)
                    {   
                        AudioManager.Instance.StopBGM();
                        AudioManager.Instance.PlayHeartDeath();
                    }
                else if (prevLives == 3 && remainingLives == 2)
                {
                    AudioManager.Instance.PlayHeartHurt1(); // 1つ目が減った
                }
                else if (prevLives == 2 && remainingLives == 1)
                {
                    AudioManager.Instance.PlayHeartHurt2(); // 2つ目が減った
                }
            }
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
        totalExchangeCount++;

        // 累計交換数を加算
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.AddTotalExchangedCount(1);
        }

        // ゲームオーバーチェック（ライフ0以下）
        if (remainingLives <= 0)
        {
            FinishGame();
        }
        else
        {
            // 次の交換を開始
            StartNewExchange();
        }
    }

    /// <summary>
    /// ゲーム終了処理
    /// </summary>
    void FinishGame()
    {
        currentState = BattleState.AllComplete;

        Debug.Log($"Game Over! Total Exchanges: {totalExchangeCount}, Success: {successCount}, Failure: {failureCount}");

        // BGMを停止（ハート0で聖戦終了）
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.StopBGM();
        }

        // 最終結果を保存
        string currentSceneName = SceneManager.GetActiveScene().name;
        ScoreManager.Instance.SaveBattleResult(ScoreManager.Instance.Score, 
            $"Success: {successCount}, Failure: {failureCount}", currentSceneName);
        
        // ScoreManagerに統計情報を保存
        ScoreManager.Instance.CurrentExchangeNumber = totalExchangeCount;
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
