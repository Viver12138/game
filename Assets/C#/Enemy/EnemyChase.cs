using UnityEngine;
using UnityEngine.AI;

[System.Serializable]
public class DropInfo
{
    public int minCount = 1;
    public int maxCount = 3;
    public int expValue = 1;
    public float orbScale = 1f;
    public float scatterForce = 2f;
}

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyChase : MonoBehaviour
{
    [Header("移动参数")]
    public float chaseRange = 10f;      // 开始追逐的距离
    public float attackRange = 2f;      // 攻击距离

    public DropInfo drop;

    [Header("生命参数")]
    public int hp = 3;

    [Header("攻击参数")]
    public int damage = 10;
    public float attackInterval = 1f;
    private float attackTimer;

    [Header("掉落参数")]
    public GameObject expOrbPrefab;
    public int minDrop = 1;
    public int maxDrop = 3;
    public float dropOffset = 0.5f;

    [Header("组件")]
    private Transform player;
    private Animator animator;
    private NavMeshAgent agent;
    private bool isAttacking;
    private bool navReady = false;

    [Header("击杀得分")]
    public int killScore = 10;

    void Awake()
    {
        if (drop == null) drop = new DropInfo();

        agent = GetComponent<NavMeshAgent>();
        agent.enabled = false;   // 等 NavMesh 烘焙好再启用
    }

    void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) player = playerObj.transform;
        else Debug.LogWarning("未找到Tag为'Player'的对象！");

        animator = GetComponent<Animator>();
    }

    void OnEnable()
    {
        MapGenerator.OnMapGenerated += OnNavReady;
        // 运行时刷出的敌人：订阅时事件可能早就触发过，若 NavMesh 已就绪则直接启用
        if (MapGenerator.NavReady) OnNavReady();
    }

    void OnDisable()
    {
        MapGenerator.OnMapGenerated -= OnNavReady;
    }

    /// <summary>
    /// NavMesh 烘焙完成后调用：启用 Agent 并吸附到最近的导航点
    /// </summary>
    void OnNavReady()
    {
        if (NavMesh.SamplePosition(transform.position, out NavMeshHit hit, 5f, NavMesh.AllAreas))
        {
            agent.enabled = true;
            agent.Warp(hit.position);
            navReady = true;
        }
        else
        {
            Debug.LogWarning($"{name} 附近没有 NavMesh，无法启用寻路");
        }
    }

    void Update()
    {
        if (hp <= 0)
        {
            Die();
            return;
        }

        if (!navReady || player == null) return;

        float distance = Vector3.Distance(transform.position, player.position);

        if (distance <= attackRange)
            AttackBehavior();
        else if (distance <= chaseRange)
            ChaseBehavior();
        else
            IdleBehavior();

        UpdateAnimation();
    }

    /// <summary>
    /// 追逐行为：交给 NavMeshAgent
    /// </summary>
    void ChaseBehavior()
    {
        isAttacking = false;
        attackTimer = 0;

        if (agent.isOnNavMesh)
            agent.SetDestination(player.position);
    }

    /// <summary>
    /// 攻击行为
    /// </summary>
    void AttackBehavior()
    {
        isAttacking = true;

        // 停下并面向玩家
        if (agent.isOnNavMesh) agent.ResetPath();

        Vector3 dir = player.position - transform.position;
        dir.y = 0;
        if (dir != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 5f * Time.deltaTime);
        }

        attackTimer += Time.deltaTime;
        if (attackTimer >= attackInterval)
        {
            attackTimer = 0;
            Attack();
        }
    }

    /// <summary>
    /// 待机行为
    /// </summary>
    void IdleBehavior()
    {
        isAttacking = false;
        attackTimer = 0;

        if (agent.isOnNavMesh) agent.ResetPath();
    }

    void Attack()
    {
        if (player == null) return;

        float dist = Vector3.Distance(transform.position, player.position);
        if (dist > attackRange) return;

        PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
        if (playerHealth != null)
        {
            playerHealth.TakeDamage(damage);
            Debug.Log($"敌人攻击玩家，造成 {damage} 点伤害");
        }
    }

    void UpdateAnimation()
    {
        if (animator == null) return;

        // 用 Agent 的实际速度驱动动画，比固定 speed 更准
        float currentSpeed = (agent.enabled && agent.isOnNavMesh) ? agent.velocity.magnitude : 0f;
        if (isAttacking) currentSpeed = 0f;

        animator.SetFloat("Speed", currentSpeed);
        animator.SetBool("IsAttacking", isAttacking);
    }

    void Die()
    {
        DropExpOrbs();
        if (GameManager.Instance != null)
            GameManager.Instance.AddScore(killScore);

        Destroy(gameObject);
    }

    void DropExpOrbs()
    {
        if (expOrbPrefab == null) return;

        int count = Random.Range(minDrop, maxDrop + 1);

        for (int i = 0; i < count; i++)
        {
            Vector3 offset = Random.insideUnitSphere * dropOffset;
            offset.y = 0;
            GameObject go = Instantiate(expOrbPrefab, transform.position + offset, Quaternion.identity);
            ExpOrb orb = go.GetComponent<ExpOrb>();
            if (orb == null) continue;
            Vector3 scatter = Random.insideUnitSphere * drop.scatterForce;
            scatter.y = Mathf.Abs(scatter.y) * 0.6f + 0.3f;
            orb.velocity = scatter;
        }
        Debug.Log($"敌人死亡，掉落 {count} 个经验球");
    }

    public void TakeDamage(int damage)
    {
        hp -= damage;
        Debug.Log($"敌人受到 {damage} 点伤害，剩余 HP：{hp}");
        if (hp <= 0)
        {
            Die();
            GameManager.Instance.AddScore(10);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, chaseRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}