using UnityEngine;

/// <summary>
/// 地面から浮いている（空中にいる）間だけ、両腕（翼）を横にパタパタ羽ばたかせる演出用スクリプト。
/// アニメーション（Animator）が無いモデルでも動くように、LateUpdate でボーンを直接回している。
/// </summary>
[DisallowMultipleComponent]
public class PenguinWingFlap : MonoBehaviour
{
    [Header("左翼のボーン（根元→先端の順）")]
    [Tooltip("空のままなら、下の名前リストから自動で探します")]
    public Transform[] leftWingBones;

    [Header("右翼のボーン（根元→先端の順）")]
    public Transform[] rightWingBones;

    [Tooltip("左翼の自動検索に使う名前（部分一致・根元→先端の順）")]
    public string[] autoFindLeftNames = { "DEF-Wing.L", "DEF-Wing.001.L", "DEF-Wing.002.L" };
    [Tooltip("右翼の自動検索に使う名前（部分一致・根元→先端の順）")]
    public string[] autoFindRightNames = { "DEF-Wing.R", "DEF-Wing.001.R", "DEF-Wing.002.R" };

    [Header("羽ばたき方の設定")]
    [Tooltip("パタパタの振れ幅（度）")]
    public float maxFlapAngle = 45f;
    [Tooltip("浮いている間、常にこの角度だけ翼を横に開く（度）")]
    public float spreadAngle = 20f;
    [Tooltip("1秒間に何回パタパタするか")]
    public float flapFrequency = 3f;
    [Tooltip("根元から先端へ伝わる遅れ。しなって見える")]
    [Range(0f, 0.5f)] public float segmentDelay = 0.1f;
    [Tooltip("先端ほど大きく振れる度合い")]
    public float tipFlapMultiplier = 1.4f;
    [Tooltip("羽ばたき始め／閉じる時のなめらかさ")]
    public float blendSpeed = 8f;
    [Tooltip("開く向きが逆だった場合にオンにする")]
    public bool flipDirection = false;

    [Header("浮いている判定（コントローラーが無い時のみ使用）")]
    public LayerMask groundLayer;
    [Tooltip("この距離まで地面が無ければ「浮いている」とみなす")]
    public float groundCheckDistance = 0.9f;

    [Header("参照（任意・未設定なら自動取得）")]
    public Rigidbody body;

    private Penguin3DController playerController;
    private PenguinNPCController npcController;

    private Quaternion[] leftBaseRotations;
    private Quaternion[] rightBaseRotations;
    private Vector3[] leftFlapAxes;   // 各ボーンのローカル空間での「横に開く軸」
    private Vector3[] rightFlapAxes;
    private float phase;
    private float currentBlend;       // 0＝閉じている、1＝完全に羽ばたいている

    void Start()
    {
        if (body == null) body = GetComponentInParent<Rigidbody>();
        playerController = GetComponentInParent<Penguin3DController>();
        npcController = GetComponentInParent<PenguinNPCController>();

        if (leftWingBones == null || leftWingBones.Length == 0)
        {
            leftWingBones = FindBones(autoFindLeftNames);
        }
        if (rightWingBones == null || rightWingBones.Length == 0)
        {
            rightWingBones = FindBones(autoFindRightNames);
        }

        if ((leftWingBones == null || leftWingBones.Length == 0) &&
            (rightWingBones == null || rightWingBones.Length == 0))
        {
            Debug.LogWarning($"[PenguinWingFlap] {name}: 翼のボーンが見つかりませんでした。Inspector で Wing Bones を設定してください。");
        }

        CacheBasePose(leftWingBones, out leftBaseRotations, out leftFlapAxes);
        CacheBasePose(rightWingBones, out rightBaseRotations, out rightFlapAxes);
    }

