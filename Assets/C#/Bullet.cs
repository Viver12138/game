using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed = 10f;
    public Vector3 dir;                 // 发射时由玩家传入方向
    public float checkPadding = 0.05f;  // 前方探测余量

    void Update()
    {
        float step = speed * Time.deltaTime;

        // 前方射线探测障碍：只有障碍顶部 >= 子弹高度才挡，否则子弹越过去
        if (Physics.Raycast(transform.position, dir, out RaycastHit hit, step + checkPadding))
        {
            if (hit.collider.CompareTag("Obstacle") &&
                hit.collider.bounds.max.y >= transform.position.y)
            {
                Destroy(gameObject);   // 够高的墙 → 子弹消失
                return;
            }
            // 障碍顶部低于子弹高度 → 不挡，子弹继续越过去
        }

        transform.position += dir * step;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            var chase = other.GetComponent<EnemyChase>();
            if (chase != null) { chase.hp -= 1; }

            Destroy(gameObject);
        }
        // 注意：障碍不再在 OnTriggerEnter 里处理，改由 Update 的射线判定
    }
}