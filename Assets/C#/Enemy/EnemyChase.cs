using UnityEngine;

[System.Serializable]
public class DropInfo
{
    public int minCount = 1;
    public int maxCount = 3;
    public int expValue = 1;
    public float orbScale = 1f;
    public float scatterForce = 2f;
}


public class EnemyChase : MonoBehaviour
{
    [Header("移动参数")]
    public float speed = 2f;
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
    private bool isAttacking;

    [Header("游荡参数")]
    public bool wanderOnIdle = true;       // 玩家远处时：true=瞎晃 / false=原地站
    public float wanderRadius = 6f;        // 游荡范围
    public float wanderInterval = 3f;      // 每隔多久换个游荡点
    private Vector3 wanderTarget;
    private float lastWanderTime;

    void Start()
    {
        // 获取玩家
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
        else
        {
            Debug.LogWarning("未找到Tag为'Player'的对象！");
        }

        // 获取动画组件
        animator = GetComponent<Animator>();

        wanderTarget = RandomWanderTarget();
    }

    void Update()
    {
        // 死亡检测
        if (hp <= 0)
        {
            Die();
            return;
        }

        // 没有玩家则不执行
        if (player == null) return;

        float distance = Vector3.Distance(transform.position, player.position);

        // ===== 行为决策 =====
        if (distance <= attackRange)
        {
            // 攻击状态
            AttackBehavior();
        }
        else if (distance <= chaseRange)
        {
            // 追逐状态
            ChaseBehavior();
        }
        else
        {
            // 待机 可以瞎晃 也可以原地站
            if (wanderOnIdle) WanderBehavior();
            else IdleBehavior();
        }

        // ===== 更新动画 =====
        UpdateAnimation();
    }

