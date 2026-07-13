using UnityEngine;

public class StageGameInitializer : MonoBehaviour
{
    public GameObject npcPenguin; 
    public GameObject player2Penguin; 

    [Header("ブーストゲージ")]
    public GameObject canvas1P;
    public GameObject canvas2P;

    void Start()
    {
        int mode = PlayerPrefs.GetInt("GameMode", 0);

        if (mode == 0)
        {
            // NPCモード（1人プレイ）のとき
            if(npcPenguin != null) npcPenguin.SetActive(true);
            if(player2Penguin != null) player2Penguin.SetActive(false);

            // 1Pのゲージだけ表示して、2Pのゲージは非表示にする
            if(canvas1P != null) canvas1P.SetActive(true);
            if(canvas2P != null) canvas2P.SetActive(false);
        }
        else
        {
            // 2P対戦モードのとき
            if(npcPenguin != null) npcPenguin.SetActive(false);
            if(player2Penguin != null) player2Penguin.SetActive(true);

            // 1P・2P両方のゲージを表示する
            if(canvas1P != null) canvas1P.SetActive(true);
            if(canvas2P != null) canvas2P.SetActive(true);
        }
    }
}
