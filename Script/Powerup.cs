using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Powerup : NetworkBehaviour
{
    // mirror将NetworkInstanceId替换成uint
    [HideInInspector] [SyncVar] public uint parentID;
    private ObjectSpawner spawner;

    public override void OnStartClient()
    {
        // 找到物品
        GameObject parentObj = NetworkIdentity.spawned[parentID].gameObject;
        spawner = parentObj.GetComponent<ObjectSpawner>();
        spawner.obj = gameObject;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isServer) return;
        GameObject obj = other.gameObject;
        Player player = obj.GetComponent<Player>();
        if (player == null) return;
        if(Apply(player)) spawner.Destroy();
    }
    
    public virtual bool Apply(Player p)
    {
        return false;
    }
    
}