    /// <summary>
    /// 追逐行为
    /// </summary>
    void ChaseBehavior()
    {
        isAttacking = false;
        attackTimer = 0;

        // 到玩家的水平方向（抹平 Y，射线和移动都用它，不会朝上偏）
        Vector3 toPlayer = player.position - transform.position;
        toPlayer.y = 0;

        // ===== 障碍绕行 =====
        RaycastHit hit;
        if (toPlayer.sqrMagnitude > 0.001f &&
            Physics.Raycast(transform.position, toPlayer.normalized, out hit, 1.5f) &&
            hit.collider.CompareTag("Obstacle"))
        {
            // 沿障碍表面切线方向绕（Cross 与 up 叉积即得水平切线）
            Vector3 tangent = Vector3.Cross(toPlayer.normalized, Vector3.up).normalized;
            Vector3 mvDir = Vector3.Dot(tangent, toPlayer) > 0 ? tangent : -tangent;
            transform.position += mvDir * speed * Time.deltaTime;
            return;
        }

        // ===== 正常追击（只有前方没障碍才走到这）=====
        if (toPlayer.sqrMagnitude > 0.001f)
        {
            toPlayer.Normalize();
            Vector3 step = toPlayer * speed * Time.deltaTime;
            if (!MoveBlocked(toPlayer, step.magnitude))
                transform.position += step;

            Quaternion targetRotation = Quaternion.LookRotation(toPlayer);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 5f * Time.deltaTime);
            Vector3 p = transform.position;
            if (MapGenerator.GroundBounds.size.x > 1f)
            {
                p.x = Mathf.Clamp(p.x, MapGenerator.GroundBounds.min.x + 0.5f,
                                       MapGenerator.GroundBounds.max.x - 0.5f);
                p.z = Mathf.Clamp(p.z, MapGenerator.GroundBounds.min.z + 0.5f,
                                       MapGenerator.GroundBounds.max.z - 0.5f);
            }
            transform.position = p;
        }
    }
    /// <summary>
    /// 攻击行为
    /// </summary>
    void AttackBehavior()
    {
        isAttacking = true;

        // 面向玩家
        Vector3 dir = player.position - transform.position;
        dir.y = 0;
        if (dir != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 5f * Time.deltaTime);
        }

        // 攻击冷却
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
        // 可添加随机待机动作（如转头、小幅度移动）
    }

    /// <summary>
    /// 执行攻击
    /// </summary>
    void Attack()
    {
        if (player == null) return;

        // 再次检查距离（防止攻击时玩家已跑远）
        float dist = Vector3.Distance(transform.position, player.position);
        if (dist > attackRange) return;

        PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
        if (playerHealth != null)
        {
            playerHealth.TakeDamage(damage);
            Debug.Log($"敌人攻击玩家，造成 {damage} 点伤害");
        }
    }

    /// <summary>
    /// 更新动画参数
    /// </summary>
    void UpdateAnimation()
    {
        if (animator == null) return;

        // Speed：移动速度（0=待机，>0=移动）
        float currentSpeed = isAttacking ? 0 : speed;
        animator.SetFloat("Speed", currentSpeed);

        // IsAttacking：攻击状态
        animator.SetBool("IsAttacking", isAttacking);
    }

    /// <summary>
    /// 死亡逻辑
    /// </summary>
    void Die()
    {
        // 掉落经验球
        DropExpOrbs();

        // 播放死亡动画（如果有）
        if (animator != null)
        {
            // 可以添加死亡状态
            // animator.SetTrigger("Die");
        }

        // 延迟销毁，让死亡动画播放
        // Destroy(gameObject, 0.5f);
        Destroy(gameObject);
    }

    /// <summary>
    /// 掉落经验球
    /// </summary>
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

    void Awake()
    {
        if (drop == null) drop = new DropInfo();
    }

    /// <summary>
    /// 受击（可被外部调用，如子弹击中）
    /// </summary>
    public void TakeDamage(int damage)
    {
        hp -= damage;
        Debug.Log($"敌人受到 {damage} 点伤害，剩余 HP：{hp}");

        // 播放受击动画（如果有）
        // if (animator != null) animator.SetTrigger("Hurt");

        if (hp <= 0)
        {
            Die();
        }
    }

    // ===== 可视化调试 =====
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, chaseRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }

    void WanderBehavior()
    {
        isAttacking = false;
        attackTimer = 0;

        // 到点/到时间就换个游荡点
        if (Time.time - lastWanderTime >= wanderInterval)
        {
            lastWanderTime = Time.time;
            wanderTarget = RandomWanderTarget();
        }

        Vector3 dir = wanderTarget - transform.position;
        dir.y = 0;                                  // 只在水平面走
        if (dir.magnitude < 0.5f)
        {
            wanderTarget = RandomWanderTarget();    // 到了就换
            return;
        }

        dir.Normalize();
        Vector3 step = dir * speed * 0.6f * Time.deltaTime;
        if (!MoveBlocked(dir, step.magnitude))
            transform.position += step;
        Face(dir);
    }

    bool MoveBlocked(Vector3 dir, float dist)
    {
        // 从脚底上方腰高度发射，避开地面/自己碰撞体底边的缝
        Vector3 origin = transform.position + Vector3.up * 0.5f;

        // 前方那一步 + 0.5 米余量有没有墙/石头，有就提前停，别靠到贴边
        if (Physics.Raycast(origin, dir, out RaycastHit hit, dist + 0.5f) &&
            hit.collider.CompareTag("Obstacle"))
            return true;

        // 兜底：如果当前已经压着障碍（比如之前从没被挡时钻进去了），也视为被挡住
        Collider[] near = Physics.OverlapSphere(transform.position + Vector3.up * 0.5f, 0.3f);
        foreach (Collider c in near)
            if (c.CompareTag("Obstacle"))
                return true;

        return false;
    }

    Vector3 RandomWanderTarget()
    {
        Vector3 t = transform.position +
            new Vector3(Random.Range(-wanderRadius, wanderRadius), 0,
                        Random.Range(-wanderRadius, wanderRadius));
        return ClampToMap(t);                       // 别晃出地图
    }

    // 把位置钳回地图内（追击/游荡共用）
    Vector3 ClampToMap(Vector3 p)
    {
        if (MapGenerator.GroundBounds.size.x > 1f)
        {
            p.x = Mathf.Clamp(p.x, MapGenerator.GroundBounds.min.x + 0.5f,
                                   MapGenerator.GroundBounds.max.x - 0.5f);
            p.z = Mathf.Clamp(p.z, MapGenerator.GroundBounds.min.z + 0.5f,
                                   MapGenerator.GroundBounds.max.z - 0.5f);
        }
        return p;
    }

    void Face(Vector3 horizontalDir)
    {
        if (horizontalDir.sqrMagnitude > 0.001f)
            transform.rotation = Quaternion.Slerp(transform.rotation,
                Quaternion.LookRotation(horizontalDir), 5f * Time.deltaTime);
    }
}