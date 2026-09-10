using UnityEngine;
using System.Collections.Generic;

public class MapGenerator : MonoBehaviour
{
    [Header("地图范围")]
    public float mapHalfX = 40f;        // 场地半宽
    public float mapHalfZ = 40f;        // 场地半深

    [Header("围墙")]
    public bool generateWall = true;       // 是否建围墙
    public GameObject wallPrefab;
    public float wallHeight = 3f;          // 墙高
    public float wallThickness = 1f;       // 墙厚
    public Material wallMaterial;

    public static float MapHalfX;
    public static float MapHalfZ;

    public static Bounds GroundBounds;

    [Header("地面生成")]
    public bool generateGround = true;  // 是否一起生成地面（默认开）
    public Material groundMaterial;     // 地面材质（可空，空就是默认白/灰）

    [Header("障碍设置")]
    public GameObject[] obstaclePrefabs;   // 想随机出的障碍：树/树干/岩石…
    public int obstacleCount = 50;         // 撒多少个
    public float minSpacing = 5f;          // 障碍之间最小间距
    public float obstacleY = 0f;           // 障碍出生高度
    public float minScale = 0.85f;         // 随机大小下限
    public float maxScale = 1.2f;          // 随机大小上限

    [Header("安全区")]
    public float playerSafeRadius = 12f;   // 玩家出生保护圈
    public float spawnBand = 6f;           // 地图边缘刷怪留白

    List<Vector3> placed = new List<Vector3>();   // 记录已放障碍，防重叠

    void Start()
    {
        MapHalfX = mapHalfX;      
        MapHalfZ = mapHalfZ;

        if (generateGround) SpawnGround();

        if (generateWall) SpawnWalls();

        Transform player = FindPlayer();

        if (obstaclePrefabs == null || obstaclePrefabs.Length == 0)
        {
            Debug.LogWarning("未配置任何障碍 prefab，跳过障碍生成");
            return;
        }

        int ok = 0;
        for (int i = 0; i < obstacleCount; i++)
        {
            for (int tries = 0; tries < 60; tries++)   // 每个障碍最多试 60 个位置
            {
                Vector3 pos = RandomPosition();
                if (!IsValid(pos, player)) continue;   // 四层约束不过就换位置

                // 随机挑一种障碍 × 随机朝向 × 随机大小
                GameObject prefab = obstaclePrefabs[Random.Range(0, obstaclePrefabs.Length)];
                GameObject go = Instantiate(prefab, pos,
                    Quaternion.Euler(0, Random.Range(0f, 360f), 0));
                float s = Random.Range(minScale, maxScale);
                go.transform.localScale *= s;
                go.name = "Obstacle_" + i;

                placed.Add(pos);
                ok++;
                break;
            }
        }
        Debug.Log($"战场生成完成：地面{(generateGround ? "已生成" : "未生成")}，障碍 {ok}/{obstacleCount}");
    }

    // ============ 地面 ============
    void SpawnGround()
    {
        GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        const float baseSize = 10f;                       // 默认 Plane 是 10×10
        float sx = mapHalfX * 2f / baseSize;
        float sz = mapHalfZ * 2f / baseSize;
        ground.transform.localScale = new Vector3(sx, 1, sz);

        ground.transform.position = new Vector3(0, -0.02f, 0);   // 略下沉，防和障碍闪面
        ground.transform.rotation = Quaternion.identity;
        ground.name = "Ground";
        ground.isStatic = true;

        if (groundMaterial != null)
            ground.GetComponent<Renderer>().material = groundMaterial;
        // Plane 自带 MeshCollider，保留 → 经验球/掉落物能落在地面上、不穿模
    }

    // ============ 障碍 ============
    Vector3 RandomPosition()
    {
        // 只在内部取，避开边缘刷怪留白带
        return new Vector3(
            Random.Range(-mapHalfX + spawnBand, mapHalfX - spawnBand),
            obstacleY,
            Random.Range(-mapHalfZ + spawnBand, mapHalfZ - spawnBand));
    }

    bool IsValid(Vector3 pos, Transform player)
    {
        // ① 玩家出生保护圈
        if (player != null && Vector3.Distance(pos, player.position) < playerSafeRadius)
            return false;
        // ② 障碍之间防重叠
        foreach (Vector3 v in placed)
            if (Vector3.Distance(pos, v) < minSpacing)
                return false;
        return true;
    }

    Transform FindPlayer()
    {
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        return p != null ? p.transform : null;
    }

    // ============ 编辑器预览：选中时画地图范围红框 ============
    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1, 0, 0, 0.3f);
        Gizmos.DrawCube(transform.position, new Vector3(mapHalfX * 2, 1, mapHalfZ * 2));
    }

    void SpawnWalls()
    {
        float lenX = mapHalfX * 2f;
        float lenZ = mapHalfZ * 2f;
        float y = wallHeight * 0.5f;                       // 墙体从地面立起

        // 四面墙：沿 X 的南/北边 + 沿 Z 的西/东边（长边各加厚度，让四角重叠封口）
        PlaceWall(new Vector3(0, y, mapHalfZ), new Vector3(lenX + wallThickness, wallHeight, wallThickness)); // 北
        PlaceWall(new Vector3(0, y, -mapHalfZ), new Vector3(lenX + wallThickness, wallHeight, wallThickness)); // 南
        PlaceWall(new Vector3(mapHalfX, y, 0), new Vector3(wallThickness, wallHeight, lenZ + wallThickness)); // 东
        PlaceWall(new Vector3(-mapHalfX, y, 0), new Vector3(wallThickness, wallHeight, lenZ + wallThickness)); // 西
    }

    void PlaceWall(Vector3 pos, Vector3 scale)
    {
        GameObject wall;
        if (wallPrefab != null)
            wall = Instantiate(wallPrefab, pos, Quaternion.identity);
        else
            wall = GameObject.CreatePrimitive(PrimitiveType.Cube);   // 自带 BoxCollider

        wall.transform.localScale = scale;
        wall.name = "Wall";
        wall.isStatic = true;
        if (wallMaterial != null)
            wall.GetComponent<Renderer>().material = wallMaterial;

        // 让墙能挡子弹（前提是你已经建过 "Obstacle" 标签，你之前建了）
        if (wall.tag != "Obstacle")
            wall.tag = "Obstacle";
    }
}