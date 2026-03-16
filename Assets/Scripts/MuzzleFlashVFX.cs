using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MuzzleFlashVFX : MonoBehaviour
{
    [SerializeField] private WeaponController weapon; // мо Player ио╣д WeaponController
    [SerializeField] private Transform firePoint;     // мо FirePoint Transform
    [SerializeField] private GameObject muzzleFlashPrefab; // мо MuzzleFlash prefab

    private void OnEnable()
    {
        if (weapon != null) weapon.Fired += OnFired;
    }

    private void OnDisable()
    {
        if (weapon != null) weapon.Fired -= OnFired;
    }

    private void OnFired()
    {
        if (muzzleFlashPrefab == null || firePoint == null) return;

        ObjectPoolManager.Instance.Spawn(
            muzzleFlashPrefab,
            firePoint.position,
            firePoint.rotation
        );
    }
}
