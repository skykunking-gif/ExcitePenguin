using UnityEngine;
using UnityEngine.UI; // UI（スタミナゲージ）用

public class Penguin3DController : MonoBehaviour
{
    private Rigidbody rb;

    [Header("横移動（レーンチェンジ）")]
    public float laneDistance = 2.0f; // レーン間の距離
    public float laneChangeSpeed = 10f; // レーン移動のスムーズさ
    private int currentLane = 1; // 最初は左から2番目のレーン（インデックス0〜3）

    [Header("前進・スピード設定")]
    public float normalSpeed = 10f;
    public float turboSpeed = 18f;
    private float currentMaxSpeed;

    [Header("スタミナ（体温）設定")]
    public float heat = 0f;
    public float maxHeat = 100f;
    public bool isOverheated = false;
    public Slider heatSlider; // インスペクターでUIのスライダーを紐付ける

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        // スタミナ（体温）UIの更新
        if (heatSlider != null) heatSlider.value = heat;

        // オーバーヒート時の処理
        if (isOverheated)
        {
            HandleOverheat();
            return;
        }

        // 2. 入力処理（レーン移動とダッシュ）
        HandleLaneInput();
        HandleSpeedInput();
    }

    void FixedUpdate()
    {
        if (isOverheated) return;

        // 移動（前進とスムーズなレーンチェンジ）
        // 前進
        Vector3 targetVelocity = rb.linearVelocity; // 2026以降。エラーなら velocity
        targetVelocity.z = currentMaxSpeed; // Z軸が前進方向と仮定

        // 目標の横位置（X座標）を計算
        // （例：currentLane 0なら X=-3, 1なら X=-1, 2なら X=1, 3なら X=3）
        float targetX = (currentLane - 1.5f) * laneDistance; // 調整が必要
        
        // X座標だけ目標にスムーズに近づける
        float currentX = rb.position.x;
        float newX = Mathf.MoveTowards(currentX, targetX, laneChangeSpeed * Time.fixedDeltaTime);
        
        // Rigidbodyに新しい位置と速度を適用
        rb.MovePosition(new Vector3(newX, rb.position.y, rb.position.z));
        rb.linearVelocity = targetVelocity; // エラーなら velocity
    }

    // --- 各入力処理の関数 ---

    void HandleLaneInput()
    {
        // A / Dキーまたは矢印キーでレーン移動
        if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
        {
            if (currentLane > 0) currentLane--;
        }
        if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
        {
            if (currentLane < 3) currentLane++;
        }
    }

    void HandleSpeedInput()
    {
        // Aボタン相当（Jキー = 通常ダッシュ）
        if (Input.GetKey(KeyCode.J))
        {
            currentMaxSpeed = normalSpeed;
            // 徐々に熱が下がる
            heat = Mathf.Max(0, heat - Time.deltaTime * 10f);
        }
        // Bボタン相当（Kキー = ターボダッシュ）
        else if (Input.GetKey(KeyCode.K))
        {
            currentMaxSpeed = turboSpeed;
            // 熱が上がる
            heat += Time.deltaTime * 20f;

            if (heat >= maxHeat)
            {
                isOverheated = true;
            }
        }
        else
        {
            // 何も押していない時は自然減速
            currentMaxSpeed = 0;
            heat = Mathf.Max(0, heat - Time.deltaTime * 15f);
        }
    }

    void HandleOverheat()
    {
        // 強制停止（Z軸の速度を0に）
        Vector3 overHeatVel = rb.linearVelocity; // 2026以降。エラーなら velocity
        overHeatVel.z = 0;
        rb.linearVelocity = overHeatVel; // エラーなら velocity

        heat -= Time.deltaTime * 25f; // 冷却スピードアップ

        if (heat <= 0)
        {
            heat = 0;
            isOverheated = false;
        }
    }
}
