using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using Random = UnityEngine.Random;

public class BotSpawner : MonoBehaviour
{
    public int maxBots; // 机器人最大数量
    public GameObject[] prefabs;

    private void Awake() // 如果是单人游戏就加入机器人，是联机就不加入
    {
        if (NetworkManager.numPlayers > 1) enabled = false;
    }

    private void Start() // 要加Private，否则没反应
    {
        StartCoroutine(SpawnBot());
    }
    
    IEnumerator SpawnBot()
    {
        yield return new WaitForSeconds(1);
        for (int i = 0; i < maxBots; ++i)
        {
            int randIndex = Random.Range(0, prefabs.Length);
            GameObject obj = Instantiate(prefabs[randIndex], Vector3.zero, Quaternion.identity);
            Player p = obj.GetComponent<Player>();
            p.teamIndex = GameManager.GetInstance().GetTeamFill();
            NetworkServer.Spawn(obj, prefabs[randIndex].GetComponent<NetworkIdentity>().assetId);

            GameManager.GetInstance().size[p.teamIndex]++;
            GameManager.GetInstance().ui.OnTeamSizeChanged(SyncList<int>.Operation.OP_SET, p.teamIndex, 0 ,0 );

            yield return new WaitForSeconds(0.25f);
        }
    }
    
}
