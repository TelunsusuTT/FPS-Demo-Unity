using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class EnemyController : MonoBehaviour, IDamageable
{
    public int hp = 10;
    public GameObject bombEffect;

    public event Action Died;

    public void TakeDamage(int damage)
    {
        hp -= damage;
        if (hp <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Died?.Invoke(); // ֪ͨ AI

        if (bombEffect != null)
            Instantiate(bombEffect, transform.position, transform.rotation);

        Destroy(gameObject);
    }
}