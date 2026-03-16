using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class WeaponController : MonoBehaviour
{
    //射击方向
    [Header("Aim")]
    public Camera playerCam;                 // Inspector 拖你的主相机
    public float maxAimDistance = 500f;      // 朝天时用的默认距离
    public LayerMask aimMask = ~0;           // 默认射到所有层（后面建议排除 Player）

    //UI备弹
    [Header("Ammo")]
    public int CurrentAmmo = 30;
    public int ReserveAmmo = 60;
    public int magSize = 30;


    //发射位置
    public GameObject FirePoint;
    //子弹
    public GameObject BulleyPrefab;
    //火焰效果
    public GameObject FirePrefab;
    //时间间隔
    public float bulletInterval = 0.2f;
    private float timer = 0;

    public event Action Fired; // 关键：开火事件

    private PlayerController pc;
    private RecollControl rc;

    void Update()
    {
        timer += Time.deltaTime;
    }

    public bool CanFire()
    {
        return timer >= bulletInterval;
    }

    public bool TryFire()
    {
        if (!CanFire()) return false;
        if (CurrentAmmo <= 0)
        {
            Debug.Log("换弹中");
            if(ReserveAmmo >= magSize)
            {
                CurrentAmmo = magSize;
                ReserveAmmo -= magSize;
            }
            else if(ReserveAmmo < magSize && ReserveAmmo > 0)
            {
                CurrentAmmo = ReserveAmmo;
                ReserveAmmo = 0;
            }
            else
            {
                Debug.Log("备弹用完！");
                return false;
            }
                
        }

        timer = 0f;
        CurrentAmmo--;
        

        // 先广播：我开火了（recoil/ui/audio 都订阅它）
        Fired?.Invoke();

        //子弹已池化
        // 1) 计算相机中心射线
        Vector3 aimPoint = playerCam.transform.position + playerCam.transform.forward * maxAimDistance; // 默认：朝天/没命中时
        Ray ray = new Ray(playerCam.transform.position, playerCam.transform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, maxAimDistance, aimMask, QueryTriggerInteraction.Ignore))
        {
            aimPoint = hit.point;
        }

        // 2) 用 FirePoint → aimPoint 得到真正的发射方向
        Vector3 dir = (aimPoint - FirePoint.transform.position).normalized;

        // 3) 让子弹 Spawn 时就朝向这个方向（关键）
        Quaternion rot = Quaternion.LookRotation(dir, Vector3.up);

        ObjectPoolManager.Instance.Spawn(BulleyPrefab, FirePoint.transform.position, rot);

        return true;
    }
}
