using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AmmoUI : MonoBehaviour
{
    public WeaponController weapon;
    public Text ammoText;

    void Update()
    {
        if (weapon == null) return;

        ammoText.text = weapon.CurrentAmmo + " / " + weapon.ReserveAmmo;
    }
}
