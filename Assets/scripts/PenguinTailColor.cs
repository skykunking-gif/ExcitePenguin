using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ペンギンのしっぽだけを別の色で塗る（2P・NPCの見分け用）。
///
/// このモデルは全身が1マテリアル・1枚のパレットテクスチャなので、色だけでは部位を分けられない。
/// そこで「しっぽのボーンに追従している三角形」をメッシュのコピー上で別サブメッシュに切り出し、
/// そこにだけ単色マテリアルを割り当てている。メッシュもマテリアルも実行時のコピーなので、
/// 他のペンギンや元アセットには影響しない。
///
/// ※ モデル（FBX）の Read/Write Enabled をオンにしておく必要があります。
/// </summary>
[DisallowMultipleComponent]
public class PenguinTailColor : MonoBehaviour
{
    [Header("設定")]
    [Tooltip("このペンギンのしっぽを塗るか（1Pはオフ）")]
    public bool enableTailColor = false;

    [Tooltip("しっぽの色")]
    public string tailColorHex = "#4298F5";

    [Tooltip("しっぽのボーン名（PenguinTailWag と同じ）")]
    public string[] tailBoneNames = { "DEF-spine.001", "DEF-spine.002" };

    [Tooltip("この重み以上でしっぽのボーンに追従している頂点を「しっぽ」とみなす")]
    [Range(0f, 1f)] public float minWeight = 0.5f;

    private bool applied;

    void Start()
    {
        if (!enableTailColor) return;
        Apply();
    }

    /// <summary>しっぽ部分を切り出して単色マテリアルを割り当てる</summary>
    public void Apply()
    {
        if (applied) return; // 二重に切り出さない

        SkinnedMeshRenderer renderer = GetComponentInChildren<SkinnedMeshRenderer>(true);
        if (renderer == null || renderer.sharedMesh == null) return;

        Mesh source = renderer.sharedMesh;
        if (!source.isReadable)
        {
            Debug.LogWarning($"[PenguinTailColor] {name}: モデルの Read/Write Enabled がオフのため、しっぽを塗り分けられません。");
            return;
        }

        HashSet<int> tailBones = FindTailBoneIndices(renderer.bones);
        if (tailBones.Count == 0)
        {
            Debug.LogWarning($"[PenguinTailColor] {name}: しっぽのボーンが見つかりませんでした。");
            return;
        }

        bool[] isTailVertex = MarkTailVertices(source.boneWeights, tailBones);

        Mesh mesh = Instantiate(source);
        mesh.name = source.name + "_tailsplit";

        // 各サブメッシュの三角形を「しっぽ」と「それ以外」に振り分ける
        int originalSubMeshCount = mesh.subMeshCount;
        var bodyTriangles = new List<int[]>(originalSubMeshCount);
        var tailTriangles = new List<int>();

        for (int sub = 0; sub < originalSubMeshCount; sub++)
        {
            int[] triangles = mesh.GetTriangles(sub);
            var body = new List<int>(triangles.Length);

            for (int i = 0; i + 2 < triangles.Length; i += 3)
            {
                int a = triangles[i], b = triangles[i + 1], c = triangles[i + 2];
                int tailCount = (isTailVertex[a] ? 1 : 0) + (isTailVertex[b] ? 1 : 0) + (isTailVertex[c] ? 1 : 0);

                // 3頂点のうち2つ以上がしっぽなら、その三角形はしっぽ側
                if (tailCount >= 2)
                {
                    tailTriangles.Add(a); tailTriangles.Add(b); tailTriangles.Add(c);
                }
                else
                {
                    body.Add(a); body.Add(b); body.Add(c);
                }
            }
            bodyTriangles.Add(body.ToArray());
        }

        if (tailTriangles.Count == 0)
        {
            Debug.LogWarning($"[PenguinTailColor] {name}: しっぽの三角形が見つかりませんでした（Min Weight を下げてみてください）。");
            return;
        }

        mesh.subMeshCount = originalSubMeshCount + 1;
        for (int sub = 0; sub < originalSubMeshCount; sub++)
        {
            mesh.SetTriangles(bodyTriangles[sub], sub);
        }
        mesh.SetTriangles(tailTriangles, originalSubMeshCount);
        renderer.sharedMesh = mesh;

        // しっぽ用のマテリアルを足す（テクスチャを外して単色にする）
        var materials = new List<Material>(renderer.materials); // materials はインスタンス
        materials.Add(CreateTailMaterial(materials[0]));
        renderer.materials = materials.ToArray();
        applied = true;
    }

    private HashSet<int> FindTailBoneIndices(Transform[] bones)
    {
        var indices = new HashSet<int>();
        if (bones == null) return indices;

        for (int i = 0; i < bones.Length; i++)
        {
            if (bones[i] == null) continue;

            foreach (string boneName in tailBoneNames)
            {
                if (string.IsNullOrEmpty(boneName)) continue;
                if (string.Equals(bones[i].name, boneName, System.StringComparison.OrdinalIgnoreCase))
                {
                    indices.Add(i);
                    break;
                }
            }
        }
        return indices;
    }

    private bool[] MarkTailVertices(BoneWeight[] weights, HashSet<int> tailBones)
    {
        var isTail = new bool[weights.Length];
        for (int i = 0; i < weights.Length; i++)
        {
            BoneWeight w = weights[i];
            float total = 0f;
            if (tailBones.Contains(w.boneIndex0)) total += w.weight0;
            if (tailBones.Contains(w.boneIndex1)) total += w.weight1;
            if (tailBones.Contains(w.boneIndex2)) total += w.weight2;
            if (tailBones.Contains(w.boneIndex3)) total += w.weight3;

            isTail[i] = total >= minWeight;
        }
        return isTail;
    }

    private Material CreateTailMaterial(Material baseMaterial)
    {
        Color color;
        if (!ColorUtility.TryParseHtmlString(tailColorHex, out color)) color = Color.blue;

        Material tailMaterial = new Material(baseMaterial);
        tailMaterial.name = baseMaterial.name + "_tail";

        // パレットテクスチャのままだと色が乗らないので外して単色にする
        if (tailMaterial.HasProperty("_BaseMap")) tailMaterial.SetTexture("_BaseMap", null);
        tailMaterial.mainTexture = null;

        if (tailMaterial.HasProperty("_BaseColor")) tailMaterial.SetColor("_BaseColor", color);
        tailMaterial.color = color;

        return tailMaterial;
    }
}
