using UnityEngine;

public enum WeaponType { Melee, Ranged }

public class Weapon : MonoBehaviour
{
    [Header("类型")]
    public WeaponType type = WeaponType.Melee;

    [Header("通用属性")]
    public int damage = 1;
    public float cooldown = 0.6f;      // 点一下挥一下的间隔
    public GameObject visualPrefab;    // 手上/地上显示的武器模型

    [Header("近战")]
    public float attackRange = 2f;     // 正前方攻击范围

    [Header("远程")]
    public GameObject projectilePrefab;
    public float projectileSpeed = 12f;

    public void Attack(Transform owner, Vector3 muzzlePoint, Vector3 aimDir)
    {
        if (type == WeaponType.Melee) MeleeAttack(owner, aimDir);
        else RangedAttack(muzzlePoint, aimDir);
    }

    void MeleeAttack(Transform owner, Vector3 aimDir)
    {
        // 朝 aimDir 方向、以攻击范围中心打（鼠标指哪砍哪）
        Vector3 center = owner.position + aimDir * (attackRange * 0.5f);
        Collider[] hits = Physics.OverlapSphere(center, attackRange);
        foreach (Collider c in hits)
            if (c.CompareTag("Enemy"))
                c.GetComponent<EnemyChase>()?.TakeDamage(damage);
    }

    void RangedAttack(Vector3 muzzle, Vector3 aimDir)
    {
        if (projectilePrefab == null) return;
        GameObject go = Instantiate(projectilePrefab, muzzle, Quaternion.LookRotation(aimDir));
        Bullet b = go.GetComponent<Bullet>();
        if (b != null) { b.damage = damage; b.dir = aimDir; }
    }

    public void MeleeHit(Transform owner, Vector3 aimDir)
    {
        if (type != WeaponType.Melee) return;

        // 按 sender 当前朝向/方向、以攻击范围中心判断
        Vector3 center = owner.position + aimDir * (attackRange * 0.5f);
        Collider[] hits = Physics.OverlapSphere(center, attackRange);
        foreach (Collider c in hits)
            if (c.CompareTag("Enemy"))
                c.GetComponent<EnemyChase>()?.TakeDamage(damage);
    }
}