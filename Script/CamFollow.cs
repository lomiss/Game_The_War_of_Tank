using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class CamFollow : MonoBehaviour
{
    public Transform target;
    public float distance = 10f;
    public float height = 5f;
    
    private Vector3 m_camRot;
    
    [HideInInspector] public Transform camTransform;
    
    private void Awake()
    {
        camTransform = transform;
    }
    
    void Update()
    {
        if (!target) return; //没有获得玩家坐标就return
        
        //获取当前摄像机的旋转角度，返回一个四元数
        Quaternion currentRotation = Quaternion.Euler(0, transform.eulerAngles.y, 0);
        Vector3 pos = target.position; //得到玩家的坐标
        // 四元素*(0,0,1)=当前旋转方向上1单位的距离，再*距离=当前旋转方向上的距离（一个Vector）
        pos -= currentRotation * Vector3.forward * Mathf.Abs(distance);
        pos.y = target.position.y + Mathf.Abs(height); //在y方向上偏移一定的高度，相当于将摄像机的x和z轴定死
        transform.position = pos; // 将计算完后的pos赋值给摄像机
        
        transform.LookAt(target); // 当前摄像机指向玩家
        
        // 画蛇添足，对游戏效果没有影响，留坑
        //transform.position = target.position - transform.forward * Mathf.Abs(distance);

        if (Input.GetMouseButton(1))
        {
            m_camRot = camTransform.eulerAngles;
            float rh = Input.GetAxis("Mouse X"); // 返回鼠标在x轴上的偏移量，左负
            m_camRot.y += rh * 4;  // 乘上倍数，增加右键切换速度
            // 暂不支持纵向切换，因为视角会跑到地图外，留坑
            // float rv = Input.GetAxis("Mouse Y"); // 返回鼠标在y轴上的偏移量，下负
            // m_camRot.x -= rv * 4;   
            camTransform.eulerAngles = m_camRot;
        }
    }
}
