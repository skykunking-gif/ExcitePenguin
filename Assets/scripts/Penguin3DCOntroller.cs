using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class Penguin3DController : MonoBehaviour
{
    private Rigidbody rb;

    [Header("2人プレイ用のキー設定")]
    public KeyCode upKey = KeyCode.W;      // 前進
    public KeyCode boostKey = KeyCode.S;
    public KeyCode leftKey = KeyCode.A;
    public KeyCode rightKey = KeyCode.D;
    public KeyCode forwardFlipKey = KeyCode.R; // 前回り（空中制御）
    public KeyCode backFlipKey = KeyCode.F;    // 後ろ回り（空中制御）

    [Header("横移動（レーンチェンジ）設定")]
    public float laneDistance = 2.0f;
    public float laneChangeSpeed = 10f;
    private int currentLane = 1;

    [Header("前進・スピード設定")]
    public float normalSpeed = 10f;
    public float turboSpeed = 18f;
    private float currentMaxSpeed;

    [Header("スタミナ（体温）設定")]
    public bool isBoosting = false; // ブーストキーを押している間 true（演出スクリプト用）
    public float heat = 0f;
    public float maxHeat = 100f;
    public bool isOverheated = false;
    public Slider heatSlider;

    [Header("空中制御（フリップ）設定")]
    public float flipSpeed = 150f;
    public bool isGrounded;
    public LayerMask groundLayer;

    [Header("転倒（ズサーッ）ギミック設定")]
    public bool isTumbled = false; // 転倒中か
    public int tumbleCount = 0;    // 転んだ回数（リザルト表示用）
    public float tumbleDuration = 1.5f; // 転んで滑る時間（秒）
    private float tumbleTimer = 0f;
    private Quaternion targetTumbleRotation; // 転倒時の寝そべる角度

    public bool isGoal = false; // 🏁 追加：ゴールしたかどうかのフラグ

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        if (heatSlider != null)
        {
            heatSlider.minValue = 0f;
            heatSlider.maxValue = maxHeat;
        }
    }

    void Update()
    {
        // 転倒中・オーバーヒート中は下の early return でここが false のままになる
        isBoosting = false;

        RaycastHit hit;
        isGrounded = Physics.Raycast(transform.position, Vector3.down, out hit, 0.9f, groundLayer);

        bool onGrass = false;
        if (isGrounded && hit.collider.CompareTag("Grass"))
        {
            onGrass = true;
        }

        // ★ スライダー更新を早期returnより前に移動
        UpdateHeatSlider();

        if (isTumbled)
        {
            HandleTumble();
            return;
        }

        if (isOverheated)
        {
            HandleOverheat();
            return;
        }

        HandleInputNew();
        if (onGrass)
        {
            currentMaxSpeed *= 0.5f;
        }
    }

    void UpdateHeatSlider()
    {
        if (heatSlider != null)
        {
            // ゲージは「残り（スタミナ）」を表示する。
            // 最初は満タンで、ブーストで heat が上がるほど減っていく。
            heatSlider.value = maxHeat - heat;
        }
    }

    void FixedUpdate()
    {
        // 🏁 追加：ゴールしていたら、速度をゼロにしてこれ以降の移動処理をスキップする
        if (isGoal)
        {
            rb.linearVelocity = Vector3.zero; // velocity の場合は rb.velocity = Vector3.zero;
            return;
        }

        MovePenguin();
    }

    void HandleInputNew()
    {
        var keyboard = Keyboard.current;
        if (keyboard == null) return;

        if (isGrounded)
        {
            // 地面にいる時は勝手に転ばないように固定
            rb.constraints = RigidbodyConstraints.FreezeRotation;

            // 角度を真っ直ぐに戻す
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.identity, 10f * Time.deltaTime);

            // レーンチェンジ
            if (Input.GetKeyDown(leftKey))
            {
                if (currentLane > 0) currentLane--;
            }
            if (Input.GetKeyDown(rightKey))
            {
                if (currentLane < 3) currentLane++;
            }
        }
        else
        {
            // 空中は前後のピッチ回転のみ許可する
            rb.constraints = RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;

            // r で前傾
            if (Input.GetKeyDown(forwardFlipKey))
            {
                transform.Rotate(Vector3.right, flipSpeed * Time.deltaTime);
            }
            // f で後傾
            if (Input.GetKeyDown(backFlipKey))
            {
                transform.Rotate(Vector3.left, flipSpeed * Time.deltaTime);
            }
        }

        // スピード入力
        if (Input.GetKey(upKey))
        {
            currentMaxSpeed = normalSpeed;
            heat = Mathf.Max(0, heat - Time.deltaTime * 10f);
        }
        else if (Input.GetKey(boostKey))
        {
            currentMaxSpeed = turboSpeed;
            heat += Time.deltaTime * 20f;
            isBoosting = true;

            if (heat >= maxHeat) isOverheated = true;
        }
        else
        {
            currentMaxSpeed = 0f;
            heat = Mathf.Max(0, heat - Time.deltaTime * 15f);
        }
    }

    void MovePenguin()
    {
        Vector3 currentVelocity = rb.linearVelocity; // 2026年仕様。エラーなら velocity に

        if (isTumbled)
        {
            // 【改善】転倒中：キー入力は無視、徐々に摩擦で減速しながら滑る（ズサーッ）
            currentVelocity.z = Mathf.MoveTowards(currentVelocity.z, 0, 8f * Time.fixedDeltaTime);
            rb.linearVelocity = currentVelocity;
            return;
        }

        if (isOverheated)
        {
            currentVelocity.z = Mathf.MoveTowards(currentVelocity.z, 0, 15f * Time.fixedDeltaTime);
            rb.linearVelocity = currentVelocity;
            return;
        }

        // ---　前進（Z軸）の速度計算 ---
        if (isGrounded)
        {
            // 地面にいるときは、キー入力（目標速度）に合わせて加減速
            currentVelocity.z = Mathf.MoveTowards(currentVelocity.z, currentMaxSpeed, 30f * Time.fixedDeltaTime);
        }
        else
        {
            // 空中にいるときは、Jキーを離しても急停止せず、空中の風の抵抗（少しだけ自然減速）で進む！
            currentVelocity.z = Mathf.MoveTowards(currentVelocity.z, 0, 1.5f * Time.fixedDeltaTime);
        }

        rb.linearVelocity = currentVelocity;

        // 横移動（地面にいる時だけ）
        if (isGrounded)
        {
            float targetX = (currentLane - 1.5f) * laneDistance;
            float newX = Mathf.MoveTowards(transform.position.x, targetX, laneChangeSpeed * Time.fixedDeltaTime);
            rb.MovePosition(new Vector3(newX, transform.position.y, transform.position.z));
        }
    }

    // 着地時の衝突判定
    private void OnCollisionEnter(Collision collision)
    {
        // 転倒中、または空中にいない（最初から接地している）なら判定しない
        if (isTumbled) return;

        // 地面レイヤーのオブジェクトにぶつかった時だけチェック
        if (((1 << collision.gameObject.layer) & groundLayer) != 0)
        {
            // ペンギンの前方向（transform.forward）がどれくらい傾いているか調べる
            // 垂直（真っ直ぐ）から前後に35度以上傾いて着地したら「転倒」
            float landAngle = Vector3.Angle(transform.forward, Vector3.forward);

            // もし坂道の傾きに対応させる場合は、世界の真上(Vector3.up)とペンギンの上方向の角度を測る
            float angleFromUp = Vector3.Angle(transform.up, Vector3.up);

            if (angleFromUp > 35f)
            {
                TriggerTumble();
            }
        }
    }

    // 転倒スタート
    void TriggerTumble()
    {
        isTumbled = true;
        tumbleCount++;
        tumbleTimer = tumbleDuration;

        // 物理の回転固定を完全に解除して、お腹側にペタンと倒す
        rb.constraints = RigidbodyConstraints.None;

        // 前方に90度パタンと倒れた角度（お腹スライディング）をターゲットにする
        targetTumbleRotation = Quaternion.Euler(90, 0, 0);
    }

    // 転倒中の処理（ズサーッの絵面を作る）
    void HandleTumble()
    {
        tumbleTimer -= Time.deltaTime;

        // スムーズに横倒し（お腹這い）のポーズに回転させる
        transform.rotation = Quaternion.Lerp(transform.rotation, targetTumbleRotation, 15f * Time.deltaTime);

        // 時間が経ったら復活
        if (tumbleTimer <= 0f)
        {
            isTumbled = false;
            // 角度を真っ直ぐに戻す準備
            rb.constraints = RigidbodyConstraints.FreezeRotation;
            transform.rotation = Quaternion.identity;

            // 復活時は安全のために少しだけ上方に跳ね上げる、または少し無敵時間を作ると良いです
        }
    }

    void HandleOverheat()
    {
        heat -= Time.deltaTime * 25f;
        if (heat <= 0)
        {
            heat = 0;
            isOverheated = false;
        }
    }

    // 🏁 追加：トリガー（センサー）に触れたときの判定
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Goal"))
        {
            isGoal = true;
            rb.constraints = RigidbodyConstraints.FreezeAll; // 物理挙動も完全にカチコチに止める
            Debug.Log("🎉 ゴールした！");
        }
    }
}
