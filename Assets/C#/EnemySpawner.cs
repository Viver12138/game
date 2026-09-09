using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public GameObject enemyPrefab;  // 把敌人预制件拖进来
    public float interval = 3f;     // 每 3 秒刷一个
    float timer;

    void Update()
    {
        // 游戏结束时停止刷怪（双保险，通常 timeScale=0 已冻结 Update）
        if (GameManager.Instance.State != GameState.Playing) return;

        timer += Time.deltaTime;
        if (timer >= interval)
        {
            timer = 0;
            Vector3 pos = new Vector3(Random.Range(-20f, 20f), 1f, 20f);
            Instantiate(enemyPrefab, pos, Quaternion.identity);
        }
    }
}