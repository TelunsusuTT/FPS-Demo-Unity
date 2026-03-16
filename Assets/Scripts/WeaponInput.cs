using UnityEngine;

public class WeaponInput : MonoBehaviour
{
    private WeaponController weapon;
    private PlayerController pc;

    void Start()
    {
        weapon = GetComponent<WeaponController>();
        pc = GetComponent<PlayerController>();
    }

    void Update()
    {
        if (Input.GetMouseButton(0) && weapon != null && pc != null && !pc.highSpeed)
        {
            weapon.TryFire();
        }
    }
}