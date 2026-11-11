using UnityEngine;

/// <summary>
/// オプションで選んだ色を、ペンギンのオレンジ色の部分（くちばし・耳あたりの模様）に反映する。
/// このモデルは1枚のパレットテクスチャで色分けされているので、
/// 「オレンジ系の色相のピクセルだけ」を選んだ色の色相に置き換えたテクスチャを作り直している。
/// （明るさ・彩度のグラデーションはそのまま残るので、塗り替えても立体感が消えない）
///
/// ※ パレットテクスチャの Read/Write Enabled をオンにしておく必要があります。
/// </summary>
[DisallowMultipleComponent]
public class PenguinColorOption : MonoBehaviour
{
    /// <summary>PlayerPrefs のキー。0:そのまま 1:#A7F542 2:#4298F5</summary>
    public const string PrefsKey = "PenguinColor";

    /// <summary>選べる色。index 0 は「そのままの色」なので塗り替えない</summary>
    public static readonly string[] HexColors = { "", "#A7F542", "#4298F5" };

    [Header("設定")]
    [Tooltip("このペンギンにオプションの色を反映するか（2PやNPCはオフにする）")]
    public bool applyToThisPenguin = true;

    [Tooltip("色を固定したい時に使う。-1ならオプション（PlayerPrefs）の選択に従う。0:そのまま 1:#A7F542 2:#4298F5")]
    public int forceColorIndex = -1;

    [Tooltip("塗り替える色相の範囲（0〜1）。既定はオレンジ〜黄色あたり")]
    [Range(0f, 1f)] public float hueMin = 0f;
    [Range(0f, 1f)] public float hueMax = 0.14f;
    [Tooltip("これより鮮やかなピクセルだけ塗り替える（灰色や白は対象外）")]
    [Range(0f, 1f)] public float minSaturation = 0.3f;

    [Tooltip("テクスチャが入っているマテリアルのプロパティ名（URP Lit は _BaseMap）")]
    public string textureProperty = "_BaseMap";

    void Start()
    {
        // forceColorIndex が設定されていればそれを優先（撮影用シーンなどで色を固定したい時に使う）
        Apply(forceColorIndex >= 0 ? forceColorIndex : GetSelectedIndex());
    }

    /// <summary>いま選ばれている色番号</summary>
    public static int GetSelectedIndex()
    {
        return Mathf.Clamp(PlayerPrefs.GetInt(PrefsKey, 0), 0, HexColors.Length - 1);
    }

    /// <summary>色番号から実際の色を取り出す（index 0 は「そのまま」なので false を返す）</summary>
    public static bool TryGetColor(int index, out Color color)
    {
        color = Color.white;
        if (index <= 0 || index >= HexColors.Length) return false;
        return ColorUtility.TryParseHtmlString(HexColors[index], out color);
    }

    /// <summary>指定の色番号でペンギンを塗り替える</summary>
    public void Apply(int index)
    {
        if (!applyToThisPenguin) return;

        Color target;
        if (!TryGetColor(index, out target)) return; // そのままの色

        foreach (Renderer renderer in GetComponentsInChildren<Renderer>(true))
        {
            Material material = renderer.material; // インスタンス化されるので他のペンギンには影響しない

            Texture2D source = null;
            if (material.HasProperty(textureProperty))
            {
                source = material.GetTexture(textureProperty) as Texture2D;
            }
            if (source == null) source = material.mainTexture as Texture2D;
            if (source == null) continue;

            Texture2D recolored = CreateRecoloredTexture(source, target);
            if (recolored == null) continue;

            if (material.HasProperty(textureProperty)) material.SetTexture(textureProperty, recolored);
            else material.mainTexture = recolored;
        }
    }

    /// <summary>オレンジ系のピクセルだけ色相を差し替えたテクスチャを作る</summary>
    private Texture2D CreateRecoloredTexture(Texture2D source, Color target)
    {
        Color[] pixels;
        try
        {
            pixels = source.GetPixels();
        }
        catch (UnityException)
        {
            Debug.LogWarning($"[PenguinColorOption] {source.name} の Read/Write Enabled がオフのため色を変えられません。");
            return null;
        }

        float targetHue;
        float targetSaturation;
        float targetValue;
        Color.RGBToHSV(target, out targetHue, out targetSaturation, out targetValue);

        for (int i = 0; i < pixels.Length; i++)
        {
            float h, s, v;
            Color.RGBToHSV(pixels[i], out h, out s, out v);

            if (s < minSaturation) continue;          // 白・黒・灰色はそのまま
            if (h < hueMin || h > hueMax) continue;   // オレンジ系以外はそのまま

            // 元の陰影（彩度・明るさ）は残したまま、色相だけ差し替える
            Color replaced = Color.HSVToRGB(targetHue, s, v);
            replaced.a = pixels[i].a;
            pixels[i] = replaced;
        }

        Texture2D result = new Texture2D(source.width, source.height, TextureFormat.RGBA32, true);
        result.name = source.name + "_recolored";
        result.filterMode = source.filterMode;
        result.wrapMode = source.wrapMode;
        result.SetPixels(pixels);
        result.Apply();
        return result;
    }
}
