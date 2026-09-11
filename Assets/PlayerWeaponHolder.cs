using UnityEngine;

public class PlayerWeaponHolder : MonoBehaviour
{
    public Weapon currentWeapon;
    public Weapon defaultMeleeWeapon;
    public Transform handSlot;                     // 可留空，会自动填
    public HumanBodyBones handBone = HumanBodyBones.RightHand;   // 默认右手
    public float weaponScale = 1f;                 // 剑在手上的大小
    public float weaponSize = 1f;
    private Animator animator;

    void Start()
    {
        if (animator == null) animator = GetComponentInChildren<Animator>();
        // 只有 Hand Slot 为空才去取手骨；拖了 ArmR 就用 ArmR
        if (handSlot == null && animator != null)
            handSlot = animator.GetBoneTransform(handBone);

        if (defaultMeleeWeapon != null && currentWeapon == null)
            Equip(defaultMeleeWeapon);
    }

    public void Equip(Weapon w)
    {
        currentWeapon = w;

        if (handSlot == null)
        {
            Debug.LogWarning("PlayerWeaponHolder: handSlot 为空，武器不会显示", this);
            return;
        }

        // 清掉手上旧的武器
        foreach (Transform child in handSlot) Destroy(child.gameObject);

        if (w.visualPrefab != null)
        {
            GameObject visual = Instantiate(w.visualPrefab, handSlot);
            visual.transform.localPosition = Vector3.zero;
            visual.transform.localRotation = Quaternion.identity;

            // 关键：用当前挂点的世界缩放反推，让剑在世界恒定大小（父级 0.28 也能补偿回来）
            float parentScale = Mathf.Max(handSlot.lossyScale.x, 0.0001f);
            visual.transform.localScale = Vector3.one * (weaponSize / parentScale);
        }
    }
}