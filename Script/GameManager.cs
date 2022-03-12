using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using Random = UnityEngine.Random;


public class GameManager : NetworkBehaviour
{
    private static GameManager instance;  // 创建单例模式
    
    public static GameManager GetInstance()
    {
        return instance;
    }
    public UIGame ui;
    public Team[] teams;
    public Player localPlayer;
    public int respawnTime=10;
    
    // 将每个组的数量封装成列表List，同时设置成SyncList，实现可以服务器到客户端的同步传输
    public SyncList<int> size = new SyncList<int>();
    public SyncList<int> score = new SyncList<int>();
    
    private void Awake()
    {
        instance = this;
    }

    // 每个组的数量初始化为0
    public override void OnStartServer()
    {
        if (size.Count != teams.Length)
        {
            for (int i = 0; i < teams.Length; ++i)
            {
                size.Add(0);
                score.Add(0);
            }
        }
    }

    public override void OnStartClient()
    {
        size.Callback += ui.OnTeamSizeChanged;
        score.Callback += ui.OnTeamScoreChanged;
        for (int i = 0; i < teams.Length; ++i)
        {
            ui.OnTeamSizeChanged(SyncList<int>.Operation.OP_SET,i,0,0);
            ui.OnTeamScoreChanged(SyncList<int>.Operation.OP_SET,i,0,0);
        }
    }
    
    // 得到四个组中组中数量最小的组
    public int GetTeamFill()
    {
        int teamNo = 0;
        int min = size[0];
        for (int i = 0; i < teams.Length; ++i)
        {
            if (size[i] < min)
            {
                min = size[i];
                teamNo = i;
            }
        }
        return teamNo;
    }
    
    // 实现随机生成在生成地(cube)，并生成的角色互相不碰撞
    public Vector3 GetSpawnPosition(int teamIndex)
    {
        Vector3 pos = teams[teamIndex].spawn.position; // 得到生成位置，为生成地(cube)的中心地点
        BoxCollider col = teams[teamIndex].spawn.GetComponent<BoxCollider>(); // 获取生成地(cube)的碰撞组件col
        if (col != null)
        {
            pos.y = col.transform.position.y; // 将碰撞体的y赋值给pos的y，因为y是不需要变的 
            int counter = 10; 
            do
            {
                pos.x = Random.Range(col.bounds.min.x, col.bounds.max.x); // 获得生成地在x轴范围内的随机值
                pos.z = Random.Range(col.bounds.min.z, col.bounds.max.z); // 获得生成地在z轴范围内的随机值
                counter--;
            } while (!col.bounds.Contains(pos) && counter > 0); 
        }
        return pos;
    }

    public void DisplayDeath()
    {
        Player other = localPlayer.killedBy.GetComponent<Player>();
        ui.SetDeathText(other.myName, teams[other.teamIndex]);
        // 启动协程，来实时显示倒计时时间
        StartCoroutine(SpawnRoutine());
    }

    IEnumerator SpawnRoutine()
    {
        float targetTime = Time.time + respawnTime;
        while (targetTime - Time.time > 0)
        {
            ui.SetSpawnDelay(targetTime - Time.time); // 设置倒计时时间文本
            yield return null;
        }
        ui.DisableDeath(); // 复活清空文本
        localPlayer.CmdRespawn(); // 在服务器调用复活函数
    }
}

[System.Serializable]
public class Team
{
    public string name;   // 组名
    public Material material; // 组的材质
    public Transform spawn; // 角色生成的地点
    public Color color; // 颜色
}
