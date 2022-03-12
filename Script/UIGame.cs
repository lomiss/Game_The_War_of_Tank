using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;

public class UIGame : MonoBehaviour
{
    public Slider[] teamSize;
    public Text[] teamScore;
    public Text deathText;
    public Text spawnDelayText; // 死亡上帝时间
    
    public void OnTeamSizeChanged(SyncList<int>.Operation op, int index, int oldItem, int newItem)
    {
        teamSize[index].value = GameManager.GetInstance().size[index];
    }

    public void OnTeamScoreChanged(SyncList<int>.Operation op, int index, int oldItem, int newItem)
    {
        teamScore[index].text = GameManager.GetInstance().score[index].ToString();
    }
    
    public void SetDeathText(string playerName, Team team)
    {
        deathText.text = "KILLED BY\n<color=#" + ColorUtility.ToHtmlStringRGB(team.material.color) + ">" + playerName +
                         "</color>";
    }

    public void SetSpawnDelay(float time)
    {
        spawnDelayText.text = Mathf.Ceil(time) + ""; // 显示倒计时时间
    }

    public void DisableDeath() // 复活后将文本清空
    {
        deathText.text = string.Empty;
        spawnDelayText.text = string.Empty;
    }
    
}
