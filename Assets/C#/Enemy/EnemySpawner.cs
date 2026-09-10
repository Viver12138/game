using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public GameObject enemyPrefab;
    public float interval = 3f;


    [Header("避让")]
    public float minPlayerDist = 15f;       // 别刷在玩家脸上
    public float obstacleAvoidRadius = 1.5f;// 别刷进树/石头里
    public int maxTries = 30;

    float timer;

    void Update()
    {
        if (GameManager.Instance.State != GameState.Playing) return;
        timer += Time.deltaTime;
        if (timer >= interval)
        {
            timer = 0;
            SpawnEnemy();
        }
    }

    void SpawnEnemy()
    {
        if (enemyPrefab == null) return;

        float hx = MapGenerator.MapHalfX;
        float hz = MapGenerator.MapHalfZ;

        Transform player = null;
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;

        // 拒绝采样：随机试位置，直到满足约束
        for (int tries = 0; tries < maxTries; tries++)
        {
            Vector3 pos = new Vector3(Random.Range(-hx, hx), 50f, Random.Range(-hz, hz));  // 从高处往下找地面
            if (Physics.Raycast(pos, Vector3.down, out RaycastHit ground, 100f))
                pos.y = ground.point.y;            // 把脚落在地面
            else
                pos.y = 0f;

            // ① 别离玩家太近（只看水平距离）
            if (player != null)
            {
                Vector3 toPlayer = pos - player.position;
                toPlayer.y = 0;
                if (toPlayer.magnitude < minPlayerDist) continue;
            }

            // ② 别刷进障碍里
            Collider[] hits = Physics.OverlapSphere(pos, obstacleAvoidRadius);
            bool blocked = false;
            foreach (Collider c in hits)
                if (c.CompareTag("Obstacle")) { blocked = true; break; }
            if (blocked) continue;

            Instantiate(enemyPrefab, pos, Quaternion.identity);
            return;   // 生成一个就结束，等下个计时
        }
        // 试了 30 次都没有好位置 → 本次放弃，下个 interval 再刷
    }
}