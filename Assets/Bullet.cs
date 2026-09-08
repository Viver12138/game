using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed = 10f;
    public Vector3 dir;   // 发射时由玩家脚本传入方向

    void Update()
    {
        transform.position += dir * speed * Time.deltaTime;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            other.GetComponent<EnemyChase>().hp -= 1;
            Destroy(gameObject);   // 子弹销毁
        }
    }
}