using UnityEngine;

/// <summary>
/// 走っているスピードに合わせて、しっぽのボーンを左右にフリフリ揺らす演出用スクリプト。
/// アニメーション（Animator）が無いモデルでも動くように、LateUpdate でボーンを直接回している。
/// </summary>
[DisallowMultipleComponent]
public class PenguinTailWag : MonoBehaviour
{
    [Header("しっぽのボーン（根元→先端の順）")]
    [Tooltip("空のままなら、下の名前リストから自動で探します")]
    public Transform[] tailBones;

    [Tooltip("自動検索に使う名前（部分一致・根元→先端の順）")]
    public string[] autoFindNames = { "DEF-spine.001", "DEF-spine.002" };

    [Header("揺れ方の設定")]
    [Tooltip("左右の最大振れ幅（度）")]
    public float maxSwingAngle = 22f;
    [Tooltip("スピード1あたり、1秒間に何往復するか")]
    public float wagPerSpeed = 0.35f;
    [Tooltip("往復速度の下限・上限（回/秒）")]
    public float minWagFrequency = 0.5f;
    public float maxWagFrequency = 5f;
    [Tooltip("この速度で振れ幅が最大になる")]
    public float speedForMaxSwing = 12f;
    [Tooltip("これ以下の速度では揺れない（止まっている判定）")]
    public float minSpeedToWag = 0.5f;
    [Tooltip("根元から先端へ伝わる遅れ。しなって見える")]
    [Range(0f, 0.5f)] public float segmentDelay = 0.12f;
    [Tooltip("先端ほど大きく振れる度合い")]
    public float tipSwingMultiplier = 1.3f;
    [Tooltip("揺れ始め／止まる時のなめらかさ")]
    public float blendSpeed = 6f;

    [Header("参照（任意・未設定なら自動取得）")]
    public Rigidbody body;

    private Penguin3DController playerController;
    private PenguinNPCController npcController;

    private Quaternion[] baseRotations;
    private Vector3[] swingAxes;     // 各ボーンのローカル空間での「左右に振る軸」
    private float phase;
    private float currentAmplitude;
    private Vector3 lastPosition;

    void Start()
    {
        if (body == null) body = GetComponentInParent<Rigidbody>();
        playerController = GetComponentInParent<Penguin3DController>();
        npcController = GetComponentInParent<PenguinNPCController>();

        if (tailBones == null || tailBones.Length == 0)
        {
            tailBones = FindTailBones();
        }

        CacheBasePose();
        lastPosition = transform.position;
    }

    /// <summary>名前からしっぽのボーンを探す</summary>
    private Transform[] FindTailBones()
    {
        var found = new System.Collections.Generic.List<Transform>();
        Transform[] all = GetComponentsInChildren<Transform>(true);

        foreach (string keyword in autoFindNames)
        {
            if (string.IsNullOrEmpty(keyword)) continue;

            foreach (Transform t in all)
            {
                if (found.Contains(t)) continue;
                if (t.name.IndexOf(keyword, System.StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    found.Add(t);
                    break;
                }
            }
        }

        if (found.Count == 0)
        {
            Debug.LogWarning($"[PenguinTailWag] {name}: しっぽのボーンが見つかりませんでした。Inspector で Tail Bones を設定してください。");
        }
        return found.ToArray();
    }

    /// <summary>基準の姿勢と、左右に振るための回転軸を記録しておく</summary>
    private void CacheBasePose()
    {
        if (tailBones == null) return;

        baseRotations = new Quaternion[tailBones.Length];
        swingAxes = new Vector3[tailBones.Length];

        for (int i = 0; i < tailBones.Length; i++)
        {
            if (tailBones[i] == null) continue;

            baseRotations[i] = tailBones[i].localRotation;
            // キャラクターの「上方向」をボーンのローカル空間に変換 → この軸で回すと左右に振れる
            swingAxes[i] = (Quaternion.Inverse(tailBones[i].rotation) * transform.up).normalized;
        }
    }

    void LateUpdate()
    {
        if (tailBones == null || tailBones.Length == 0 || baseRotations == null) return;

        // 位置の記録を途切れさせないよう、止まっている時も計算自体は毎回行う
        float speed = GetCurrentSpeed();

        // レース開始前（カウントダウン中）は止まっているものとして扱う
        if (!IsControlActive()) speed = 0f;

        // --- 振れ幅：速いほど大きく振る ---
        float targetAmplitude = speed < minSpeedToWag
            ? 0f
            : maxSwingAngle * Mathf.Clamp01(speed / Mathf.Max(0.01f, speedForMaxSwing));
        currentAmplitude = Mathf.Lerp(currentAmplitude, targetAmplitude, blendSpeed * Time.deltaTime);

        // --- 往復の速さ：走るほど速くフリフリ ---
        float frequency = Mathf.Clamp(speed * wagPerSpeed, minWagFrequency, maxWagFrequency);
        phase += frequency * Mathf.PI * 2f * Time.deltaTime;
        if (phase > Mathf.PI * 2f) phase -= Mathf.PI * 2f;

        for (int i = 0; i < tailBones.Length; i++)
        {
            Transform bone = tailBones[i];
            if (bone == null) continue;

            // 先端ほど遅れて、大きく振れる（しなり）
            float delay = segmentDelay * Mathf.PI * 2f * i;
            float tipRatio = tailBones.Length > 1 ? (float)i / (tailBones.Length - 1) : 0f;
            float scale = Mathf.Lerp(1f, tipSwingMultiplier, tipRatio);

            float angle = currentAmplitude * scale * Mathf.Sin(phase - delay);
            bone.localRotation = baseRotations[i] * Quaternion.AngleAxis(angle, swingAxes[i]);
        }
    }

    /// <summary>水平方向の移動スピードを取得する</summary>
    private float GetCurrentSpeed()
    {
        if (body != null)
        {
            Vector3 v = body.linearVelocity; // 2026年仕様。エラーなら velocity に
            return new Vector3(v.x, 0f, v.z).magnitude;
        }

        // Rigidbody が無い場合は位置の変化量から計算
        Vector3 delta = transform.position - lastPosition;
        lastPosition = transform.position;
        delta.y = 0f;
        return Time.deltaTime > 0f ? delta.magnitude / Time.deltaTime : 0f;
    }

    /// <summary>ペンギンが操作可能な状態か（＝レースが始まっているか）</summary>
    private bool IsControlActive()
    {
        if (playerController != null) return playerController.enabled;
        if (npcController != null) return npcController.enabled;
        return true;
    }
}
