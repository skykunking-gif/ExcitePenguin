using UnityEngine;

public class StageGameInitializer : MonoBehaviour
{
    public GameObject npcPenguin;
    public GameObject player2Penguin;
    void Start()
    {
        //タイトル画面で保存されたモードを読み込む
        int mode = PlayerPrefs.GetInt("GameMode",0);

        if (mode == 0)
        {
            //NPCモードなら
            if(npcPenguin != null) npcPenguin.SetActive(true);
            if(player2Penguin != null) player2Penguin.SetActive(false);
        }
        else
        {
            //2pモード
            if(npcPenguin != null) npcPenguin.SetActive(false);
            if(player2Penguin != null) player2Penguin.SetActive(true);
        }
    }
}