    /// <summary>名前からボーンを探す</summary>
    private Transform[] FindBones(string[] keywords)
    {
        var found = new System.Collections.Generic.List<Transform>();
        if (keywords == null) return found.ToArray();

        Transform[] all = GetComponentsInChildren<Transform>(true);

        foreach (string keyword in keywords)
        {
            if (string.IsNullOrEmpty(keyword)) continue;

            foreach (Transform t in all)
            {
                if (found.Contains(t)) continue;
                if (string.Equals(t.name, keyword, System.StringComparison.OrdinalIgnoreCase))
                {
                    found.Add(t);
                    break;
                }
            }
        }
        return found.ToArray();
    }

    /// <summary>基準の姿勢と、横に開くための回転軸を記録しておく</summary>
    private void CacheBasePose(Transform[] bones, out Quaternion[] baseRotations, out Vector3[] axes)
    {
        if (bones == null)
        {
            baseRotations = new Quaternion[0];
            axes = new Vector3[0];
            return;
        }

        baseRotations = new Quaternion[bones.Length];
        axes = new Vector3[bones.Length];

        for (int i = 0; i < bones.Length; i++)
        {
            if (bones[i] == null) continue;

            baseRotations[i] = bones[i].localRotation;
            // キャラクターの「前方向」をボーンのローカル空間に変換 → この軸で回すと翼が横に開く
            axes[i] = (Quaternion.Inverse(bones[i].rotation) * transform.forward).normalized;
        }
    }

    void LateUpdate()
    {
        // --- 浮いている間だけ羽ばたく（着地したらなめらかに閉じる）---
        float target = IsAirborne() ? 1f : 0f;
        currentBlend = Mathf.Lerp(currentBlend, target, blendSpeed * Time.deltaTime);

        phase += flapFrequency * Mathf.PI * 2f * Time.deltaTime;
        if (phase > Mathf.PI * 2f) phase -= Mathf.PI * 2f;

        float sign = flipDirection ? -1f : 1f;
        ApplyFlap(leftWingBones, leftBaseRotations, leftFlapAxes, sign);
        ApplyFlap(rightWingBones, rightBaseRotations, rightFlapAxes, -sign); // 右は左右対称なので逆回し
    }

    private void ApplyFlap(Transform[] bones, Quaternion[] baseRotations, Vector3[] axes, float sign)
    {
        if (bones == null || baseRotations == null || bones.Length == 0) return;

        for (int i = 0; i < bones.Length; i++)
        {
            Transform bone = bones[i];
            if (bone == null || i >= baseRotations.Length) continue;

            // 先端ほど遅れて、大きく振れる（しなり）
            float delay = segmentDelay * Mathf.PI * 2f * i;
            float tipRatio = bones.Length > 1 ? (float)i / (bones.Length - 1) : 0f;
            float scale = Mathf.Lerp(1f, tipFlapMultiplier, tipRatio);

            float angle = (spreadAngle + maxFlapAngle * Mathf.Sin(phase - delay)) * scale * currentBlend * sign;
            bone.localRotation = baseRotations[i] * Quaternion.AngleAxis(angle, axes[i]);
        }
    }

    /// <summary>地面から浮いているか</summary>
    private bool IsAirborne()
    {
        // レース開始前（カウントダウン中）はコントローラーが無効化されていて
        // isGrounded が更新されないので、羽ばたかせずに翼を閉じておく
        if (!IsControlActive()) return false;

        if (playerController != null) return !playerController.isGrounded;
        if (npcController != null) return !npcController.isGrounded;

        // コントローラーが無い場合は自前で地面を探す
        Vector3 origin = body != null ? body.position : transform.position;
        if (groundLayer.value != 0)
        {
            return !Physics.Raycast(origin, Vector3.down, groundCheckDistance, groundLayer);
        }
        return !Physics.Raycast(origin, Vector3.down, groundCheckDistance);
    }

    /// <summary>ペンギンが操作可能な状態か（＝レースが始まっているか）</summary>
    private bool IsControlActive()
    {
        if (playerController != null) return playerController.enabled;
        if (npcController != null) return npcController.enabled;
        return true;
    }
}
