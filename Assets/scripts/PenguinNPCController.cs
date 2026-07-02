using UnityEngine;

public class PenguinNPCController : MonoBehaviour
{
    private Rigidbody rb;

    [Header("横移動（レーンチェンジ）設定")]
    public float laneDistance = 2.0f; 
    public float laneChangeSpeed = 8f; // プレイヤーより少しのんびり移動
    private int currentLane = 1; 

    [Header("前進・スピード設定")]
    public float normalSpeed = 10f; // プレイヤーよりほんの少し遅く（調整可能）
    private float currentMaxSpeed;

    [Header("空中制御・接地設定")]
    public float flipSpeed = 120f; 
    public bool isGrounded; 
    public LayerMask groundLayer; 

    [Header("NPCの性格・タイマー設定")]
    [Tooltip("何秒ごとにレーンチェンジを検討するか")]
    public float decisionInterval = 3f; 
    private float decisionTimer = 0f;

    [Header("転倒（ズサーッ）ギミック設定")]
    public bool isTumbled = false;
    public float tumbleDuration = 1.7f;
    private float tumbleTimer = 0f;
    private Quaternion targetTumbleRotation;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        // スタート時のレーンをランダム（0〜3）に決める
        currentLane = Random.Range(0, 4);
    }

    void Update()
    {
        RaycastHit hit;
        // 地面判定
        isGrounded = Physics.Raycast(transform.position, Vector3.down, out hit, 1.1f, groundLayer);

        bool onGrass = false;
        if (isGrounded && hit.collider.CompareTag("Grass"))
        {
            onGrass = true;
        }
        if (isTumbled)
        {
            HandleTumble();
            return;
        }

        HandleAIThinking();
        if (onGrass)
        {
            currentMaxSpeed *= 0.5f; 
        }
    }

    void FixedUpdate()
    {
        MoveNPC();
    }

    // --- NPCの自動思考アルゴリズム ---
    void HandleAIThinking()
    {
        // 1. 【地上】自動アクセルとランダムなレーンチェンジ
        if (isGrounded)
        {
            rb.constraints = RigidbodyConstraints.FreezeRotation;
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.identity, 10f * Time.deltaTime);

            // NPCは常に「通常アクセル」を踏み続ける
            currentMaxSpeed = normalSpeed;

            // 一定時間ごとにランダムでレーンを変える
            decisionTimer += Time.deltaTime;
            if (decisionTimer >= decisionInterval)
            {
                decisionTimer = 0f;
                // 30%の確率で隣のレーンに移動する
                if (Random.value < 0.3f)
                {
                    int direction = Random.value > 0.5f ? 1 : -1;
                    currentLane = Mathf.Clamp(currentLane + direction, 0, 3);
                }
            }
        }
        // 2. 【空中】自動着地修正（エキサイトバイクの賢いCPU）
        else
        {
            rb.constraints = RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;

            // 世界の真上（Vector3.up）に対するペンギンの現在の傾きを調べる
            float angle = Vector3.SignedAngle(transform.up, Vector3.up, Vector3.right);

            // 傾きがズレていたら、自動的に真っ直ぐ（0度）になるようにパタパタ戻す
            if (angle > 5f)
            {
                // 前に傾きすぎているので、後ろに戻す
                transform.Rotate(Vector3.left, flipSpeed * Time.deltaTime);
            }
            else if (angle < -5f)
            {
                // 後ろに傾きすぎているので、前に戻す
                transform.Rotate(Vector3.right, flipSpeed * Time.deltaTime);
            }
        }
    }

    void MoveNPC()
    {
        Vector3 currentVelocity = rb.linearVelocity; // エラーなら velocity に

        if (isTumbled)
        {
            currentVelocity.z = Mathf.MoveTowards(currentVelocity.z, 0, 8f * Time.fixedDeltaTime);
            rb.linearVelocity = currentVelocity;
            return;
        }

        // 前進
        if (isGrounded)
        {
            currentVelocity.z = Mathf.MoveTowards(currentVelocity.z, currentMaxSpeed, 25f * Time.fixedDeltaTime);
        }
        else
        {
            // 空中での慣性
            currentVelocity.z = Mathf.MoveTowards(currentVelocity.z, 0, 1.5f * Time.fixedDeltaTime);
        }
        rb.linearVelocity = currentVelocity;

        // 横移動（レーンチェンジ）
        if (isGrounded)
        {
            float targetX = (currentLane - 1.5f) * laneDistance;
            float newX = Mathf.MoveTowards(transform.position.x, targetX, laneChangeSpeed * Time.fixedDeltaTime);
            rb.MovePosition(new Vector3(newX, transform.position.y, transform.position.z));
        }
    }

    // 着地判定（プレイヤーと同じ）
    private void OnCollisionEnter(Collision collision)
    {
        if (isTumbled) return;

        if (((1 << collision.gameObject.layer) & groundLayer) != 0)
        {
            float angleFromUp = Vector3.Angle(transform.up, Vector3.up);

            // CPUも着地角度が35度以上ズレていたらお腹で滑る！
            if (angleFromUp > 35f)
            {
                TriggerTumble();
            }
        }
    }

    void TriggerTumble()
    {
        isTumbled = true;
        tumbleTimer = tumbleDuration;
        rb.constraints = RigidbodyConstraints.None;
        targetTumbleRotation = Quaternion.Euler(90, 0, 0);
    }

    void HandleTumble()
    {
        tumbleTimer -= Time.deltaTime;
        transform.rotation = Quaternion.Lerp(transform.rotation, targetTumbleRotation, 15f * Time.deltaTime);

        if (tumbleTimer <= 0f)
        {
            isTumbled = false;
            rb.constraints = RigidbodyConstraints.FreezeRotation;
            transform.rotation = Quaternion.identity;
        }
    }
}
