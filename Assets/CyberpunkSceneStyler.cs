using UnityEngine;

public class CyberpunkSceneStyler : MonoBehaviour
{
    public static CyberpunkSceneStyler Instance { get; private set; }

    [Header("Shader")]
    public string outlineShaderName = "Shader Graphs/SG_OutlineLit";

    [Header("障碍物颜色")]
    public Color obstacleColor = new Color(0f, 1f, 1f, 1f);
    [Range(0f, 10f)] public float obstacleIntensity = 2f;

    [Header("围墙颜色")]
    public Color wallColor = new Color(1f, 0.2f, 0.5f, 1f);
    [Range(0f, 10f)] public float wallIntensity = 3f;

    [Header("玩家颜色")]
    public Color playerColor = new Color(0f, 1f, 1f, 1f);
    [Range(0f, 10f)] public float playerIntensity = 4f;

    [Header("敌人颜色")]
    public Color enemyColor = new Color(1f, 0f, 1f, 1f);
    [Range(0f, 10f)] public float enemyIntensity = 3f;

    [Header("地面颜色")]
    public Color groundBaseColor = new Color(0.08f, 0.08f, 0.15f, 1f);

    Shader _outlineShader;
    Shader _litShader;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        _outlineShader = Shader.Find(outlineShaderName);
        _litShader = Shader.Find("Universal Render Pipeline/Lit");

        if (_outlineShader == null)
        {
            Debug.LogError($"[CyberpunkSceneStyler] Shader not found: {outlineShaderName}");
            Debug.LogError("请先在 Unity 中打开并保存一次 SG_OutlineLit.shadergraph，让 Unity 编译注册它");
            return;
        }

        MapGenerator.OnMapGenerated += ApplyAfterMapReady;
        StartCoroutine(ApplyDelayed());
    }

    System.Collections.IEnumerator ApplyDelayed()
    {
        yield return null;
        ApplyAll();
    }

    void ApplyAfterMapReady()
    {
        ApplyAll();
    }

    void ApplyAll()
    {
        int total = 0;

        total += ApplyToTag("Obstacle", obstacleColor, obstacleIntensity, false);
        total += ApplyToTag("Player", playerColor, playerIntensity, false);
        total += ApplyToTag("Enemy", enemyColor, enemyIntensity, false);

        ApplyWalls();
        ApplyGround();

        Debug.Log($"[CyberpunkSceneStyler] 完成！共处理 {total} 个 Renderer");
    }

    public void StyleEnemy(GameObject enemy)
    {
        if (enemy == null || _outlineShader == null) return;
        Renderer[] renderers = enemy.GetComponentsInChildren<Renderer>(true);
        foreach (var r in renderers)
            ApplyOutlineMaterial(r, enemyColor, enemyIntensity);
    }

    int ApplyToTag(string tag, Color outlineColor, float intensity, bool includeInactive)
    {
        int count = 0;
        GameObject[] gos = GameObject.FindGameObjectsWithTag(tag);
        foreach (var go in gos)
        {
            Renderer[] renderers = go.GetComponentsInChildren<Renderer>(includeInactive);
            foreach (var r in renderers)
            {
                ApplyOutlineMaterial(r, outlineColor, intensity);
                count++;
            }
        }
        return count;
    }

    void ApplyWalls()
    {
        GameObject[] walls = GameObject.FindGameObjectsWithTag("Obstacle");
        foreach (var w in walls)
        {
            if (w.name.StartsWith("Wall"))
            {
                Renderer r = w.GetComponent<Renderer>();
                if (r != null) ApplyOutlineMaterial(r, wallColor, wallIntensity);
            }
        }
    }

    void ApplyGround()
    {
        GameObject ground = GameObject.Find("Ground");
        if (ground == null) return;

        Renderer r = ground.GetComponent<Renderer>();
        if (r == null || _litShader == null) return;

        Material mat = new Material(_litShader);
        mat.SetColor("_BaseColor", groundBaseColor);
        mat.SetFloat("_Smoothness", 0.3f);
        mat.EnableKeyword("_EMISSION");
        mat.SetColor("_EmissionColor", new Color(0.05f, 0.02f, 0.1f) * 0.5f);
        r.material = mat;
    }

    void ApplyOutlineMaterial(Renderer renderer, Color outlineColor, float intensity)
    {
        Material[] mats = renderer.sharedMaterials;
        Material[] newMats = new Material[mats.Length];

        for (int i = 0; i < mats.Length; i++)
        {
            if (_outlineShader != null)
            {
                Material m = new Material(_outlineShader);
                if (mats[i] != null)
                {
                    if (mats[i].HasProperty("_BaseColor"))
                        m.SetColor("_BaseColor", mats[i].GetColor("_BaseColor"));
                    else if (mats[i].HasProperty("_Color"))
                        m.SetColor("_BaseColor", mats[i].GetColor("_Color"));
                    else
                        m.SetColor("_BaseColor", Color.white);

                    if (mats[i].HasProperty("_BaseMap"))
                    {
                        Texture t = mats[i].GetTexture("_BaseMap");
                        if (t != null) m.SetTexture("_BaseMap", t);
                    }
                    else if (mats[i].HasProperty("_MainTex"))
                    {
                        Texture t = mats[i].GetTexture("_MainTex");
                        if (t != null) m.SetTexture("_BaseMap", t);
                    }

                    if (mats[i].HasProperty("_BumpMap"))
                    {
                        Texture t = mats[i].GetTexture("_BumpMap");
                        if (t != null)
                        {
                            m.SetTexture("_BumpMap", t);
                            m.EnableKeyword("_NORMALMAP");
                        }
                    }
                    if (mats[i].HasProperty("_MetallicMap"))
                    {
                        Texture t = mats[i].GetTexture("_MetallicMap");
                        if (t != null) m.SetTexture("_MetallicMap", t);
                    }
                    if (mats[i].HasProperty("_OcclusionMap"))
                    {
                        Texture t = mats[i].GetTexture("_OcclusionMap");
                        if (t != null) m.SetTexture("_OcclusionMap", t);
                    }

                    if (mats[i].HasProperty("_Smoothness"))
                        m.SetFloat("_Smoothness", mats[i].GetFloat("_Smoothness"));
                    if (mats[i].HasProperty("_Metallic"))
                        m.SetFloat("_Metallic", mats[i].GetFloat("_Metallic"));
                }
                else
                {
                    m.SetColor("_BaseColor", Color.white);
                }
                m.SetColor("_OutlineColor", outlineColor);
                m.SetFloat("_OutlineIntensity", intensity);
                newMats[i] = m;
            }
            else
            {
                newMats[i] = mats[i];
            }
        }

        renderer.sharedMaterials = newMats;
    }
}
