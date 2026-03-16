using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletController : MonoBehaviour
{
    public float speed = 50;
    private Rigidbody rb;

    public GameObject effectPrefab;

    private float lifeTimer = 0f;
    public float lifeTime = 1f; // 原来 Destroy(gameObject, 1f)

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    // 关键：每次从对象池 Spawn 出来 -> SetActive(true) -> OnEnable 会执行
    private void OnEnable()
    {
        lifeTimer = 0f;

        // 重置物理状态（非常重要，否则会继承上一次的速度/旋转）
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        // 发射：直接设速度，比 AddForce 更可控（不受质量影响）
        rb.velocity = transform.forward * speed;
    }

    private void Update()
    {
        lifeTimer += Time.deltaTime;
        if (lifeTimer >= lifeTime)
        {
            ObjectPoolManager.Instance.Despawn(gameObject);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        // 如果打到可破坏物体
        if (collision.gameObject.CompareTag("Des"))
        {
            //保险：确保击碎过的物体不会再被附加上刚体
            Rigidbody rbody = collision.gameObject.GetComponent<Rigidbody>();
            if (rbody == null)
            {
                //本身没有刚体，需要我们给这个碎块添加刚体
                rbody = collision.gameObject.AddComponent<Rigidbody>();
            }
            //给刚体一个在:子弹方向，在碰撞点,一个冲量力
            rbody.AddForceAtPosition(transform.forward * 10, collision.contacts[0].point, ForceMode.Impulse);
            //先销毁碰撞体
            Destroy(collision.gameObject, 2f);
        }

        //如果打到敌人
        // 命中任何实现了 IDamageable 的对象就造成伤害
        if (collision.gameObject.TryGetComponent<IDamageable>(out var damageable))
        {
            damageable.TakeDamage(2);
            HitmarkerUI ui = FindObjectOfType<HitmarkerUI>();
            if(ui != null)
            {
                ui.Show();
            }
        }

        // ImpactEffect已池化
        if (effectPrefab != null && collision.contacts.Length > 0)
        {
            Vector3 pos = collision.contacts[0].point;
            Quaternion rot = Quaternion.LookRotation(collision.contacts[0].normal);

            ObjectPoolManager.Instance.Spawn(effectPrefab, pos, rot);
        }

        // 子弹本体不 Destroy -> 回收进对象池
        ObjectPoolManager.Instance.Despawn(gameObject);
    }
}

/*using UnityEngine;

public class BulletController : MonoBehaviour
{
    public float speed = 50;
    private Rigidbody rb;

    public GameObject effectPrefab;
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>(); 
        rb.AddForce(transform.forward * speed, ForceMode.Impulse);
        Destroy(gameObject, 1f);
    }

    private void OnCollisionEnter(Collision collision)
    {
        //如果打到可破坏物体
        if(collision.gameObject.tag == "Des")
        {
            //保险：确保击碎过的物体不会再被附加上刚体
            Rigidbody rbody = collision.gameObject.GetComponent<Rigidbody>();
            if(rbody == null)
            {
                //本身没有刚体，需要我们给这个碎块添加刚体
                rbody = collision.gameObject.AddComponent<Rigidbody>();
            }
            //给刚体一个在:子弹方向，在碰撞点,一个冲量力
            rbody.AddForceAtPosition(transform.forward * 10, collision.contacts[0].point, ForceMode.Impulse);
            //先销毁碰撞体
            //Destroy(collision.gameObject.GetComponent<Collider>(), 0.1f);
            Destroy(collision.gameObject, 2f);
        }

        //如果打到敌人
        // 命中任何实现了 IDamageable 的对象就造成伤害
        if (collision.gameObject.TryGetComponent<IDamageable>(out var damageable))
        {
            damageable.TakeDamage(2);
        }

        //效果沿着击中点的法线创建
        var go = Instantiate(effectPrefab, transform.position, Quaternion.LookRotation(collision.contacts[0].normal));
        Destroy(go, 1f);
        Destroy(gameObject);
        
    }
}*/
