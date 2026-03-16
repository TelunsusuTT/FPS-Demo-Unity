using System.Collections.Generic;
using UnityEngine;

public class ObjectPoolManager : MonoBehaviour
{
    //单例化 - 让任何脚本都能通过ObjectPoolManager.Instance 访问对象池
    public static ObjectPoolManager Instance { get; private set; }

    //Unity Inspector 配置项
    [System.Serializable]
    public class PoolConfig
    {
        public GameObject prefab;
        public int preloadCount = 30;
        public bool canExpand = true;
    }

    public List<PoolConfig> pools = new List<PoolConfig>();

    //都是用一个key：prefabID
    private readonly Dictionary<int, Queue<GameObject>> poolDict = new(); //池队列
    private readonly Dictionary<int, GameObject> prefabDict = new(); //prefab的本体对应什么
    private readonly Dictionary<int, Transform> parentDict = new(); //这个池在Hierachy中的父节点
    private readonly Dictionary<int, bool> expandDict = new(); //是否允许扩容


    private void Awake()
    {
        //确保全局唯一PoolManager实例
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        //初始化所有池
        foreach (var cfg in pools)
        {
            if (cfg.prefab == null) continue;
            RegisterPool(cfg.prefab, cfg.preloadCount, cfg.canExpand);
        }
    }

    //为某个prefab建一个池
    private void RegisterPool(GameObject prefab, int preload, bool canExpand)
    {
        //生成prefabID，且避免重复注册
        int id = prefab.GetInstanceID(); 
        if (poolDict.ContainsKey(id)) return;

        //初始化字典
        poolDict[id] = new Queue<GameObject>(preload);
        prefabDict[id] = prefab;
        expandDict[id] = canExpand;

        //创建一个父节点存储同类型prefab
        var parent = new GameObject($"Pool_{prefab.name}").transform;
        parent.SetParent(transform);
        parentDict[id] = parent;

        //预热：Instantiate preload个对象并入队 - 此时状态都设置为inactive！
        for (int i = 0; i < preload; i++)
        {
            var obj = CreateNew(id);
            obj.SetActive(false);
            poolDict[id].Enqueue(obj);
        }
    }

    //真正instantiate一个实例
    private GameObject CreateNew(int prefabId)
    {
        var prefab = prefabDict[prefabId];
        var obj = Instantiate(prefab, parentDict[prefabId]);
        
        //加上PooledObject组件，记录来源prefabId
        //在Despawn的时候可以通过prefabID才能知道是哪个池的对象，才能放到正确的队列
        var po = obj.GetComponent<PooledObject>();
        if (po == null) po = obj.AddComponent<PooledObject>();
        po.SourcePrefabId = prefabId;

        obj.name = prefab.name + "_Pooled";
        return obj;
    }

    //取对象出来用
    public GameObject Spawn(GameObject prefab, Vector3 pos, Quaternion rot)
    {
        if (prefab == null) return null;

        int id = prefab.GetInstanceID();
        if (!poolDict.ContainsKey(id))
        {
            // 如果你忘了在 inspector 配，也会自动注册（不预热）
            RegisterPool(prefab, 0, true);
        }

        //从队列里取出一个
        GameObject obj;
        if (poolDict[id].Count > 0)
        {
            obj = poolDict[id].Dequeue();
        }
        else
        {
            //如果没有剩的了，试图扩容，取决于有没有开expand选项
            if (!expandDict[id]) return null;
            obj = CreateNew(id);
        }

        //设置位置+激活选装
        obj.transform.SetPositionAndRotation(pos, rot);
        obj.SetActive(true); // 这里会触发子弹的 OnEnable（关键）
        return obj;
    }

    //归还对象
    public void Despawn(GameObject obj)
    {
        if (obj == null) return;
        //读取PooledObjec找回来源池
        var po = obj.GetComponent<PooledObject>();
        if (po == null)
        {
            Destroy(obj);
            return;
        }

        int id = po.SourcePrefabId;
        if (!poolDict.ContainsKey(id))
        {
            Destroy(obj);
            return;
        }

        obj.SetActive(false);

        //移回池中父节点+入队
        obj.transform.SetParent(parentDict[id], false);
        poolDict[id].Enqueue(obj);
    }
}