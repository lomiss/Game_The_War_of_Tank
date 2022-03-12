using System;
using UnityEngine;
using UnityEngine.UI;
using Mirror;

public class Player : NetworkBehaviour
{
    public float MoveSpeed = 8; //水平和垂直的速度
    public float nextFire; //下一次开火的时间
    public float fireRate = 0.75f; //两次开火至少相隔的时间
    
    private Rigidbody rb; //刚体
    public Transform turret; //炮头
    public Transform shotPos; // 开火位置
    public GameObject BulletPrefab;
    public MeshRenderer[] renderers;
    public Text label;  // 玩家名
    public Image hpImage; // 血条贴图,不同颜色表示不同阵营
    public Slider healthSlider; // 血条
    public Slider shieldSlider; // 护盾
    
    [HideInInspector] public CamFollow camFollow;
    [HideInInspector] public int maxHealth;
    [HideInInspector] public string myName;
    
    [HideInInspector] [SyncVar] public GameObject killedBy; // 击杀对象
    
    [HideInInspector] [SyncVar] public int teamIndex = 0;
    
    [HideInInspector] [SyncVar(hook=nameof(RotateTurret))] public int turretRotation;

    [HideInInspector] [SyncVar(hook = nameof(OnHealthChange))] public int health = 10;
    
    [HideInInspector] [SyncVar(hook = nameof(OnShieldChange))] public int shield;
    public override void OnStartLocalPlayer()
    {
        Camera.main.GetComponent<CamFollow>().target = transform;
        GameManager.GetInstance().localPlayer = this;
    }

    public override void OnStartClient()
    {
        Team team = GameManager.GetInstance().teams[teamIndex];
        for (int i = 0; i < renderers.Length; i++) // 遍历所有需要渲染的对象，即杆子和旗面
        {
            renderers[i].material = team.material;
        }
        // HP颜色的修改
        myName = "Player";
        label.text = myName;
        hpImage.color = team.color;
    }
    
    private void Awake()
    {
        rb = transform.GetComponent<Rigidbody>();
        camFollow = Camera.main.transform.GetComponent<CamFollow>();
        maxHealth = health;
    }
 
    private void FixedUpdate()
    {
        if (!isLocalPlayer) return; // 如果不是本地角色，就跳过
        Vector2 moveDir;
        // 获取键盘输入的水平和纵向的值，分别是1和-1，代表两个方向，并传给moveDir这个方向变量
        if (Input.GetAxisRaw("Horizontal") == 0 && Input.GetAxisRaw("Vertical") == 0)
        {
            moveDir.x = 0;
            moveDir.y = 0;
        }
        else
        {
            moveDir.x = Input.GetAxisRaw("Horizontal");  //左返回-1，右返回1
            moveDir.y = Input.GetAxisRaw("Vertical");  //下返回-1，上返回1
            if (moveDir != Vector2.zero) // 如果方向变量不是零向量
            {
                // 调整方向，因为u3d的平面是x和z轴，然后再乘上摄像机方向的四元数
                transform.rotation = Quaternion.LookRotation(new Vector3(moveDir.x, 0, moveDir.y))*Quaternion.Euler(0,camFollow.camTransform.eulerAngles.y,0); 
                //transform.forward是一个变量，它是根据当前方向计算出的方向变量 
                Vector3 movementDir = transform.forward * MoveSpeed * Time.deltaTime; // 当前方向的单位向量 * 速度 * 时间 = 当前方向的偏移量  
                rb.MovePosition(rb.position + movementDir);
            }
        }

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition); // 返回从摄像机到当前鼠标点的一个射线
        Plane plane = new Plane(Vector3.up, Vector3.up); // 新建一个平面，因为玩家对象在(0,0,0)，所以参数中的法线和经过的点都是(0,1,0)
        float distance = 0f;
        Vector3 hitPos = Vector3.zero;
        // 判断平面和射线是否相交，并将射线起点到交点的距离返回给distance
        if (plane.Raycast(ray, out distance)) 
        {
            hitPos = ray.GetPoint(distance) - transform.position; // 获得射线在distance位置的坐标 - 玩家的坐标 = 相对玩家的交点坐标
        }
        Vector2 direction = new Vector2(hitPos.x, hitPos.z);
        if (direction != Vector2.zero)
        {
            // 因为炮台的z轴要指向射线和平面的交点，所以用LookRotation函数，只需要用到forward参数即可(z轴指向forward)，
            // 最后返回一个表示旋转的四元数，并得到它转换后欧拉角中绕y轴旋转的角度
            turretRotation = (int) (Quaternion.LookRotation(new Vector3(direction.x, 0, direction.y)).eulerAngles.y); 
            turret.rotation = Quaternion.Euler(-90, turretRotation, 0);   // 客户端实现转向
            CmdRotateTurret(turretRotation); // 同步到服务器
        }
        
