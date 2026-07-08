using UnityEngine;

public class StageGameInitializer : MonoBehaviour
{
    public GameObject npcPenguin; 
    public GameObject player2Penguin; 

    void Start()
    {
        int mode = PlayerPrefs.GetInt("GameMode", 0);

        if (mode == 0)
        {
            // NPCモード（1人プレイ）のとき
            if(npcPenguin != null) npcPenguin.SetActive(true);
            if(player2Penguin != null) player2Penguin.SetActive(false);
        }
        else
        {
            // 2P対戦モードのとき
            if(npcPenguin != null) npcPenguin.SetActive(false);
            if(player2Penguin != null) player2Penguin.SetActive(true);
        }
    }
}
