using UnityEngine;

public class OrbSpin : MonoBehaviour
{
    public float speed = 60f;
    void Update() => transform.Rotate(0, speed * Time.deltaTime, 0);
}