using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

public class PresidentBattleManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Slider gaugeSlider;
    [SerializeField] private RectTransform targetMarker; // 目標地点を示す四角いマーカー
    [SerializeField] private TMP_Text instructionText;
    [SerializeField] private TMP_Text stageText;
    [SerializeField] private GameObject resultPanel;
    [SerializeField] private TMP_Text resultText;
    [SerializeField] private GameObject openingPanel; // オープニング用の黒背景パネル

    [Header("Player Upgrades (3D Models)")]
    [SerializeField] private GameObject[] playerUpgrades; // 0: Tier1, 1: Tier2, 2: Tier3

    [Header("Camera & Animation演出")]
    [SerializeField] private GameObject[] stageCameras; // ステージごとのカメラ（3つ）
    [SerializeField] private GameObject[] stageLights;  // ステージごとのライト（3つ：各カメラに同期）
    [SerializeField] private Animator[] finalAnimators; // 3回パーフェクト達成時のアニメーター（複数対応）
    [SerializeField] private GameObject finalPlayerCard; // 最後に差し出すプレイヤーの名刺
    [SerializeField] private string animationTrigger = "PerfectWin";

    [Header("Blowback演出")]
    [SerializeField] private Rigidbody blowbackTarget; // 吹き飛ばしたい対象（社長など）のRigidbody
    [SerializeField] private float blowbackForce = 50f; // 吹き飛ばす力を強化 (15->50)
    [SerializeField] private float blowbackUpForce = 0f; // 上に跳ね上げる力
    [SerializeField] private float blowbackDrag = 0.5f;    // 空気抵抗を弱めて遠くまで飛ぶように調整 (2->0.5)
    [SerializeField] private float blowbackAngularDrag = 2f; // 回転の抵抗
    [SerializeField] private bool blowbackUseGravity = false; // 吹っ飛んだ後に落下させるかどうか
    [SerializeField] private bool addBlowbackRotation = false; // 吹っ飛ぶときに回転させるかどうか
    [SerializeField] private bool invertBlowbackDirection = false; // 吹き飛ばす方向を反転させる
    [SerializeField] private string blowbackTriggerName = "Blowback"; // 吹っ飛ぶ瞬間に再生するアニメーションのトリガー名
    
    [Header("名刺表示設定")]
    [SerializeField] private float finalCardDistance = 0.6f; // カメラから名刺までの距離（0.4から0.6へ微増）

    [Header("Settings")]
    [SerializeField] private float perfectTargetTime = 1.0f; // 成功の目標秒数
    [SerializeField] private float gaugeLoopDuration = 2.0f; // ゲージが1周する秒数（2.0sに設定し、1.0sで中央へ）
    [SerializeField] private float perfectThreshold = 0.25f; // Perfectと判定する許容誤差 (0.15から緩和)
    [SerializeField] private float goodThreshold = 0.4f;    // 成功と判定する最大許容誤差
    [SerializeField] private string nextSceneName = "ResultScene";

    private int currentStage = 0;
    private float stageTimer = 0f;
    private int perfectCount = 0;
    private bool isInputLocked = false;
    private bool isBattleFinished = false;

    private void Awake()
    {
        // ログに出ている「There are 2 audio listeners」警告をプログラムで回避する
        // Start時にUpdateCamerasで適切に設定するため、ここでは何もしなくてOKですが、
        // 重複を確実に消すなら既存のものを一度全て検索します。
    }

    private void Start()
    {
        // 初期設定
        gaugeSlider.value = 0;
        stageText.text = $"Stage: {currentStage + 1} / 3";
        instructionText.text = "1秒ジャストで SPACE を押せ！";
        resultPanel.SetActive(false);

        // ScoreManagerが見つからない場合、念のため検索する
        if (ScoreManager.Instance == null)
        {
            var sm = FindFirstObjectByType<ScoreManager>();
            if (sm != null) Debug.Log("[PresidentBattleManager] Found ScoreManager in scene.");
        }

        // 背景のメインBGMを停止（音が重なるのを防ぐ）
        if (AudioManager.Instance != null)
        {
            Debug.Log("[PresidentBattleManager] Stopping main BGM to avoid overlapping.");
            AudioManager.Instance.StopBGM();
        }

        // 社長戦専用BGMを再生
        if (PresidentAudioManager.Instance != null && PresidentAudioManager.Instance.presidentBGM != null)
        {
            PresidentAudioManager.Instance.PlayBGM(PresidentAudioManager.Instance.presidentBGM);
        }

        // オープニング演出開始
        StartCoroutine(ShowOpeningMessage());

        // ターゲットマーカーを1秒の場所に配置
        if (targetMarker != null)
        {
            float normalizedTarget = perfectTargetTime / gaugeLoopDuration;
            targetMarker.anchorMin = new Vector2(normalizedTarget, 0.5f);
            targetMarker.anchorMax = new Vector2(normalizedTarget, 0.5f);
            targetMarker.anchoredPosition = Vector2.zero;
        }

        // アップグレード品を非表示に
        foreach (var upgrade in playerUpgrades)
        {
            if (upgrade != null) upgrade.SetActive(false);
        }

        // 最後に表示する名刺も最初は非表示に
        if (finalPlayerCard != null) finalPlayerCard.SetActive(false);

        UpdateCameras();
    }

    private System.Collections.IEnumerator ShowOpeningMessage()
    {
        // 入力をロックしてメッセージに集中させる
        isInputLocked = true;
        stageTimer = 0f; // タイマーも止める（または0にしておく）

        // 黒背景を表示
        if (openingPanel != null) 
        {
            openingPanel.SetActive(true);
            // 黒背景が表示された後、テキストが隠れないようにテキストを最前面（描画順の最後）に持ってくる
            if (instructionText != null)
            {
                instructionText.transform.SetAsLastSibling();
            }
        }

        // 昇進メッセージを表示
        if (instructionText != null)
        {
            instructionText.text = "副社長になり社長への挑戦権を手に入れた";
        }

        // 3秒待機（ユーザーが読む時間）
        yield return new WaitForSeconds(3.0f);

        // 黒背景を非表示
        if (openingPanel != null) openingPanel.SetActive(false);

        // ゲーム説明に切り替え
        if (instructionText != null)
        {
            instructionText.text = "君の実力を見せてみろ... (Spaceで止めろ！)";
        }

        // 入力ロック解除
        isInputLocked = false;
        stageTimer = 0f; // 開始タイミングをリセット
    }

    private void Update()
    {
        if (isBattleFinished || isInputLocked) return;

        stageTimer += Time.deltaTime;
        
        // 設定されたループ周期でゲージを表示
        float visualValue = (stageTimer % gaugeLoopDuration) / gaugeLoopDuration; 
        gaugeSlider.value = visualValue;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            CheckTiming();
        }
    }

    private void CheckTiming()
    {
        isInputLocked = true;
        
        float diff = Mathf.Abs(stageTimer - perfectTargetTime);
        Debug.Log($"Stage {currentStage + 1} Timing Diff: {diff}s (Timer: {stageTimer}s)");

        if (diff <= perfectThreshold)
        {
            perfectCount++;
            StartCoroutine(OnStageResult("PERFECT!!", true));
        }
        else if (diff <= goodThreshold)
        {
            StartCoroutine(OnStageResult("SUCCESS (GOOD)", false));
        }
        else
        {
            StartCoroutine(OnBattleFailure());
        }
    }

    private IEnumerator OnStageResult(string label, bool isPerfect)
    {
        if (instructionText != null) instructionText.text = label;
        
        // 成功報酬の表示
        if (currentStage < playerUpgrades.Length && playerUpgrades[currentStage] != null)
        {
            playerUpgrades[currentStage].SetActive(true);
        }

        if (PresidentAudioManager.Instance != null)
        {
            // 社長戦専用成功音を優先、なければメインのRankUp音
            AudioClip clip = PresidentAudioManager.Instance.presidentSuccessSound;
            if (clip == null && AudioManager.Instance != null) clip = AudioManager.Instance.rankUpSound;
            
            if (clip != null)
            {
                PresidentAudioManager.Instance.PlaySFX(clip);
            }
        }

        yield return new WaitForSeconds(1.5f);

        currentStage++;

        if (currentStage >= 3)
        {
            FinishBattle();
        }
        else
        {
            // 次のステージへ
            stageTimer = 0f;
            stageText.text = $"Stage: {currentStage + 1} / 3";
            instructionText.text = "次も1秒ジャストを狙え！";
            UpdateCameras();
            isInputLocked = false;
        }
    }

    private void UpdateCameras()
    {
        if (stageCameras == null || stageCameras.Length == 0) return;

        // currentStageが3（終了後）の場合は、最後のカメラをそのまま使う
        int activeCamIndex = Mathf.Min(currentStage, stageCameras.Length - 1);

        for (int i = 0; i < stageCameras.Length; i++)
        {
            if (stageCameras[i] != null)
            {
                bool isActive = (i == activeCamIndex);
                stageCameras[i].SetActive(isActive);

                // アクティブなカメラのAudioListenerだけを有効にし、他は無効にする
                AudioListener listener = stageCameras[i].GetComponent<AudioListener>();
                if (listener != null)
                {
                    listener.enabled = isActive;
                }
            }

            // ライトの動的制御（シャドウアトラスの負荷軽減）
            if (stageLights != null && i < stageLights.Length && stageLights[i] != null)
            {
                bool isLightActive = (i == activeCamIndex);
                stageLights[i].SetActive(isLightActive);
                
                // 強力なシャドウアトラス対策：アクティブでないステージのライトのシャドウを完全にオフにする
                Light lightComp = stageLights[i].GetComponent<Light>();
                if (lightComp != null)
                {
                    lightComp.shadows = isLightActive ? LightShadows.Soft : LightShadows.None;
                }
            }
        }

        // さらに、独自のステージ管理外にあるシーン内の全ライトに対してもシャドウを制限する（オプション）
        // 大量の警告が出るのを防ぐため、アクティブなステージライト以外はシャドウを無効化
        Light[] allLights = FindObjectsByType<Light>(FindObjectsSortMode.None);
        foreach (var l in allLights)
        {
            bool isStageLight = false;
            if (stageLights != null)
            {
                foreach (var sl in stageLights)
                {
                    if (l.gameObject == sl) { isStageLight = true; break; }
                }
            }
            
            // ステージ管理下のライトで、かつ現在非アクティブなカメラのものは既に上で処理済み
            // ここではステージ管理外（環境ライト等）のシャドウを落としてアトラスの空きを作る
            if (!isStageLight)
            {
                // メインのディレクショナルライト等は残したいかもしれないが、
                // 警告が出る場合はこれらも制限対象にする
                if (l.type != LightType.Directional)
                {
                    l.shadows = LightShadows.None;
                }
            }
        }
    }

    private IEnumerator OnBattleFailure()
    {
        if (instructionText != null) instructionText.text = "タイミングを外した...";
        if (PresidentAudioManager.Instance != null)
        {
            if (PresidentAudioManager.Instance.presidentFailureSound != null)
            {
                PresidentAudioManager.Instance.PlaySFX(PresidentAudioManager.Instance.presidentFailureSound);
            }
            else if (AudioManager.Instance != null)
            {
                // メイン側のダメージ音を借りる
                AudioManager.Instance.PlayHeartHurt2();
            }
        }

        yield return new WaitForSeconds(2f);
        FinishBattle();
    }

    private void FinishBattle()
    {
        isBattleFinished = true;
        Debug.Log($"President Battle Finished! Total Perfects: {perfectCount} / 3");
        
        if (perfectCount == 3)
        {
            Debug.Log("Condition Met: All 3 are PERFECT! Starting Ultimate Ending Coroutine...");
            StartCoroutine(AllPerfectEnding());
        }
        else
        {
            Debug.Log($"Condition NOT Met. Perfects: {perfectCount}. Showing Result Panel.");
            resultPanel.SetActive(true);
            if (perfectCount > 0)
            {
                resultText.text = $"素晴らしい。だが真の極致には届かぬか...\n(Perfect: {perfectCount}/3)";
            }
            else
            {
                resultText.text = "社長の威圧感に敗れた...\n修行し直してこい！";
            }

            // 通常リザルトへ（BGMは止める）
            if (PresidentAudioManager.Instance != null) PresidentAudioManager.Instance.StopBGM();
            if (AudioManager.Instance != null) AudioManager.Instance.StopBGM();
            Invoke(nameof(LoadResultScene), 4f);
        }
    }

    private IEnumerator AllPerfectEnding()
    {
        if (instructionText != null) instructionText.text = "ULTIMATE PERFECT!!!";
        
        // 3連Perfect達成時、特別な音楽とアニメーションを同期
        if (PresidentAudioManager.Instance != null && PresidentAudioManager.Instance.presidentVictoryBGM != null)
        {
            // 現在の緊張BGMを止めて勝利BGMを流す
            PresidentAudioManager.Instance.PlayBGM(PresidentAudioManager.Instance.presidentVictoryBGM);
        }

        // 【改善】待機時間を0にし、曲の開始と同時にアクションを開始するように変更
        // yield return new WaitForSeconds(0.5f);

        // すべてのアニメーターに対して特別なアニメーションを発動
        if (finalAnimators != null && finalAnimators.Length > 0)
        {
            foreach (var anim in finalAnimators)
            {
                if (anim == null) continue;

                // デバッグ：Animatorが持っている全パラメータを出力
                System.Text.StringBuilder sb = new System.Text.StringBuilder();
                foreach (var p in anim.parameters) sb.Append(p.name).Append(", ");
                // Debug.Log($"[FINAL] Checking Animator on {anim.name}. Available: {sb.ToString()}"); // ログを減らすためコメントアウト

                if (HasParameter(anim, animationTrigger))
                {
                    Debug.Log($"[FINAL] Triggering Animator: {anim.name} with trigger: {animationTrigger}");
                    anim.SetTrigger(animationTrigger);
                    // 【究極】トリガーが効かない場合を考え、ステート名としても強制再生
                    anim.Play(animationTrigger, 0, 0f);
                    anim.Update(0f); // 即時ポーズ反映
                }
                else if (!string.IsNullOrEmpty(blowbackTriggerName) && HasParameter(anim, blowbackTriggerName))
                {
                    Debug.Log($"[FINAL] Fallback: '{animationTrigger}' not found on {anim.name}. Using '{blowbackTriggerName}' instead.");
                    anim.SetTrigger(blowbackTriggerName);
                    anim.Play(blowbackTriggerName, 0, 0f); // 強制再生
                    anim.Update(0f); // 即時ポーズ反映
                }
                else
                {
                    // パラメータがなくても、とりあえずアニメーターを動かす
                    Debug.LogWarning($"[FINAL] No suitable parameter found on {anim.name}. Forced animation refresh.");
                    anim.Update(0.1f); 
                }
                
                if (anim.runtimeAnimatorController == null)
                    Debug.LogWarning($"[FINAL] Animator Controller is MISSING in {anim.name}!");
            }
        }
        else
        {
            Debug.LogWarning("Final Animators is EMPTY! Assign them in the Inspector.");
        }

        // アニメーション開始と同時に名刺と吹っ飛びを開始（待機なし）
        // yield return new WaitForSeconds(0.1f);
        
        Debug.Log("[FINAL] Animation & Blowback simultaneously starting.");

        if (resultPanel != null) 
        {
            resultPanel.SetActive(true);
            Debug.Log("[FINAL] Result Panel activated.");
        }
        
        if (instructionText != null) 
        {
            instructionText.text = "伝説の名刺交換...！\n君こそが、真の社長だ。";
            instructionText.gameObject.SetActive(true);
        }

        if (finalPlayerCard != null)
        {
            // プレハブ（Project上の資産）をアサインしていないかチェック
            if (!finalPlayerCard.gameObject.scene.IsValid())
            {
                Debug.LogError("[FINAL] CRITICAL: finalPlayerCard is a PREFAB! Please set a scene object.");
            }

            // 【超重要】名刺を現在アクティブなカメラのすぐ前に移動させる
            Camera activeCam = Camera.main;
            // Camera.main が見つからない場合は stageCameras から探す
            if (activeCam == null && stageCameras != null)
            {
                foreach(var camObj in stageCameras)
                {
                    if (camObj != null && camObj.activeInHierarchy)
                    {
                        activeCam = camObj.GetComponent<Camera>();
                        if (activeCam != null) break;
                    }
                }
            }

            if (activeCam != null)
            {
                // カメラの正面指定距離（finalCardDistance）の位置に配置
                finalPlayerCard.transform.position = activeCam.transform.position + activeCam.transform.forward * finalCardDistance;
                // カメラの方を向かせる
                finalPlayerCard.transform.rotation = activeCam.transform.rotation;
                Debug.Log($"[FINAL] Forced card position to {finalCardDistance}m in front of camera: {activeCam.name}");
            }

            finalPlayerCard.SetActive(true);
            
            // レンダラーがオフになっていないか、などを見越して強制的に見えるようにする
            var renderers = finalPlayerCard.GetComponentsInChildren<Renderer>(true);
            foreach (var r in renderers) r.enabled = true;

            Debug.Log($"[FINAL] Final Player Card ({finalPlayerCard.name}) set to ACTIVE");

            if (finalPlayerCard.transform.localScale == Vector3.zero)
                finalPlayerCard.transform.localScale = Vector3.one;

            // --- 追加：吹き飛ばし演出の発動 ---
            ApplyBlowback();
        }
        else
        {
            Debug.LogError("[FINAL] finalPlayerCard is NULL! Assign it in the Inspector.");
        }

        yield return new WaitForSeconds(3f);
        LoadResultScene();
    }

    private void LoadResultScene()
    {
        SceneManager.LoadScene(nextSceneName);
    }

    /// <summary>
    /// 対象を物理的に吹き飛ばす
    /// </summary>
    private void ApplyBlowback()
    {
        if (blowbackTarget == null)
        {
            Debug.Log("[FINAL] Blowback target is not assigned. Skipping blowback.");
            return;
        }

        // キネマティックを解除して物理演算を開始
        blowbackTarget.isKinematic = false;
        blowbackTarget.constraints = RigidbodyConstraints.None; // すべての回転・移動ロックを解除

        // 【究極】もし親オブジェクトによって動きが制限されているなら、親子関係を解除する
        if (blowbackTarget.transform.parent != null)
        {
            Debug.Log($"[FINAL] Unparenting {blowbackTarget.name} from {blowbackTarget.transform.parent.name} to allow 100% free movement.");
            // 吹っ飛ぶ瞬間に一族から離脱させる（完全に自由な物理ボディにする）
            blowbackTarget.transform.SetParent(null);
        }

        // 【新機能】物理演算（AddForce）を邪魔するコンポーネントを一時的に無効化する
        var agent = blowbackTarget.GetComponent<UnityEngine.AI.NavMeshAgent>();
        if (agent == null) agent = blowbackTarget.GetComponentInParent<UnityEngine.AI.NavMeshAgent>();
        if (agent != null) agent.enabled = false;
        
        var charCtrl = blowbackTarget.GetComponent<CharacterController>();
        if (charCtrl == null) charCtrl = blowbackTarget.GetComponentInParent<CharacterController>();
        if (charCtrl != null) charCtrl.enabled = false;

        // Concave Mesh Collider対策
        MeshCollider[] meshColliders = blowbackTarget.GetComponentsInChildren<MeshCollider>();
        foreach (var mc in meshColliders)
        {
            if (!mc.convex)
            {
                mc.convex = true;
                Debug.Log($"[FINAL] Forced MeshCollider on {mc.name} to CONVEX to avoid Physics error.");
            }
        }
        
        // 【追加】吹っ飛ぶ対象にアニメーターがあれば、指定したトリガーを引く
        Animator targetAnim = blowbackTarget.GetComponent<Animator>();
        if (targetAnim == null) targetAnim = blowbackTarget.GetComponentInChildren<Animator>(); // 子オブジェクトも探す

        if (targetAnim != null)
        {
            // デバッグログ：Animatorが持っているパラメータをすべて出力して、名前が合っているか確認しやすくする
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            foreach (var p in targetAnim.parameters) sb.Append(p.name).Append(", ");
            Debug.Log($"[FINAL] Animator found on {targetAnim.name}. Parameters: {sb.ToString()}");

            // RootMotionをオフにしないと、物理による吹っ飛び（AddForce）よりもアニメーション内の移動が優先されてしまいます
            targetAnim.applyRootMotion = false;
            
            if (!string.IsNullOrEmpty(blowbackTriggerName))
            {
                // パラメータが存在するかチェックしてから実行
                if (HasParameter(targetAnim, blowbackTriggerName))
                {
                    targetAnim.SetTrigger(blowbackTriggerName);
                    // 強制再生も併用
                    targetAnim.Play(blowbackTriggerName, 0, 0f);
                    // 【即時反映】次フレームを待たずに今すぐポーズを確定させる
                    targetAnim.Update(0f);
                    Debug.Log($"[FINAL] EXECUTE: Set trigger & Play state & Force Update on {targetAnim.name}");
                }
                else
                {
                    Debug.LogWarning($"[FINAL] Trigger '{blowbackTriggerName}' not found on {targetAnim.name}. Skipping animation trigger. Available: {sb.ToString()}");
                }
            }
        }
        else
        {
            Debug.LogWarning($"[FINAL] Animator NOT FOUND on {blowbackTarget.name} or its children.");
        }
        
        // 【修正】ユーザー設定に合わせて重力の有無を切り替える
        blowbackTarget.useGravity = blowbackUseGravity;

        // 空気抵抗の設定（Unity 6 以降なら linearDamping / 以前なら drag）
        // ユーザーの環境（Unity 6）に合わせてプロパティ名を定義
        #if UNITY_6_0_OR_NEWER
        blowbackTarget.linearDamping = blowbackDrag;
        blowbackTarget.angularDamping = blowbackAngularDrag;
        #else
        // 旧バージョンでも動作を確保（Unity 6 なら自動的に linearDamping にマッピングされますが、念のため記述）
        try {
            blowbackTarget.linearDamping = blowbackDrag;
            blowbackTarget.angularDamping = blowbackAngularDrag;
        } catch {
            // 万が一旧仕様のままでエラーになる場合のみ drag を使用
            #pragma warning disable CS0618
            blowbackTarget.drag = blowbackDrag;
            blowbackTarget.angularDrag = blowbackAngularDrag;
            #pragma warning restore CS0618
        }
        #endif

        // 【修正】斜め後ろではなく、後ろ（+ユーザー設定の上方向）へ力を加える
        // モデルの向きによって逆転（前に来る）する場合は invertBlowbackDirection をオンにする
        float directionMultiplier = invertBlowbackDirection ? 1f : -1f;
        Vector3 forceDirection = (blowbackTarget.transform.forward * blowbackForce * directionMultiplier) + (Vector3.up * blowbackUpForce);
        blowbackTarget.AddForce(forceDirection, ForceMode.Impulse);

        // 【修正】回転させる設定の時だけトルクを加える
        if (addBlowbackRotation)
        {
            Vector3 randomTorque = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f)) * 10f;
            blowbackTarget.AddTorque(randomTorque, ForceMode.Impulse);
        }

        Debug.Log($"[FINAL] Applied blowback to {blowbackTarget.name} with force {forceDirection}");
    }

    /// <summary>
    /// Animatorに特定のパラメータが存在するか確認するヘルパー
    /// </summary>
    private bool HasParameter(Animator animator, string paramName)
    {
        if (animator == null) return false;
        foreach (AnimatorControllerParameter param in animator.parameters)
        {
            if (param.name == paramName) return true;
        }
        return false;
    }
}
