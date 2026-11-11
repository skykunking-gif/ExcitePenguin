using UnityEngine;

/// <summary>
/// ブースト中だけ、体を少し前傾させて左右にゆらゆら振る演出用スクリプト。
/// 当たり判定や転倒判定はルート（Rigidbody）の角度で行われるため、
/// 見た目用の Armature ノードだけを回して物理には一切影響させない。
/// </summary>
[DisallowMultipleComponent]
public class PenguinBoostLean : MonoBehaviour
{
    public enum SwayMode
    {
        Roll, // 左右に傾ける（ペンギンのヨチヨチ歩き風）
        Yaw,  // 左右にひねる
    }

    [Header("動かすノード")]
    [Tooltip("空のままなら、下の名前リストから自動で探します")]
    public Transform bodyRoot;

    [Tooltip("自動検索に使う名前（完全一致・上から順に探す）")]
    public string[] autoFindNames = { "Armature" };

    [Header("ブースト中の姿勢")]
    [Tooltip("前傾する角度（度）")]
    public float leanAngle = 12f;
    [Tooltip("左右に振る角度（度）。±この角度まで振れる")]
    public float swayAngle = 10f;
    [Tooltip("1秒間に何往復するか")]
    public float swayFrequency = 2f;
    [Tooltip("左右の振り方（傾ける／ひねる）")]
    public SwayMode swayMode = SwayMode.Roll;
    [Tooltip("姿勢を変え始め／戻す時のなめらかさ")]
    public float blendSpeed = 6f;
    [Tooltip("前傾の向きが逆だった場合にオンにする")]
    public bool invertLean = false;

    [Header("参照（任意・未設定なら自動取得）")]
    public Penguin3DController controller;

    private Quaternion baseRotation;
    private Vector3 leanAxis; // bodyRoot のローカル空間での「前に倒す軸」
    private Vector3 rollAxis; // 〃「左右に傾ける軸」
    private Vector3 yawAxis;  // 〃「左右にひねる軸」
    private float phase;
    private float currentBlend;

    void Start()
    {
        if (controller == null) controller = GetComponentInParent<Penguin3DController>();

        if (bodyRoot == null) bodyRoot = FindBodyRoot();

        if (bodyRoot == null)
        {
            Debug.LogWarning($"[PenguinBoostLean] {name}: 動かすノードが見つかりませんでした。Inspector で Body Root を設定してください。");
            return;
        }

        baseRotation = bodyRoot.localRotation;
        // キャラクターの各方向を bodyRoot のローカル空間に変換しておく
        Quaternion toLocal = Quaternion.Inverse(bodyRoot.rotation);
        leanAxis = (toLocal * transform.right).normalized;
        rollAxis = (toLocal * transform.forward).normalized;
        yawAxis = (toLocal * transform.up).normalized;
    }

    /// <summary>名前から動かすノードを探す</summary>
    private Transform FindBodyRoot()
    {
        if (autoFindNames == null) return null;

        Transform[] all = GetComponentsInChildren<Transform>(true);
        foreach (string keyword in autoFindNames)
        {
            if (string.IsNullOrEmpty(keyword)) continue;

            foreach (Transform t in all)
            {
                if (t == transform) continue;
                if (string.Equals(t.name, keyword, System.StringComparison.OrdinalIgnoreCase))
                {
                    return t;
                }
            }
        }
        return null;
    }

    void LateUpdate()
    {
        if (bodyRoot == null) return;

        float target = IsBoosting() ? 1f : 0f;
        currentBlend = Mathf.Lerp(currentBlend, target, blendSpeed * Time.deltaTime);

        phase += swayFrequency * Mathf.PI * 2f * Time.deltaTime;
        if (phase > Mathf.PI * 2f) phase -= Mathf.PI * 2f;

        float lean = leanAngle * currentBlend * (invertLean ? -1f : 1f);
        float sway = swayAngle * currentBlend * Mathf.Sin(phase);
        Vector3 swayAxis = swayMode == SwayMode.Roll ? rollAxis : yawAxis;

        bodyRoot.localRotation = baseRotation
            * Quaternion.AngleAxis(lean, leanAxis)
            * Quaternion.AngleAxis(sway, swayAxis);
    }

    /// <summary>ブースト中か（地上でブーストしている時だけ姿勢を変える）</summary>
    private bool IsBoosting()
    {
        if (controller == null) return false;

        // レース開始前（カウントダウン中）はコントローラーが無効化されている
        if (!controller.enabled) return false;

        // 空中で前傾させると、着地角度（転倒判定）を見誤りやすいので地上だけにする
        return controller.isBoosting && controller.isGrounded && !controller.isGoal;
    }
}
