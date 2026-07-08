using UnityEngine;

public class CourseGenerator : MonoBehaviour
{
    [Header("コースパーツのプレハブ登録")]
    public GameObject straightPrefab;
    public GameObject jumpPrefab;

    [Header("上に敷く障害物プレハブ")]
    public GameObject grassPrefab;

    [Header("コースの設定")]
    public float pieceLength = 10f;
    public int totalPieces = 30;

    private float laneDistance = 2.0f;
    private float currentZ = 0f;

    // 連続回数を数えるためのカウンター
    private int continuousSafeStraightCount = 0; // 「芝生なしストレート」の連続数
    private int continuousJumpCount = 0;         // 「ジャンプ台」の連続数

    void Start()
    {
        GenerateRandomCourse();
    }

    void GenerateRandomCourse()
    {
        // 最初の1枚目（スタート地点）にストレートを置く
        SpawnBasePiece(straightPrefab);

        for (int i = 1; i < totalPieces - 1; i++)
        {
            GameObject chosenBase = straightPrefab;
            bool isJump = false;

            // 1. ベース道路の抽選（ジャンプの連続防止） ---
            float jumpChance = 0.3f; // 基本のジャンプ確率 (30%)

            // もしジャンプ台がすでに3連続していたら、今回は強制的にストレートにする
            if (continuousJumpCount >= 3)
            {
                jumpChance = 0f; 
            }

            if (Random.value < jumpChance)
            {
                chosenBase = jumpPrefab;
                isJump = true;
                continuousJumpCount++;
                continuousSafeStraightCount = 0; // ジャンプ台が置かれたので安全直線カウントはリセット
            }
            else
            {
                chosenBase = straightPrefab;
                continuousJumpCount = 0; // ストレートが置かれたのでジャンプカウントはリセット
            }

            // ベース道路を生成
            Transform spawnedBase = SpawnBasePiece(chosenBase);
            
            // 2. 芝生の抽選（何もない直線の連続防止） ---
            if (!isJump && grassPrefab != null)
            {
                float grassChance = 0.3f; // 基本の芝生確率 (30%)

                // もし「芝生のないただの直線」がすでに3連続していたら、
                // 4マス目は【100%（強制）】で芝生を敷く
                if (continuousSafeStraightCount >= 3)
                {
                    grassChance = 1.0f; 
                }

                if (Random.value < grassChance)
                {
                    SpawnGrassOnTop(spawnedBase);
                    continuousSafeStraightCount = 0; // 芝生が置かれたのでリセット
                }
                else
                {
                    // 芝生が置かれなかった＝ただの安全な直線
                    continuousSafeStraightCount++;
                }
            }
            else if (isJump)
            {
                // ジャンプ台のマスは「芝生がない直線」としては数えない
                // （ジャンプというイベントが既に起きているため）
                continuousSafeStraightCount = 0;
            }
        }
        
        // 🏁 最後のゴール地点を生成し、そこにゴール判定を仕込む
        Transform goalBase = SpawnBasePiece(straightPrefab);
        if (goalBase != null)
        {
            // オブジェクトに「Goal」タグをつける
            goalBase.gameObject.tag = "Goal";

            // プログラムからその場に「通り抜け可能な透明な壁（Trigger）」を出現させる
            BoxCollider goalTrigger = goalBase.gameObject.AddComponent<BoxCollider>();
            goalTrigger.isTrigger = true; // 通り抜け可能にする
            
            // センサーの大きさをコース全体（4レーン分）をしっかり覆うように広げる
            goalTrigger.size = new Vector3(12f, 5f, 1f); 
            goalTrigger.center = new Vector3(2.0f, 2.5f, 0f); // 芝生のX座標（0, 2, 4）に合わせて中央を調整
        }
    }

    Transform SpawnBasePiece(GameObject prefab)
    {
        if (prefab == null) return null;

        Vector3 spawnPosition = new Vector3(0f, 0f, currentZ);
        GameObject spawnedPiece = Instantiate(prefab, spawnPosition, Quaternion.identity);
        spawnedPiece.transform.SetParent(this.transform);

        currentZ += pieceLength;
        return spawnedPiece.transform;
    }

    void SpawnGrassOnTop(Transform baseTransform)
    {
        float grassX = 0f;
        int randomPattern = Random.Range(0, 3);

        switch (randomPattern)
        {
            case 0: grassX = 0.0f; break; // 左側2レーン
            case 1: grassX = 2.0f;  break; // 真ん中2レーン
            case 2: grassX = 4.0f;  break; // 右側2レーン
        }

        Vector3 grassPosition = new Vector3(grassX, 0.05f, baseTransform.position.z);
        GameObject grass = Instantiate(grassPrefab, grassPosition, Quaternion.identity);
        grass.transform.SetParent(baseTransform);
    }
}
