using UnityEngine;

public class LifeCycleTest : MonoBehaviour
{
    void Awake() { Debug.Log("1 Awake：物体一创建就执行"); }
    void Start() { Debug.Log("2 Start：第一帧之前执行一次"); }
    void Update() { }   // 每帧执行
    void FixedUpdate() { }   // 固定时间间隔执行（物理用）
    void LateUpdate() { }   // 每帧最后执行
    void OnDestroy() { Debug.Log("Destroy：物体销毁时执行"); }
}