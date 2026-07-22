using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// レースの進行管理。タイム計測、1P・対戦相手（NPCまたは2P）のゴール判定、
/// 両者がゴールした時点での勝者確定とリザルト表示への引き渡しを行う。
/// レース中は左上のタイマーを表示し、終了したら非表示にしてリザルト側（中央）に一本化する。
/// </summary>
public class RaceManager : MonoBehaviour
{
    [Header("プレイヤー参照（対応しない方は空でOK）")]
    [SerializeField] private Penguin3DController player1;  // 1P（人間操作）
    [SerializeField] private Penguin3DController player2;  // 2P（人間操作、vs2Pモード用）
    [SerializeField] private PenguinNPCController npc;      // NPC（vsNPCモード用）

    [Header("表示名（リザルト画面に出す名前）")]
    [SerializeField] private string player1Name = "1P";
    [SerializeField] private string player2Name = "2P";
    [SerializeField] private string npcName = "NPC";

    [Header("参照")]
    [SerializeField] private ResultController resultController;
    [SerializeField] private Text timeText; // レース中（左上）のタイム表示。Text/Legacy

    private bool isRacing = false;
    private bool raceFinished = false;
    private float raceStartTime;
    private int currentMode; // OnRaceStart時点のGameMode（0:NPC 1:2P）

    // 1P側のゴール状況
    private bool player1Finished = false;
    private float player1FinishTime;

    // 対戦相手側（NPC or 2P）のゴール状況
    private bool opponentFinished = false;
    private float opponentFinishTime;

    /// <summary>
    /// CountdownController.onCountdownFinished から呼び出す
    /// </summary>
    public void OnRaceStart()
    {
        isRacing = true;
        raceFinished = false;
        raceStartTime = Time.time;
        currentMode = PlayerPrefs.GetInt("GameMode", 0); // 0:NPC 1:2P

        player1Finished = false;
        opponentFinished = false;

        if (timeText != null) timeText.gameObject.SetActive(true);
    }

    private void Update()
    {
        if (!isRacing || raceFinished) return;

        UpdateTimeDisplay();

        // 1Pのゴール検出（一度ゴールしたら二度と上書きしない）
        if (!player1Finished && player1 != null && player1.isGoal)
        {
            player1Finished = true;
            player1FinishTime = Time.time - raceStartTime;
        }

        // 対戦相手（モードに応じてNPC or 2P）のゴール検出
        if (!opponentFinished)
        {
            bool opponentGoaled = currentMode == 0
                ? (npc != null && npc.isGoal)
                : (player2 != null && player2.isGoal);

            if (opponentGoaled)
            {
                opponentFinished = true;
                opponentFinishTime = Time.time - raceStartTime;
            }
        }

        // 両者ともゴールしたらリザルト確定
        if (player1Finished && opponentFinished)
        {
            FinishRace();
        }
    }

    private void UpdateTimeDisplay()
    {
        if (timeText == null) return;
        timeText.text = FormatTime(Time.time - raceStartTime);
    }

    private void FinishRace()
    {
        raceFinished = true;
        isRacing = false;

        // 先にゴールした方が勝者
        bool player1Wins = player1FinishTime <= opponentFinishTime;
        string winnerName = player1Wins
            ? player1Name
            : (currentMode == 0 ? npcName : player2Name);
        float winnerTime = player1Wins ? player1FinishTime : opponentFinishTime;

        // 左上のレース中タイマーは消し、リザルト側（中央）だけ表示する
        if (timeText != null)
        {
            timeText.gameObject.SetActive(false);
        }

        if (resultController != null)
        {
            resultController.ShowResult(winnerName, winnerTime);
        }
    }

    /// <summary>
    /// 秒数を "M:SS:m" 形式（分:秒:1/100秒）に変換する（ResultControllerと同じ表記）
    /// </summary>
    private string FormatTime(float seconds)
    {
        int minutes = Mathf.FloorToInt(seconds / 60f);
        int secs = Mathf.FloorToInt(seconds % 60f);
        int centis = Mathf.FloorToInt((seconds * 100f) % 100f);

        return $"{minutes}:{secs:00}:{centis:00}";
    }
}
