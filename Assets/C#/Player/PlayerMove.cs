using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    [Header("移动")]
    public float speed = 5f;

    [Header("动画")]
    public Animator animator;

    private Vector3 moveDirection;

    void Start()
    {
        if (animator == null) animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (GameManager.Instance.State != GameState.Playing) return;

        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        moveDirection = new Vector3(h, 0, v).normalized;

        if (moveDirection.sqrMagnitude > 0.001f)
        {
            // 移动
            transform.position += moveDirection * speed * Time.deltaTime;
            // 转向：面向移动方向（按键控制转身）
            transform.rotation = Quaternion.Slerp(transform.rotation,
                Quaternion.LookRotation(moveDirection), 10f * Time.deltaTime);
        }

        if (animator != null)
            animator.SetFloat("Speed", moveDirection.sqrMagnitude > 0.001f ? 1f : 0f);
    }

    public bool IsMoving => moveDirection.sqrMagnitude > 0.001f;
}