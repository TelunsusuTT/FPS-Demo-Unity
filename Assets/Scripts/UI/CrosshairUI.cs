using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditorInternal.VersionControl.ListControl;

public class CrosshairUI : MonoBehaviour
{
    [Header("Refs")]
    public PlayerController player;
    public WeaponController weapon;     // 用于订阅 Fired（推荐）
    public GameObject crosshairRoot;    // 你的 Crosshair（用于整体显示/隐藏）

    [Header("Dashes (RectTransform)")]
    public RectTransform rightDash;
    public RectTransform leftDash;
    public RectTransform upperDash;
    public RectTransform lowerDash;

    [Header("Spread Settings")]
    public float baseOffset = 10f;      // 你现在的初始距离 10
    public float maxOffset = 40f;       // 你要的上限 40（离中心的距离）
    public float moveAdd = 18f;         // 移动最多额外扩散多少（可调）
    public float lookAdd = 12f;         // 旋转最多额外扩散多少（可调）
    public float fireKickAdd = 6f;      // 每次开火瞬间扩散（可调）

    [Header("Smoothing")]
    public float expandSpeed = 18f;     // 扩散变大的响应速度
    public float recoverSpeed = 10f;    // 回正速度
    public float fireKickRecoverSpeed = 30f;

    float currentOffset;
    float fireKick;                    // 开火的瞬时扩散累积

    void OnEnable()
    {
        currentOffset = baseOffset;
        if (weapon != null) weapon.Fired += OnFired;
    }

    void OnDisable()
    {
        if (weapon != null) weapon.Fired -= OnFired;
    }

    void Update()
    {
        if (player == null || crosshairRoot == null) return;

        // 1) ADS：隐藏准星（你原本的逻辑）
        bool shouldShow = !player.isAiming;
        if (crosshairRoot.activeSelf != shouldShow)
            crosshairRoot.SetActive(shouldShow);

        // ADS 时你想“纯净机瞄”：直接不更新扩散也可以
        if (!shouldShow) return;

        // 2) 计算移动强度（0~1）
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        float move = Mathf.Clamp01(new Vector2(h, v).magnitude); // 走路=1，站着=0

        // 3) 计算视角旋转强度（0~1）
        float mx = Mathf.Abs(Input.GetAxis("Mouse X"));
        float my = Mathf.Abs(Input.GetAxis("Mouse Y"));

        // 这个阈值/归一化系数可调：数值越小，越容易触发“甩枪扩散”
        float look = Mathf.Clamp01((mx + my) / 2.0f);

        // 4) 开火 kick 衰减（让它慢慢回落）
        fireKick = Mathf.MoveTowards(fireKick, 0f, fireKickRecoverSpeed * Time.deltaTime);

        // 5) 目标 offset（基础 + 移动 + 旋转 + 开火kick）
        float targetOffset = baseOffset + move * moveAdd + look * lookAdd + fireKick;

        // 限制上限：离中心最大 40
        targetOffset = Mathf.Clamp(targetOffset, baseOffset, maxOffset);

        // 6) 平滑：变大用 expandSpeed，变小用 recoverSpeed（手感更像 FPS）
        float speed = (targetOffset > currentOffset) ? expandSpeed : recoverSpeed;
        currentOffset = Mathf.Lerp(currentOffset, targetOffset, speed * Time.deltaTime);

        ApplyOffset(currentOffset);
    }

    void ApplyOffset(float offset)
    {
        if (rightDash) rightDash.anchoredPosition = new Vector2(+offset, 0f);
        if (leftDash) leftDash.anchoredPosition = new Vector2(-offset, 0f);
        if (upperDash) upperDash.anchoredPosition = new Vector2(0f, +offset);
        if (lowerDash) lowerDash.anchoredPosition = new Vector2(0f, -offset);
    }

    void OnFired()
    {
        // 让站桩连射也会扩散（因为你的 recoil 不会触发 mouse input）
        fireKick = Mathf.Clamp(fireKick + fireKickAdd, 0f, (maxOffset - baseOffset));
    }
}
