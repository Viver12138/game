using UnityEngine;

public class ExpOrb : MonoBehaviour
{
    public float absorbRange = 3f;
    public float flySpeed = 8f;
    public int expValue = 1;
    Transform player;
    bool absorbing;   // 正在被吸收

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    void Update()
    {
        if (player == null) return;   // 找不到玩家就不跑

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