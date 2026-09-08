using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public int hp = 100;
    float lastHitTime;
    public float invincibleTime = 1f;

    public void TakeDamage(int damage)
    {
        if (Time.time - lastHitTime < invincibleTime) return;   // 无敌帧
        lastHitTime = Time.time;

        hp -= damage;
        Debug.Log("受伤！剩余血量：" + hp);

        if (hp <= 0) GameManager.Instance.GameOver();
    }
}
