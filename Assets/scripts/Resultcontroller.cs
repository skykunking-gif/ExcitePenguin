using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// ゴール後のリザルト画面。
/// レース終了処理（ゴール判定）側から ShowResult(...) を呼び出すと結果パネルが表示される。
/// 1P・2P（またはNPC）それぞれのカードに、タイムとこけた回数を出す。
/// </summary>
public class ResultController : MonoBehaviour
{
    [Header("UI参照")]
    [SerializeField] private GameObject resultPanel;   // 結果表示パネル（最初は非表示）
    [SerializeField] private Text winnerText;          // 「勝者：AAA」（Text/Legacy）

    [Header("1Pカード")]
    [SerializeField] private Text player1TimeText;
    [SerializeField] private Text player1FallsText;
    [Tooltip("カードに映すペンギン（RenderTexture）")]
    [SerializeField] private RawImage player1PoseImage;

    [Header("2P（またはNPC）カード")]
    [SerializeField] private Text player2TimeText;
    [SerializeField] private Text player2FallsText;
    [SerializeField] private RawImage player2PoseImage;

    [Header("ポーズ用ペンギン（3D・画面外）")]
    [Tooltip("ポーズ用ペンギンとカメラ一式の親")]
    [SerializeField] private GameObject winnerPodium;

    [Tooltip("ガッツポーズ側")]
    [SerializeField] private Camera winnerCamera;
    [SerializeField] private PenguinColorOption winnerColor;
    [SerializeField] private PenguinTailColor winnerTail;

    [Tooltip("残念がっている側")]
    [SerializeField] private Camera loserCamera;
    [SerializeField] private PenguinColorOption loserColor;
    [SerializeField] private PenguinTailColor loserTail;

    [SerializeField] private int poseTextureSize = 512;

    [Header("ボタン")]
    [SerializeField] private Button retryButton;
    [SerializeField] private Button titleButton;

    [Header("シーン名")]
    [SerializeField] private string titleSceneName = "Title";

    private RenderTexture winnerTexture;
    private RenderTexture loserTexture;

    private void Awake()
    {
        if (resultPanel != null) resultPanel.SetActive(false);

        if (retryButton != null) retryButton.onClick.AddListener(OnRetry);
        if (titleButton != null) titleButton.onClick.AddListener(OnBackToTitle);
    }

    /// <summary>
    /// レース終了時に呼び出す。
    /// </summary>
    public void ShowResult(string winnerName, float player1Seconds, int player1Falls,
                           float player2Seconds, int player2Falls)
    {
        if (winnerText != null) winnerText.text = $"勝者：{winnerName}";

        SetText(player1TimeText, FormatTime(player1Seconds));
        SetText(player1FallsText, player1Falls.ToString());
        SetText(player2TimeText, FormatTime(player2Seconds));
        SetText(player2FallsText, player2Falls.ToString());

        // 勝った方にはガッツポーズ、負けた方には残念がっているペンギンを出す
        bool player1Wins = player1Seconds <= player2Seconds;
        SetUpPodium(player1Wins);
        ShowPose(player1PoseImage);
        ShowPose(player2PoseImage);

        if (resultPanel != null) resultPanel.SetActive(true);
    }

    /// <summary>
    /// 画面外のポーズ用ペンギンを起こして、勝者に合わせた色にしてから
    /// RenderTexture に描かせる。
    /// </summary>
    private void SetUpPodium(bool player1Wins)
    {
        if (winnerPodium == null) return;

        winnerPodium.SetActive(true);

        // 勝った方・負けた方、それぞれのペンギンを本人の色にする
        ApplyPenguinLook(winnerColor, winnerTail, player1Wins);
        ApplyPenguinLook(loserColor, loserTail, !player1Wins);

        winnerTexture = PrepareTexture(winnerCamera, winnerTexture, "WinnerPoseTexture");
        loserTexture = PrepareTexture(loserCamera, loserTexture, "LoserPoseTexture");

        if (player1PoseImage != null) player1PoseImage.texture = player1Wins ? winnerTexture : loserTexture;
        if (player2PoseImage != null) player2PoseImage.texture = player1Wins ? loserTexture : winnerTexture;
    }

    /// <summary>1Pならオプションの色、2P・NPCならしっぽが青</summary>
    private void ApplyPenguinLook(PenguinColorOption color, PenguinTailColor tail, bool isPlayer1)
    {
        if (isPlayer1)
        {
            if (color != null) color.Apply(PenguinColorOption.GetSelectedIndex());
        }
        else if (tail != null)
        {
            tail.enableTailColor = true;
            tail.Apply();
        }
    }

    private RenderTexture PrepareTexture(Camera camera, RenderTexture texture, string name)
    {
        if (camera == null) return null;

        if (texture == null)
        {
            texture = new RenderTexture(poseTextureSize, poseTextureSize, 24, RenderTextureFormat.ARGB32);
            texture.name = name;
        }
        camera.targetTexture = texture;
        return texture;
    }

    /// <summary>ペンギンが用意できているカードだけ表示する</summary>
    private void ShowPose(RawImage pose)
    {
        if (pose == null) return;
        pose.gameObject.SetActive(pose.texture != null);
    }

    /// <summary>タイムだけ分かっている場合の簡易版（旧API）</summary>
    public void ShowResult(string winnerName, float raceTimeSeconds)
    {
        ShowResult(winnerName, raceTimeSeconds, 0, raceTimeSeconds, 0);
    }

    private void SetText(Text target, string value)
    {
        if (target != null) target.text = value;
    }

    /// <summary>
    /// 秒数を "M:SS:m" 形式（分:秒:10ミリ秒単位）に変換する
    /// </summary>
    private string FormatTime(float seconds)
    {
        int minutes = Mathf.FloorToInt(seconds / 60f);
        int secs = Mathf.FloorToInt(seconds % 60f);
        int centis = Mathf.FloorToInt((seconds * 100f) % 100f); // 1/100秒

        return $"{minutes}:{secs:00}:{centis:00}";
    }

    private void OnDestroy()
    {
        ReleaseTexture(winnerCamera, winnerTexture);
        ReleaseTexture(loserCamera, loserTexture);
    }

    private void ReleaseTexture(Camera camera, RenderTexture texture)
    {
        if (texture == null) return;
        if (camera != null) camera.targetTexture = null;
        texture.Release();
        Destroy(texture);
    }

    private void OnRetry()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void OnBackToTitle()
    {
        SceneManager.LoadScene(titleSceneName);
    }
}
