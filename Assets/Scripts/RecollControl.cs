using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class RecollControl : MonoBehaviour
{
    public float X = -3f;
    public float speed = 10;
    public float returnSpeed = 5;

    private float targetRotation;
    private float currentRotation;

    [SerializeField] private WeaponController weapon; // Inspector拖引用最稳

    void OnEnable()
    {
        if (weapon != null) weapon.Fired += OnFired;
    }

    void OnDisable()
    {
        if (weapon != null) weapon.Fired -= OnFired;
    }

    void Update()
    {
        //恢复 -枪口向下
        targetRotation = Mathf.Lerp(targetRotation, 0, returnSpeed * Time.deltaTime);
        //旋转 - 枪口向上
        currentRotation = Mathf.Lerp(currentRotation, targetRotation, speed * Time.deltaTime);
        //应用旋转 - 只影响x
        transform.localRotation = Quaternion.Euler(currentRotation, transform.localEulerAngles.y, 0);
    }

    private void OnFired()
    {
        targetRotation += X;
    }
}