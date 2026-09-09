using UnityEngine;

public class ExpOrb : MonoBehaviour
{
    public float absorbRange = 3f;
    public float flySpeed = 8f;
    public int expValue = 1;
    public Vector3 velocity;      // 初始弹开速度
    public float settleTime = 0.5f;
    float scatterTimer;

    public float appearTime = 0.2f;   // 渐显时长（秒）
    float appearTimer;                 // 已渐显时间
    Vector3 targetScale;

    Transform player;
    bool absorbing;   // 正在被吸收

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        targetScale = transform.localScale;        
        transform.localScale = targetScale * 0.01f;
    }

    void Update()
    {
        if (player == null) return;   // 找不到玩家就不跑

        // ===== 渐显：0.2 秒内从 0.01 倍放大到正常 =====
        if (appearTimer < appearTime)
        {
            appearTimer += Time.deltaTime;
            float t = Mathf.Clamp01(appearTimer / appearTime);
            float s = 0.01f + (1f - 0.01f) * Mathf.SmoothStep(0f, 1f, t);
            transform.localScale = targetScale * s;
        }

        // 弹开
        if (scatterTimer < settleTime)
        {
            scatterTimer += Time.deltaTime;
            transform.position += velocity * Time.deltaTime;             // 用初速度弹开
            velocity = Vector3.Lerp(velocity, Vector3.zero, 5f * Time.deltaTime);  // 平滑减速
            return;                                                     // 弹开期间不吸附
        }

        //  吸收
        if (absorbing)
        {
            transform.localScale *= 0.9f;   // 每帧缩到 90%
            transform.position = Vector3.MoveTowards(transform.position,
                player.position + Vector3.up * 1f, flySpeed * 4f * Time.deltaTime);
            if (transform.localScale.x < 0.05f) Destroy(gameObject);
            return;
        }

        // 旧的吸附逻辑
        float dist = Vector3.Distance(transform.position, player.position);
        if (dist < absorbRange)
        {
            float speed = flySpeed * (1f + 3f * (1f - dist / absorbRange));
            Vector3 target = player.position + Vector3.up * 1f;
            transform.position = Vector3.MoveTowards(transform.position, target, speed * Time.deltaTime);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            other.GetComponent<PlayerExp>().GainExp(expValue);
            GetComponent<Collider>().enabled = false;   // 防止重复触发加经验
            absorbing = true;                           // 不销毁 先进缩小状态
        }
    }
}