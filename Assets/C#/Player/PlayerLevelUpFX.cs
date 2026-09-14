using UnityEngine;
using DG.Tweening;

public class PlayerLevelUpFX : MonoBehaviour
{
    [Header("粒子")]
    public ParticleSystem levelUpVfx;

    [Header("辉光")]
    public Color glowColor = new Color(1f, 1f, 1f, 1f);  // 白色辉光
    public float glowPeak = 6f;        // Emission 强度峰值（>1 才发光）
    public float glowDuration = 0.8f;  // 总衰减时长
    public float pulseDelay = 0.15f;   // 二次脉冲延迟（0=禁用）

    private Renderer[] renderers;
    private MaterialPropertyBlock block;
    private int emissionId;

    void Awake()
    {
        renderers = GetComponentsInChildren<Renderer>();
        block = new MaterialPropertyBlock();
        emissionId = Shader.PropertyToID("_EmissionColor");
    }

    void Start()
    {
        // 启用每个材质的 _EMISSION 关键字（URP/Lit 必须激活 keyword 才会渲染 Emission）
        if (renderers != null)
        {
            foreach (var r in renderers)
            {
                if (r.sharedMaterial == null) continue;
                var mat = r.sharedMaterial;
                mat.EnableKeyword("_EMISSION");
                if (mat.HasProperty(emissionId))
                {
                    mat.SetColor(emissionId, Color.black);
                }
            }
        }
    }

    public void Play()
    {
        // 1. 粒子爆发
        if (levelUpVfx != null)
        {
            levelUpVfx.Play(true);
        }

        // 2. 辉光：快速冲峰 → 衰减 →（可选）二次脉冲
        if (renderers == null || renderers.Length == 0) return;

        // 阶段一：瞬间冲到峰值 + 衰减到 30%
        SetEmission(glowPeak);
        DOTween.Sequence()
            .Append(DOVirtual.Float(glowPeak, glowPeak * 0.3f, 0.15f, v => SetEmission(v)).SetEase(Ease.OutQuad))
            // 阶段二：衰减到 0（缓慢淡出）
            .Append(DOVirtual.Float(glowPeak * 0.3f, 0f, glowDuration - 0.15f, v => SetEmission(v)).SetEase(Ease.OutQuint))
            // 阶段三：收尾归零
            .AppendCallback(() => SetEmission(0f));

        // 3. 二次脉冲（加强冲击感）
        if (pulseDelay > 0f)
        {
            DOVirtual.DelayedCall(pulseDelay, () =>
            {
                DOTween.Sequence()
                    .Append(DOVirtual.Float(0f, glowPeak * 0.6f, 0.08f, v => SetEmission(v)).SetEase(Ease.OutBack))
                    .Append(DOVirtual.Float(glowPeak * 0.6f, 0f, 0.4f, v => SetEmission(v)).SetEase(Ease.OutQuint))
                    .AppendCallback(() => SetEmission(0f));
            });
        }
    }

    /// <summary>统一设置所有 renderer 的 EmissionColor（强度 × 颜色，alpha 保持 1）</summary>
    private void SetEmission(float intensity)
    {
        Color c = new Color(
            glowColor.r * intensity,
            glowColor.g * intensity,
            glowColor.b * intensity,
            1f
        );

        foreach (var r in renderers)
        {
            if (r == null) continue;

            // 方案 A：MaterialPropertyBlock（不破坏共享材质）
            r.GetPropertyBlock(block);
            block.SetColor(emissionId, c);
            r.SetPropertyBlock(block);

            // 方案 B：直接修改材质实例（保险）
            if (r.sharedMaterial != null && r.sharedMaterial.HasProperty(emissionId))
            {
                r.sharedMaterial.SetColor(emissionId, c);
            }
        }
    }
}
