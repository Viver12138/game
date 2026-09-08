using UnityEngine;

public class ExpOrb : MonoBehaviour
{
    public float absorbRange = 3f;
    public float flySpeed = 8f;
    Transform player;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    void Update()
    {
        float dist = Vector3.Distance(transform.position, player.position);
        if (dist < absorbRange)
        {
            // MoveTowards = 朝目标匀速移动，不会过头
            transform.position = Vector3.MoveTowards(
                transform.position, player.position, flySpeed * Time.deltaTime);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            other.GetComponent<PlayerExp>().GainExp(1);
            Destroy(gameObject);
        }
    }
}