using Unity.Hierarchy.Editor;
using Unity.VisualScripting;
using UnityEngine;

public class StageGameInitializer : MonoBehaviour
{
    public GameObject npcPenguin;
    public GameObject player2Penguin;

    [Header("画面分割用のカメラ（本体）")]
    public Camera camera1P;
    public Camera camera2P;

    [Header("Chinemachineのバーチャルカメラ")]
    public GameObject vcam1P;
    public GameObject vcam2P;

    void Start()
    {
        //タイトル画面で保存されたモードを読み込む
        int mode = PlayerPrefs.GetInt("GameMode",0);

        if (mode == 0)
        {
            //NPCモードなら
            if(npcPenguin != null) npcPenguin.SetActive(true);
            if(player2Penguin != null) player2Penguin.SetActive(false);

            if(camera2P != null) camera2P.gameObject.SetActive(false);
            if(camera1P != null) camera1P.rect = new Rect(0f, 0f, 1f, 1f);

            // 2P用のCinemachineもオフにする
            if(vcam2P != null) vcam2P.SetActive(false);
        }
        else
        {
            //2pモード
            if(npcPenguin != null) npcPenguin.SetActive(false);
            if(player2Penguin != null) player2Penguin.SetActive(true);

            if(camera2P != null) camera2P.gameObject.SetActive(true);
            if(camera1P != null) camera1P.rect = new Rect(0f, 0.5f, 1f, 0.5f);

            // 2P用のCinemachineもオフにする
            if(vcam2P != null) vcam2P.SetActive(true);
        }
    }
}
