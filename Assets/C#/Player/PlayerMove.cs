using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    [Header("攻击设置")]
    public GameObject bulletPrefab;
    public float attackRange = 15f;
    public float attackInterval = 1f;
    float attackTimer;


    [Header("移动设置")]
    public float speed = 5f;

    [Header("动画设置")]
    public Animator animator; // 拖入Animator组件

    private Vector3 moveDirection;
    private bool isAttacking;

    void Start()
    {
        // 如果没有手动赋值，自动获取Animator
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }
    }

    void Update()
    {
        // 游戏结束时不再处理输入（双保险，通常 timeScale=0 已冻结 Update）
        if (GameManager.Instance.State != GameState.Playing) return;

        // 获取输入
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        moveDirection = new Vector3(h, 0, v).normalized;

        // 移动
        if (moveDirection != Vector3.zero)
        {
            transform.position += moveDirection * speed * Time.deltaTime;
            transform.forward = moveDirection;
        }

        // 攻击
        Attack();

        // ===== 更新动画 =====
        UpdateAnimation();
    }

    void Attack()
    {

        attackTimer += Time.deltaTime;
        if (attackTimer < attackInterval)
        {
            isAttacking = false;
            return;
        }

        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        if (enemies.Length == 0)
        {
            isAttacking = false;
            return;
        }

        GameObject nearest = null;
        float minDist = float.MaxValue;
        foreach (GameObject e in enemies)
        {
            float d = Vector3.Distance(transform.position, e.transform.position);
            if (d <= attackRange && d < minDist)
            {
                minDist = d;
                nearest = e;
            }
        }
        if (nearest == null) return;

        // === 新增：朝向敌人 ===
        Vector3 directionToEnemy = (nearest.transform.position - transform.position).normalized;
        if (directionToEnemy != Vector3.zero)
        {
            // 平滑转向敌人（可选）
            Quaternion targetRotation = Quaternion.LookRotation(directionToEnemy);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 10f * Time.deltaTime);
        }

        // 发射子弹
        attackTimer = 0;
        isAttacking = true;

        Vector3 chest = transform.position + directionToEnemy * 0.7f + Vector3.up * 0.3f;
        Vector3 aim = (nearest.transform.position - chest).normalized;

        GameObject bullet = Instantiate(bulletPrefab, chest, Quaternion.identity);
        bullet.GetComponent<Bullet>().dir = aim;
    }

    /// <summary>
    /// 更新动画状态
    /// </summary>
    void UpdateAnimation()
    {
        if (animator == null) return;

        // 1. 速度动画：移动速度
        float currentSpeed = moveDirection.magnitude * speed;
        animator.SetFloat("Speed", currentSpeed);

        // 2. 攻击动画：是否在攻击
        animator.SetBool("IsShooting", isAttacking);

        // 3. 可选：移动方向（用于混合）
        if (moveDirection != Vector3.zero)
        {
            animator.SetFloat("Horizontal", moveDirection.x);
            animator.SetFloat("Vertical", moveDirection.z);
        }
    }

    /// <summary>
    /// 公共方法：重置攻击计时器（外部调用可打断攻击）
    /// </summary>
    public void ResetAttackTimer()
    {
        attackTimer = 0;
        isAttacking = false;
    }
}