using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitmarkerUI : MonoBehaviour
{
    [SerializeField] private GameObject hitmarkerImageGO; // мовснОлЕ HitmarkerImage
    [SerializeField] private float showTime = 0.1f;

    private float timer;

    private void Awake()
    {
        if (hitmarkerImageGO != null)
            hitmarkerImageGO.SetActive(false);
    }

    public void Show()
    {
        if (hitmarkerImageGO == null) return;

        hitmarkerImageGO.SetActive(true);
        timer = showTime;
    }

    private void Update()
    {
        if (timer <= 0f) return;

        timer -= Time.deltaTime;
        if (timer <= 0f && hitmarkerImageGO != null)
        {
            hitmarkerImageGO.SetActive(false);
        }
    }
}