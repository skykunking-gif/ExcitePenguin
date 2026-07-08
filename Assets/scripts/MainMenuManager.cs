using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    //コースを選ぶ
    //手作りコース１
    public void SelectManualStage()
    {
        SceneManager.LoadScene("Stage_Manual");
    }

    //自動生成コースへ移動
    public void SelectAutoStage()
    {
        SceneManager.LoadScene("Stage_Auto");
    }
    
    //対戦相手を選ぶ
    //NPCと
    public void SetModeNPC()
    {
        PlayerPrefs.SetInt("GameMode",0);
        PlayerPrefs.Save();
        Debug.Log("モード変更： vs NPC");
    }

    //２人で
    public void SetMode2P()
    {
        PlayerPrefs.SetInt("GameMode",1);
        PlayerPrefs.Save();
        Debug.Log("モード変更： vs 2P");
    }
}