        //开火
        if (Input.GetButton("Fire1"))
        {
            if (Time.time > nextFire)
            {
                nextFire = Time.time + fireRate;
                CmdShoot((short) (shotPos.position.x * 10), (short) (shotPos.position.z * 10));
            }
        }
        
    }
    
    [Command]
    void CmdRotateTurret(int value)
    {
        turretRotation = value;
    }
    
    void RotateTurret(int oldValue, int newValue) 
    {
        //因为炮台旋转，实际上是绕y轴旋转的，所以将y轴旋转的角度封装成四元数赋值给炮台的rotation即可
        turret.rotation = Quaternion.Euler(-90, newValue, 0); //因为炮台本来就旋转了90，所以这里要不变
    }
    
    [Command]
    public void CmdShoot(short xPos, short zPos)
    {
        Vector3 shotCenter =
            Vector3.Lerp(shotPos.position, new Vector3(xPos / 10f, shotPos.position.y, zPos / 10f), 0.6f);
        // 创建子弹，并同步到客户端
        GameObject obj = Instantiate(BulletPrefab, shotCenter, Quaternion.Euler(0, turret.eulerAngles.y, 0)); //把预制体实例化出子弹
        obj.GetComponent<Bullet>().owner = gameObject;
        NetworkServer.Spawn(obj,obj.GetComponent<NetworkIdentity>().assetId); // 在服务器端生成，并同步到客户端
        // 所有客户端进行特效显示
        RpcOnShot();
    }
    
    [ClientRpc]
    protected void RpcOnShot()
    {
        // 显示特效
    }
    
    [Server]
    public void TankDamage(Bullet bullet)
    {
        if (shield > 0)
        {
            shield--; // 一炮的伤害为3，刚好把护盾抵消一格
            return;
        }
        health -= bullet.damage;
        if (health <= 0)
        {
            // 当前玩家死亡
            Player other = bullet.owner.GetComponent<Player>();
            GameManager.GetInstance().score[other.teamIndex]++;
            // 改变分数
            GameManager.GetInstance().ui.OnTeamScoreChanged(SyncList<int>.Operation.OP_SET,other.teamIndex,0,0);
            health = maxHealth;
            RpcRespawn();
        }
    }

    protected void OnHealthChange(int oldValue, int newValue)
    {
        health = newValue;
        healthSlider.value = (float)health / maxHealth;
    }
    
    protected void OnShieldChange(int oldValue, int newValue)
    {
        shield = newValue;
        shieldSlider.value = shield;
    }
    
    [Command]
    public void CmdRespawn()
    {
        RpcRespawn();
    }
    
    [ClientRpc]
    protected virtual void RpcRespawn()
    {
        gameObject.SetActive(!gameObject.activeInHierarchy);
        if (!isLocalPlayer) return;
        bool isActive = gameObject.activeInHierarchy;
        if (isActive)
        {
            // 重新放到出生点
            ResetPosition();
        }
        else
        {
            camFollow.target = killedBy.transform;
            GameManager.GetInstance().DisplayDeath();
        }
        
    }
    
    public void ResetPosition()
    {
        camFollow.target = transform;
        transform.position = GameManager.GetInstance().GetSpawnPosition(teamIndex);
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        transform.rotation = Quaternion.identity;
    }
}
