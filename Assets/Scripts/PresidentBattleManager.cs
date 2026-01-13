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
    [SerializeField] private float blowbackForce = 15f; // 吹き飛ばす力
    [SerializeField] private float blowbackUpForce = 5f; // 上に跳ね上げる力

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
                stageLights[i].SetActive(i == activeCamIndex);
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

        yield return new WaitForSeconds(0.5f);

        // すべてのアニメーターに対して特別なアニメーションを発動
        if (finalAnimators != null && finalAnimators.Length > 0)
        {
            foreach (var anim in finalAnimators)
            {
                if (anim == null) continue;

                if (HasParameter(anim, animationTrigger))
                {
                    Debug.Log($"[FINAL] Triggering Animator: {anim.name} with trigger: {animationTrigger}");
                    anim.SetTrigger(animationTrigger);
                }
                else
                {
                    Debug.LogError($"[FINAL] Animator Parameter '{animationTrigger}' NOT FOUND in {anim.name}!");
                }
                
                if (anim.runtimeAnimatorController == null)
                    Debug.LogWarning($"[FINAL] Animator Controller is MISSING in {anim.name}!");
            }
        }
        else
        {
            Debug.LogWarning("Final Animators is EMPTY! Assign them in the Inspector.");
        }

        // ここではまだリザルトパネルを出さず、アニメーションをしっかり見せる
        // if (resultPanel != null) resultPanel.SetActive(true);
        // if (resultText != null) resultText.text = "伝説の名刺交換...！\n君こそが、真の社長だ。";
        
        // アニメーション開始後、すぐ（0.1秒）に名刺とリザルトを出す
        yield return new WaitForSeconds(0.1f);
        
        Debug.Log("[FINAL] Animation started. Showing results and card.");

        // 白いリザルトパネルは出さず、テキストのみで演出する
        // if (resultPanel != null) 
        // {
        //     resultPanel.SetActive(true);
        //     Debug.Log("[FINAL] Result Panel activated.");
        // }
        
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
                // カメラの正面 0.4m の位置に配置
                finalPlayerCard.transform.position = activeCam.transform.position + activeCam.transform.forward * 0.4f;
                // カメラの方を向かせる
                finalPlayerCard.transform.rotation = activeCam.transform.rotation;
                Debug.Log($"[FINAL] Forced card position to front of camera: {activeCam.name}");
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

        // 重力を有効にし、キネマティックを解除（アニメーション制御から物理制御へ）
        blowbackTarget.isKinematic = false;
        blowbackTarget.useGravity = true;

        // 斜め後ろ（後ろ方向 + 上方向）へ力を加える
        Vector3 forceDirection = (-blowbackTarget.transform.forward * blowbackForce) + (Vector3.up * blowbackUpForce);
        blowbackTarget.AddForce(forceDirection, ForceMode.Impulse);

        // トドメにランダムな回転を加える
        Vector3 randomTorque = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f)) * 10f;
        blowbackTarget.AddTorque(randomTorque, ForceMode.Impulse);

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
