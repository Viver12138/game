using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    [Header("相机")]
    public Camera cam;              // 主相机，空则自动获取

    private PlayerWeaponHolder holder;
    private Animator animator;
    private float fireTimer;

    void Start()
    {
        holder = GetComponent<PlayerWeaponHolder>();
        if (animator == null) animator = GetComponent<Animator>();
        if (cam == null) cam = Camera.main;
    }

    void Update()
    {
        if (holder == null || holder.currentWeapon == null || cam == null) return;
        var w = holder.currentWeapon;

        fireTimer += Time.deltaTime;

        // 鼠标左键 → 出手（点一下挥/射一下）
        if (Input.GetMouseButtonDown(0) && fireTimer >= w.cooldown)
        {
            fireTimer = 0;

            // 近战：只触发挥砍动画，伤害交给动画事件 OnAttackHit
            if (w.type == WeaponType.Melee)
            {
                if (animator != null)
                {
                    animator.SetBool("IsAttacking", true);
                    CancelInvoke(nameof(StopAttacking));
                    Invoke(nameof(StopAttacking), 0.5f);   // 挥完退回待机/跑
                }
            }
            // 远程：点击直接朝鼠标方向发弹
            else
            {
                Vector3 aimDir = GetMouseAim();
                if (aimDir.sqrMagnitude < 0.001f) return;
                Vector3 muzzle = transform.position + aimDir * 0.6f + Vector3.up * 1.0f;
                w.Attack(transform, muzzle, aimDir);
            }
        }
    }

    /// <summary>停止攻击动画（退回待机/跑）</summary>
    void StopAttacking()
    {
        if (animator != null) animator.SetBool("IsAttacking", false);
    }

    /// <summary>动画事件回调：剑挥到命中帧时才结算近战伤害</summary>
    public void OnAttackHit()
    {
        if (holder == null || holder.currentWeapon == null) return;
        if (holder.currentWeapon.type == WeaponType.Melee)
            holder.currentWeapon.MeleeHit(transform, GetMouseAim());
    }

    /// <summary>鼠标瞄准方向（投到地面取水平方向）</summary>
    Vector3 GetMouseAim()
    {
        if (cam == null) return transform.forward;
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 200f))
        {
            Vector3 dir = hit.point - transform.position;
            dir.y = 0;
            return dir.normalized;
        }
        return transform.forward;
    }
}