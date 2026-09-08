using UnityEngine;

public class EnemyChase : MonoBehaviour
{
    public float speed = 2f;
    public int hp = 3;
    Transform player;

    public float attackRange = 2f;      // 距玩家小于这个值开始攻击
    public float attackInterval = 1f;   // 攻击冷却（秒）
    public int damage = 10;             // 每次伤害
    float attackTimer;

    public GameObject expOrbPrefab;  // 经验球预制件
    public int dropCount = 1;        // 掉几个球
    public float dropOffset = 0.5f;  //经验球范围

    void Start()
    {
        // 通过 Tag 找到玩家，玩家的 Tag 要手动设为 Player
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    void Update()
    {
        if (hp <= 0)
        {
            DropExpOrbs();
            Destroy(gameObject);
            return;
        }
        Vector3 dir = (player.position - transform.position).normalized;
        transform.position += dir * speed * Time.deltaTime;

        float dist = Vector3.Distance(transform.position, player.position);
        if (dist <= attackRange)
        {
            attackTimer += Time.deltaTime;
            if (attackTimer >= attackInterval)
            {
                attackTimer = 0;
                Attack();
            }
        }

    }

    void Attack()
    {
        player.GetComponent<PlayerHealth>().TakeDamage(damage);

    }

    void DropExpOrbs()
    {
        if (expOrbPrefab == null) return;
        for (int i = 0; i < dropCount; i++)
        {
            Vector3 offset = Random.insideUnitSphere * dropOffset;
            offset.y = 0;                    
            Instantiate(expOrbPrefab, transform.position + offset, Quaternion.identity);
        }
    }
}