using UnityEngine;
using UnityEngine.InputSystem;

public class Penguin3DController : MonoBehaviour
{
    private Rigidbody rb;

    [Header("横移動（レーンチェンジ）設定")]
    public float laneDistance = 2.0f; 
    public float laneChangeSpeed = 10f; 
    private int currentLane = 1; 

    [Header("前進・スピード設定")]
    public float normalSpeed = 10f;
    public float turboSpeed = 18f;
    private float currentMaxSpeed;

    [Header("スタミナ（体温）設定")]
    public float heat = 0f;
    public float maxHeat = 100f;
    public bool isOverheated = false;

    [Header("空中制御（フリップ）設定")]
    [Tooltip("空中での回転スピード")]
    public float flipSpeed = 150f; 
    [Tooltip("今地面に触れているかどうか")]
    public bool isGrounded; 
    [Tooltip("地面と判定するレイヤー")]
    public LayerMask groundLayer; 

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        // 地面判定：ペンギンの中心から下方向へ、少しだけ(0.6m)見えない光線（Ray）を飛ばす
        // カプセルやキューブの大きさに合わせて 0.6f の値は調整してください
        isGrounded = Physics.Raycast(transform.position, Vector3.down, 0.8f, groundLayer);

        if (isOverheated)
        {
            HandleOverheat();
            return;
        }

        HandleInputNew();
    }

    void FixedUpdate()
    {
        MovePenguin();
    }

    void HandleInputNew()
    {
        var keyboard = Keyboard.current;
        if (keyboard == null) return;

        // --- 1. 地面にいる時の操作 ---
        if (isGrounded)
        {
            // レーンチェンジ
            if (keyboard.aKey.wasPressedThisFrame || keyboard.leftArrowKey.wasPressedThisFrame)
            {
                if (currentLane > 0) currentLane--;
            }
            if (keyboard.dKey.wasPressedThisFrame || keyboard.rightArrowKey.wasPressedThisFrame)
            {
                if (currentLane < 3) currentLane++;
            }

            // 地面にいる時は、勝手に転ばないように回転をリセット＆固定する
            rb.constraints = RigidbodyConstraints.FreezeRotation;
            // 角度を真っ直ぐ（あるいは道路の傾き）に戻す補助を入れると綺麗ですが、まずは垂直に固定
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.identity, 10f * Time.deltaTime);
        }
        // --- 2. 空中にいる時の操作（フリップ） ---
        else
        {
            // 空中は物理で倒れるように、回転の固定を解除する（ただしY軸のヨコ回転とZ軸のローリングは防ぐ）
            rb.constraints = RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;

            // Wキー または 上矢印で「前傾（おじぎ）」
            if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed)
            {
                transform.Rotate(Vector3.right, flipSpeed * Time.deltaTime);
            }
            // Sキー または 下矢印で「後傾（ウィリー状態）」
            if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed)
            {
                transform.Rotate(Vector3.left, flipSpeed * Time.deltaTime);
            }
        }

        // --- 3. スピード入力（J/K） ---
        if (keyboard.jKey.isPressed)
        {
            currentMaxSpeed = normalSpeed;
            heat = Mathf.Max(0, heat - Time.deltaTime * 10f);
        }
        else if (keyboard.kKey.isPressed)
        {
            currentMaxSpeed = turboSpeed;
            heat += Time.deltaTime * 20f;

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

        if (isOverheated)
        {
            currentVelocity.z = 0;
        }
        else
        {
            // 空中ではアクセルを踏んでも劇的には加速しないように、前進はスムーズ補間
            currentVelocity.z = Mathf.MoveTowards(currentVelocity.z, currentMaxSpeed, 30f * Time.fixedDeltaTime);
        }

        rb.linearVelocity = currentVelocity;

        // 横移動（レーンチェンジ）は地面にいる時だけ位置を固定する
        if (isGrounded)
        {
            float targetX = (currentLane - 1.5f) * laneDistance;
            float newX = Mathf.MoveTowards(transform.position.x, targetX, laneChangeSpeed * Time.fixedDeltaTime);
            rb.MovePosition(new Vector3(newX, transform.position.y, transform.position.z));
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
}
