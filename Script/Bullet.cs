using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Bullet : NetworkBehaviour
{
    public float speed = 15;
    public int damage = 3;
    
    private Rigidbody myRigidbody;

    [HideInInspector] [SyncVar] public GameObject owner;
    
    private void Awake()
    {
        myRigidbody = GetComponent<Rigidbody>();
    }

    private void Start()
    {
        myRigidbody.velocity = speed * transform.forward;
    }

    private void OnTriggerEnter(Collider other)
    {
        GameObject obj = other.gameObject;  // 获取子弹碰撞的对象
        Player player = obj.GetComponent<Player>(); // 获取该对象的player组件，如果无返回Null
        if (player != null) // 碰撞对象是坦克
        {
            if (player.gameObject == owner) return; // 是自己
            if (player.teamIndex == owner.GetComponent<Player>().teamIndex) return; // 是同类
            player.killedBy = owner;
        }
        if (!isServer) return;
        if (player) player.TankDamage(this); // 触发坦克少血机制
        Destroy(this.gameObject); // 子弹销毁
    }
}
