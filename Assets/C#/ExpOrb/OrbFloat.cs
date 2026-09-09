using UnityEngine;

public class OrbFloat : MonoBehaviour
{
    public float floatHeight = 0.15f;   // 上下浮动幅度
    public float floatSpeed = 2f;
    public float breathAmount = 0.1f;   // 呼吸缩放幅度
    public float breathSpeed = 3f;

    Vector3 startPos, startScale;

    void Start()
    {
        
        startScale = transform.localScale;
    }

    void Update()
    {
        // 相对父物体上下浮动 
        float y = Mathf.Sin(Time.time * floatSpeed) * floatHeight;
        transform.localPosition = new Vector3(0, y, 0);

        float s = 1f + Mathf.Sin(Time.time * breathSpeed) * breathAmount;
        transform.localScale = startScale * s;
    }
}