using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    enum State { Patrol, Chase, Attack, Dead }

    [Header("Refs")]
    public Transform player;                 // 拖 Player 的 Transform
    public NavMeshAgent agent;              // 拖同物体上的 NavMeshAgent
    public Animator animator;               // 敌人 Animator（只有 walk 也行）
    public EnemyController health;          // 拖 EnemyController

    [Header("Patrol")]
    public float patrolRadius = 8f;
    public float patrolWait = 1.0f;

    [Header("Detect")]
    public float detectRadius = 10f;        // 进入这个范围就追
    public float loseRadius = 16f;          // 离开这个更大范围就放弃追
    public float fovAngle = 120f;           // 扇形视野角度（可选）
    public bool useFOV = true;
    public LayerMask losMask = ~0;          // 视线检测用（可选）

    [Header("Attack")]
    public float attackRange = 1.6f;
    public float attackCooldown = 1.0f;

    State state = State.Patrol;
    Vector3 spawnPos;
    float patrolTimer = 0f;
    float attackTimer = 0f;

    void Awake()
    {
        if (agent == null) agent = GetComponent<NavMeshAgent>();
        if (animator == null) animator = GetComponentInChildren<Animator>();
        if (health == null) health = GetComponent<EnemyController>();
        spawnPos = transform.position;
    }

    void OnEnable()
    {
        if (health != null) health.Died += OnDied;
    }

    void OnDisable()
    {
        if (health != null) health.Died -= OnDied;
    }

    void Update()
    {
        if (state == State.Dead) return;
        if (player == null || agent == null) return;

        attackTimer -= Time.deltaTime;

        switch (state)
        {
            case State.Patrol:
                UpdatePatrol();
                if (CanSeePlayer()) state = State.Chase;
                break;

            case State.Chase:
                UpdateChase();
                break;

            case State.Attack:
                UpdateAttack();
                break;
        }

        UpdateAnim();
    }

    void UpdatePatrol()
    {
        // 如果没在走，等一会再找新点
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            patrolTimer += Time.deltaTime;
            if (patrolTimer >= patrolWait)
            {
                patrolTimer = 0f;
                SetRandomPatrolPoint();
            }
        }
    }

    void UpdateChase()
    {
        float dist = Vector3.Distance(transform.position, player.position);

        // 超出丢失范围 -> 回巡逻
        if (dist > loseRadius)
        {
            state = State.Patrol;
            SetRandomPatrolPoint();
            return;
        }

        // 进入攻击范围 -> Attack
        if (dist <= attackRange)
        {
            state = State.Attack;
            agent.ResetPath();
            return;
        }

        // 追踪玩家
        agent.SetDestination(player.position);
    }

    void UpdateAttack()
    {
        // 面向玩家
        Vector3 toPlayer = (player.position - transform.position);
        toPlayer.y = 0f;
        if (toPlayer.sqrMagnitude > 0.001f)
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(toPlayer), 12f * Time.deltaTime);

        float dist = Vector3.Distance(transform.position, player.position);

        // 玩家跑开 -> 继续追
        if (dist > attackRange + 0.5f)
        {
            state = State.Chase;
            return;
        }

        // 最简单攻击：冷却到了就“碰到我你就死”
        if (attackTimer <= 0f)
        {
            attackTimer = attackCooldown;
            Debug.Log("Enemy Attack! (TODO: kill player)");

            // 如果你想立刻判玩家死亡：先只 log，后面再接 PlayerHealth
        }
    }

    bool CanSeePlayer()
    {
        float dist = Vector3.Distance(transform.position, player.position);
        if (dist > detectRadius) return false;

        if (useFOV)
        {
            Vector3 toPlayer = (player.position - transform.position);
            toPlayer.y = 0f;
            float angle = Vector3.Angle(transform.forward, toPlayer);
            if (angle > fovAngle * 0.5f) return false;
        }

        // 可选：视线遮挡检测（简单 ray）
        Vector3 eye = transform.position + Vector3.up * 1.2f;
        Vector3 target = player.position + Vector3.up * 1.2f;
        if (Physics.Linecast(eye, target, out RaycastHit hit, losMask, QueryTriggerInteraction.Ignore))
        {
            // 如果第一撞到的不是玩家，说明被墙挡住
            if (hit.transform != player) return false;
        }

        return true;
    }

    void SetRandomPatrolPoint()
    {
        Vector2 r = Random.insideUnitCircle * patrolRadius;
        Vector3 candidate = spawnPos + new Vector3(r.x, 0f, r.y);

        if (NavMesh.SamplePosition(candidate, out NavMeshHit hit, 2.0f, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }
    }

    void UpdateAnim()
    {
        if (animator == null) return;

       
    }

    void OnDied()
    {
        state = State.Dead;

        // 停止导航
        if (agent != null)
        {
            agent.ResetPath();
            agent.isStopped = true;
            agent.enabled = false;
        }

        // 禁用碰撞（防止尸体继续挡路/触发）
        var col = GetComponent<Collider>();
        if (col != null) col.enabled = false;

        Debug.Log("Enemy Dead");
    }
}
