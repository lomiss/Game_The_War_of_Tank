using System.Collections;
using UnityEngine;
using Mirror;

public class ObjectSpawner : NetworkBehaviour
{
    public GameObject prefab; // 物体预制体
    public bool respawn; // 是否重新生成
    public int respawmTime; // 生成时间
    public float nextSpawn;

    [HideInInspector] public GameObject obj;

    public override void OnStartServer()
    {
        if (obj != null && obj.activeInHierarchy) return;
        StartCoroutine(SpawnRoutine());
    }

    IEnumerator SpawnRoutine()
    {
        float delay = Mathf.Clamp(nextSpawn - Time.time, 0, respawmTime);
        yield return new WaitForSeconds(delay);
        Instantiate();
    }

    [Server]
    public void Instantiate()
    {
        obj = Instantiate(prefab, transform.position, transform.rotation);
        obj.GetComponent<Powerup>().parentID = GetComponent<NetworkIdentity>().netId;
        NetworkServer.Spawn(obj);
    }

    [Server]
    public void Destroy()
    {
        Destroy(obj);
        NetworkServer.UnSpawn(obj);
        obj = null;
        nextSpawn = Time.time + respawmTime;
        if (respawn)
        {
            StartCoroutine(SpawnRoutine());
        }
    }
    
}
