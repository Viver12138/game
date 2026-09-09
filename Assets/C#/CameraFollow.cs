using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform player;                        // 把玩家拖进这个槽
    public Vector3 offset = new Vector3(0, 12, -8); // 相机在玩家上方12、后方8

    void LateUpdate()
    {
        transform.position = player.position + offset;
        transform.LookAt(player);   // 相机始终看向玩家
    }
}