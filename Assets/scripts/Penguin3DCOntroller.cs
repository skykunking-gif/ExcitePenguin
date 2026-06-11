using UnityEngine;
// ⬇︎ 新しいInput Systemを使うためにこれを追加
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

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotation;
    }

    void Update()
    {
        if (isOverheated)
        {
            HandleOverheat();
            return;
        }

        // 新しいInput System用の入力処理
        HandleInputNew();
    }

    void FixedUpdate()
    {
        MovePenguin();
    }

    // 新しいInput Systemに対応した入力処理関数
    void HandleInputNew()
    {
        // キーボードの現在の状態を取得
        var keyboard = Keyboard.current;
        if (keyboard == null) return; // キーボードが接続されていない場合は何もしない

        // 1. レーンチェンジ（押した瞬間：wasPressedThisFrame）
        if (keyboard.aKey.wasPressedThisFrame || keyboard.leftArrowKey.wasPressedThisFrame)
        {
            if (currentLane > 0) currentLane--;
        }
        if (keyboard.dKey.wasPressedThisFrame || keyboard.rightArrowKey.wasPressedThisFrame)
        {
            if (currentLane < 3) currentLane++;
        }

        // 2. スピード（押しっぱなし：isPressed）
        // Jキー = 通常アクセル
        if (keyboard.jKey.isPressed)
        {
            currentMaxSpeed = normalSpeed;
            heat = Mathf.Max(0, heat - Time.deltaTime * 10f);
        }
        // Kキー = ターボアクセル
        else if (keyboard.kKey.isPressed)
        {
            currentMaxSpeed = turboSpeed;
            heat += Time.deltaTime * 20f;

            if (heat >= maxHeat)
            {
                isOverheated = true;
            }
        }
        // 何も押していないとき
        else
        {
            currentMaxSpeed = 0f;
            heat = Mathf.Max(0, heat - Time.deltaTime * 15f);
        }
    }

    void MovePenguin()
    {
        Vector3 currentVelocity = rb.linearVelocity; // エラーなら velocity に変更
        
        if (isOverheated)
        {
            currentVelocity.z = 0;
        }
        else
        {
            currentVelocity.z = Mathf.MoveTowards(currentVelocity.z, currentMaxSpeed, 30f * Time.fixedDeltaTime);
        }

        rb.linearVelocity = currentVelocity;

        float targetX = (currentLane - 1.5f) * laneDistance;
        float newX = Mathf.MoveTowards(transform.position.x, targetX, laneChangeSpeed * Time.fixedDeltaTime);

        rb.MovePosition(new Vector3(newX, transform.position.y, transform.position.z));
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
