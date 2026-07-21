using UnityEngine;

/// <summary>
/// レースの進行管理役。
/// カウントダウン終了でタイム計測を開始し、いずれかのプレイヤーが
/// ゴール（isGoal == true）した時点で結果を確定してResultControllerに渡す。
///
/// 【使い方】
/// 1. プレイシーンの空オブジェクト（例："RaceManager"）にこのスクリプトをアタッチ
/// 2. player1 に 1P（Penguin3DController）をドラッグ
/// 3. モードに応じて player2（2P用Penguin3DController）または npc（PenguinNPCController）をドラッグ
///    ※片方は使わないので空のままでOK
/// 4. resultController に ResultController をドラッグ
/// 5. CountdownController の onCountdownFinished イベントに
///    この RaceManager.OnRaceStart をInspector上で登録する
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

    private bool isRacing = false;
    private bool raceFinished = false;
    private float raceStartTime;

    /// <summary>
    /// CountdownController.onCountdownFinished から呼び出す
    /// </summary>
    public void OnRaceStart()
    {
        isRacing = true;
        raceFinished = false;
        raceStartTime = Time.time;
    }

    private void Update()
    {
        if (!isRacing || raceFinished) return;

        // MainMenuManagerが保存したGameModeと同じキーを見て、対戦相手を判定する
        int mode = PlayerPrefs.GetInt("GameMode", 0); // 0:NPC 1:2P

        if (player1 != null && player1.isGoal)
        {
            FinishRace(player1Name);
            return;
        }

        if (mode == 0)
        {
            if (npc != null && npc.isGoal)
            {
                FinishRace(npcName);
            }
        }
        else
        {
            if (player2 != null && player2.isGoal)
            {
                FinishRace(player2Name);
            }
        }
    }

    private void FinishRace(string winnerName)
    {
        raceFinished = true;
        isRacing = false;

        float elapsedTime = Time.time - raceStartTime;

        if (resultController != null)
        {
            resultController.ShowResult(winnerName, elapsedTime);
        }
    }
}
