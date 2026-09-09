using UnityEngine;

public class ExpOrb : MonoBehaviour
{
    public float absorbRange = 3f;
    public float flySpeed = 8f;
    public int expValue = 1;
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
            float speed = flySpeed * (1f + 3f * (1f - dist / absorbRange));
            Vector3 target = player.position + Vector3.up * 1f;
            transform.position = Vector3.MoveTowards(transform.position, target, speed * Time.deltaTime);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            other.GetComponent<PlayerExp>().GainExp(expValue);
            Destroy(gameObject);
        }
    }
}