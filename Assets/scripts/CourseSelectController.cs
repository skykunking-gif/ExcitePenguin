using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// コース選択画面の制御。
/// 「1人 / 2人」と「オリジナル / ランダム」を選んでから「ゲームスタート」で該当シーンへ移動する。
/// 選択中のボタンはカラー版、選んでいない方は colorless 版の画像に切り替わる。
/// </summary>
public class CourseSelectController : MonoBehaviour
{
    /// <summary>PlayerPrefs のキー。0:オリジナル（手作り） 1:ランダム（自動生成）</summary>
    public const string CoursePrefsKey = "CourseType";
    /// <summary>PlayerPrefs のキー。0:vs NPC（1人） 1:vs 2P（2人）</summary>
    public const string ModePrefsKey = "GameMode";

    [Header("遷移先シーン名")]
    [SerializeField] private string originalSceneName = "Stage_Manual";
    [SerializeField] private string randomSceneName = "Stage_Auto";

    [Header("UI参照")]
    [SerializeField] private Button onePlayerButton;
    [SerializeField] private Button twoPlayerButton;
    [SerializeField] private Button originalCourseButton;
    [SerializeField] private Button randomCourseButton;
    [SerializeField] private Button startButton;

    [Header("選択中／未選択の画像")]
    [SerializeField] private Sprite onePlayerOn;
    [SerializeField] private Sprite onePlayerOff;
    [SerializeField] private Sprite twoPlayerOn;
    [SerializeField] private Sprite twoPlayerOff;
    [SerializeField] private Sprite originalOn;
    [SerializeField] private Sprite originalOff;
    [SerializeField] private Sprite randomOn;
    [SerializeField] private Sprite randomOff;

    private int mode;   // 0:1人（vs NPC） 1:2人
    private int course; // 0:オリジナル 1:ランダム

    private void Start()
    {
        mode = PlayerPrefs.GetInt(ModePrefsKey, 0);
        course = PlayerPrefs.GetInt(CoursePrefsKey, 0);

        if (onePlayerButton != null) onePlayerButton.onClick.AddListener(() => SetMode(0));
        if (twoPlayerButton != null) twoPlayerButton.onClick.AddListener(() => SetMode(1));
        if (originalCourseButton != null) originalCourseButton.onClick.AddListener(() => SetCourse(0));
        if (randomCourseButton != null) randomCourseButton.onClick.AddListener(() => SetCourse(1));
        if (startButton != null) startButton.onClick.AddListener(OnStartPressed);

        Refresh();
    }

    public void SetMode(int value)
    {
        mode = value;
        Refresh();
    }

    public void SetCourse(int value)
    {
        course = value;
        Refresh();
    }

    /// <summary>ゲームスタート。選んだ内容を保存して、コースのシーンへ移動する</summary>
    public void OnStartPressed()
    {
        PlayerPrefs.SetInt(ModePrefsKey, mode);
        PlayerPrefs.SetInt(CoursePrefsKey, course);
        PlayerPrefs.Save();

        SceneManager.LoadScene(course == 0 ? originalSceneName : randomSceneName);
    }

    /// <summary>選択状態にあわせてボタンの絵を差し替える</summary>
    private void Refresh()
    {
        SetSprite(onePlayerButton, mode == 0 ? onePlayerOn : onePlayerOff);
        SetSprite(twoPlayerButton, mode == 1 ? twoPlayerOn : twoPlayerOff);
        SetSprite(originalCourseButton, course == 0 ? originalOn : originalOff);
        SetSprite(randomCourseButton, course == 1 ? randomOn : randomOff);
    }

    private void SetSprite(Button button, Sprite sprite)
    {
        if (button == null || sprite == null) return;

        Image image = button.image != null ? button.image : button.GetComponent<Image>();
        if (image != null) image.sprite = sprite;
    }
}
