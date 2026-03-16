using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoDespawn : MonoBehaviour
{
    public float lifeTime = 0.1f;

    private float timer;

    private void OnEnable()
    {
        timer = 0f;

        // 如果是粒子系统，确保每次重播（可选）
        var ps = GetComponent<ParticleSystem>();
        if (ps != null)
        {
            ps.Clear(true);
            ps.Play(true);
        }
    }

    private void Update()
    {
        timer += Time.deltaTime;
        if (timer >= lifeTime)
        {
            ObjectPoolManager.Instance.Despawn(gameObject);
        }
    }
}
