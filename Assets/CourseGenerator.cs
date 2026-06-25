using UnityEngine;

public class CourseGenerator : MonoBehaviour
{
    [Header("コースパーツのプレハブ登録")]
    public GameObject straightPrefab;
    public GameObject jumpPrefab;

    [Header("上に敷く障害物プレハブ")]
    public GameObject grassPrefab;

    [Header("コースの設定")]
    public float pieceLengh = 10f;
    public int totalPieces = 30;

    private float laneDistance = 2.0f;

    private float currentZ = 0f;

    void Start()
    {
        GenearteRandomCourse();
    }

    void GenearteRandomCourse()
    {
        //最初の1枚目（スタート地点）にストレートを置く
        SpawnPiece(straightPrefab);

        for (int i = 1;i < totalPieces-1; i++)
        {
            GameObject chosenBase = straightPrefab;

            if(Random.value < 0.3f)
            {
                chosenBase = jumpPrefab;
            }
            else
            {
                chosenBase = straightPrefab;
            }
            TransformBlock spawnedBase = SpawnBasePiece(chosenBase);
            
            if(Random.value < 0.3f && grassPrefab != null)
            {
                SpawnGrassOnTop(spawnedBase);
            }
        }
        SpawnBasePiece(straightPrefab);
    }

    TransformBlock SpawnBasePiece(GameObject prefab)
    {
        if (prefab == null) return null;

        Vector3 spawnPosition = new Vecotor3(0f, 0f, currentZ);
        GameObject spawnedPiece = Instantiate(prefab, spawnPosition, Quaternion.identity);
        spawnedPiece.transform.Setparent(this.transform);

        currentZ += pieceLengh;
        return spawnedPiece.transform;
    }

    // 道路の上に芝生を生成する関数（3パターンの配置に対応）
    void SpawnGrassOnTop(Transform baseTransform)
    {
        float grassX = 0f;

        // 0, 1, 2 のいずれかをランダムに選択
        int randomPattern = Random.Range(0, 3);

        switch (randomPattern)
        {
            case 0:
                grassX = -2.0f; // パターン0：左側2レーン
                break;
            case 1:
                grassX = 0.0f;  // パターン1：真ん中2レーン 【新設】
                break;
            case 2:
                grassX = 2.0f;  // パターン2：右側2レーン
                break;
        }

        // ベース道路の真上の位置を計算（Yを少しだけ上げて道路の上に乗せる）
        Vector3 grassPosition = new Vector3(grassX, 0.05f, baseTransform.position.z);

        // 芝生を生成
        GameObject grass = Instantiate(grass2LanePrefab, grassPosition, Quaternion.identity);
        
        // 道路の子要素にしておく（片付けや管理が楽になります）
        grass.transform.SetParent(baseTransform);
    }
}
