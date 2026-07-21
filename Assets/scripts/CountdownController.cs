using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using TMPro;

/// <summary>
/// レース開始前のカウントダウン演出（3→2→1→GO!）。
/// カウントダウン中は Penguin3DController / PenguinNPCController を自動検出して操作を止め、
/// 終了と同時に操作を有効化する。
///
/// 【Hierarchy構成例】
/// Canvas
///  └ CountdownText (TMP_Text, 大きめのフォント・中央配置)
///
/// 【使い方】
/// 1. プレイシーンの空オブジェクトにこのスクリプトをアタッチ
/// 2. countdownText に上記TMP_Textをセット
/// 3. onCountdownFinished に RaceManager.OnRaceStart をInspectorで登録
///
/// ※ StageGameInitializer が Start() でNPC/2Pの表示切替を行うため、
///   1フレーム待ってからプレイヤーを検出・停止させています。
/// </summary>
public class CountdownController : MonoBehaviour
{
    [Header("UI参照")]
    [SerializeField] private TMP_Text countdownText;

    [Header("カウントダウン設定")]
    [SerializeField] private float numberDisplayDuration = 1f;
    [SerializeField] private float goDisplayDuration = 0.7f;
    [SerializeField] private string goText = "GO!";

    [Header("演出設定（数字が飛び出すようなポップ演出）")]
    [SerializeField] private float popScale = 1.3f;
    [SerializeField] private float popDuration = 0.15f;

    [Header("追加で止めたいスクリプト（任意）")]
    [Tooltip("カメラの導入演出など、ペンギン以外にもカウントダウン中に止めたいスクリプトがあればここに追加")]
    [SerializeField] private MonoBehaviour[] additionalScriptsToDisable;

    [Header("イベント")]
    public UnityEvent onCountdownStart;
    public UnityEvent onCountdownFinished; // RaceManager.OnRaceStart などをここに登録

    private void Start()
    {
        StartCoroutine(RunCountdown());
    }

    private IEnumerator RunCountdown()
    {
        // StageGameInitializerがStart()でNPC/2Pのactive状態を切り替えるので、
        // それが終わってからプレイヤーを検出する
        yield return null;

        SetPlayerControlEnabled(false);
        onCountdownStart?.Invoke();

        for (int count = 3; count >= 1; count--)
        {
            yield return StartCoroutine(ShowNumber(count.ToString(), numberDisplayDuration));
        }

        yield return StartCoroutine(ShowNumber(goText, goDisplayDuration));

        countdownText.gameObject.SetActive(false);
        SetPlayerControlEnabled(true);
        onCountdownFinished?.Invoke();
    }

    private IEnumerator ShowNumber(string text, float duration)
    {
        countdownText.text = text;
        countdownText.transform.localScale = Vector3.one * popScale;

        float t = 0f;
        while (t < popDuration)
        {
            t += Time.deltaTime;
            countdownText.transform.localScale = Vector3.Lerp(
                Vector3.one * popScale, Vector3.one, t / popDuration);
            yield return null;
        }
        countdownText.transform.localScale = Vector3.one;

        float remaining = duration - popDuration;
        if (remaining > 0f)
        {
            yield return new WaitForSeconds(remaining);
        }
    }

    private void SetPlayerControlEnabled(bool value)
    {
        // シーン内のアクティブなペンギン（1P/2P/NPC）をすべて検出して操作を止める/戻す
        var players = FindObjectsByType<Penguin3DController>(FindObjectsInactive.Exclude);
        foreach (var p in players) p.enabled = value;

        var npcs = FindObjectsByType<PenguinNPCController>(FindObjectsInactive.Exclude);
        foreach (var n in npcs) n.enabled = value;

        foreach (var script in additionalScriptsToDisable)
        {
            if (script != null) script.enabled = value;
        }
    }
}
