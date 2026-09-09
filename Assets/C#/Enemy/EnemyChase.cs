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
            // 待机状态
            IdleBehavior();
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

        // 解决追击时会往天上飞的问题
        Vector3 dir = player.position - transform.position;
        dir.y = 0;                                // 只在水平面追
        if (dir.sqrMagnitude > 0.001f) dir.Normalize();
        transform.position += dir * speed * Time.deltaTime;
        // 面向玩家
        if (dir != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 5f * Time.deltaTime);
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
            Instantiate(expOrbPrefab, transform.position + offset, Quaternion.identity);
            Vector3 scatter = Random.insideUnitSphere * drop.scatterForce;
            scatter.y = Mathf.Abs(scatter.y) * 0.6f + 0.3f;
            orb.velocity = scatter;
        }
        Debug.Log($"敌人死亡，掉落 {count} 个经验球");
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
}