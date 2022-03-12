using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class PlayerBot : Player
{
    public float range = 6f; // 射程
    private List<GameObject> inRange = new List<GameObject>();

    private NavMeshAgent agent;
    private Vector3 targetPoint;
    private float nextShot;
    private bool isDead = false;

    private void Start()
    {
        camFollow = Camera.main.GetComponent<CamFollow>();
        agent = GetComponent<NavMeshAgent>();
        agent.speed = MoveSpeed;

        targetPoint = GameManager.GetInstance().GetSpawnPosition(teamIndex);
        agent.Warp(targetPoint);
        
        Team team = GameManager.GetInstance().teams[teamIndex];
        for (int i = 0; i < renderers.Length; ++i) renderers[i].material = team.material;
        // 改变名称
        myName = label.text = "Bot" + System.String.Format("{0:0000}", Random.Range(1, 9999));
        hpImage.color = team.color;
        health = maxHealth;
        shield = 0;
        //OnHealthChange(health);
        //OnShieldChange(shield);
        StartCoroutine(DetectPlayers());
        // InvokeRepeating(); 可以用这个函数代替DetectPlayers
    }
    IEnumerator DetectPlayers()
    {
        yield return new WaitForEndOfFrame();
        while (true)
        {
            inRange.Clear();
            Collider[] cols = Physics.OverlapSphere(transform.position, range, LayerMask.GetMask("Player"));
            for (int i = 0; i < cols.Length; ++i)
            {
                Player p = cols[i].gameObject.GetComponent<Player>();
                if (p.teamIndex != teamIndex && !inRange.Contains(cols[i].gameObject))
                {
                    inRange.Add(cols[i].gameObject);
                }
            }
            yield return new WaitForSeconds(1);
        }
    }

    private void RandomPoint(Vector3 center, float range, out Vector3 result)
    {
        result = Vector3.zero;
        for (int i = 0; i < 30; ++i)
        {
            Vector3 randomPoint = center + (Vector3) Random.insideUnitCircle*range;
            NavMeshHit hit;
            if (NavMesh.SamplePosition(randomPoint, out hit, 1f, NavMesh.AllAreas))
            {
                result = hit.position;
                break;
            }
        }

        agent.SetDestination(result);
    }

    void FixedUpdate()
    {
        if (isDead) return;
        if (inRange.Count == 0)
        {
            if (Vector3.Distance(transform.position, targetPoint) < agent.stoppingDistance)
            {
                RandomPoint(transform.position, range * 3, out targetPoint);
            }
        }
        else
        {
            if (Vector3.Distance(transform.position, targetPoint) < agent.stoppingDistance)
            {
                RandomPoint(inRange[0].transform.position, range * 2, out targetPoint);
            }

            for (int i = 0; i < inRange.Count; ++i)
            {
                RaycastHit hit;
                if (Physics.Linecast(transform.position, inRange[i].transform.position, out hit))
                {
                    Vector3 lookPos = inRange[i].transform.position;
                    turret.LookAt(lookPos);
                    turret.eulerAngles = new Vector3(-90, turret.eulerAngles.y, 0);
                    Vector3 shotDir = lookPos - transform.position;
                    if (Time.time > nextFire)
                    {
                        nextFire = Time.time + fireRate;
                        Shoot((short) (shotDir.x * 10), (short) (shotDir.z * 10));
                    }
                    break;
                }
            }
        }
    }
    
    public void Shoot(short xPos, short zPos)
    {
        Vector3 shotCenter =
            Vector3.Lerp(shotPos.position, new Vector3(xPos / 10f, shotPos.position.y, zPos / 10f), 0.6f);
        // 创建子弹，并同步到客户端
        GameObject obj = Instantiate(BulletPrefab, shotCenter, Quaternion.Euler(0, turret.eulerAngles.y, 0)); //把预制体实例化出子弹
        obj.GetComponent<Bullet>().owner = gameObject;
        NetworkServer.Spawn(obj,obj.GetComponent<NetworkIdentity>().assetId); // 在服务器端生成，并同步到客户端
    }

    protected override void RpcRespawn()
    {
        StartCoroutine(Respawn());
    }

    IEnumerator Respawn()
    {
        isDead = true;
        inRange.Clear();
        agent.isStopped = true;
        ToggleComponents(false);
        yield return new WaitForSeconds(GameManager.GetInstance().respawnTime);
        ToggleComponents(true);
        targetPoint = GameManager.GetInstance().GetSpawnPosition(teamIndex);
        transform.position = targetPoint;
        agent.Warp(targetPoint);
        agent.isStopped = false;
        isDead = false;
    }

    void ToggleComponents(bool state)
    {
        GetComponent<Rigidbody>().isKinematic = state;
        GetComponent<Collider>().enabled = state;
        for (int i = 0; i < transform.childCount; ++i)
        {
            transform.GetChild(i).gameObject.SetActive(state);
        }
    }
}
