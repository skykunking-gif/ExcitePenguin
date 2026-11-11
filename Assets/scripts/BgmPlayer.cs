using UnityEngine;

/// <summary>
/// ゲーム起動時から BGM をループ再生する。シーンをまたいでも途切れないよう 1 つだけ残す。
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class BgmPlayer : MonoBehaviour
{
    [SerializeField] AudioClip bgm;
    [SerializeField, Range(0f, 1f)] float volume = 0.5f;

    static BgmPlayer instance;

    void Awake()
    {
        // タイトルに戻ってきたときに 2 つ目が生まれないようにする
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);

        var source = GetComponent<AudioSource>();
        source.clip = bgm;
        source.volume = volume;
        source.loop = true;
        source.playOnAwake = false;
        source.spatialBlend = 0f;
        source.Play();
    }
}
