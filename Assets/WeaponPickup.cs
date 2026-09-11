using UnityEngine;

public class WeaponPickup : MonoBehaviour
{
    public Weapon weapon;            // 预制件上挂好的 Weapon
    public KeyCode pickupKey = KeyCode.E;

    PlayerWeaponHolder holder;
    bool inRange;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            inRange = true;
            holder = other.GetComponent<PlayerWeaponHolder>();
        }
    }
    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player")) { inRange = false; holder = null; }
    }

    void Update()
    {
        if (inRange && holder != null && Input.GetKeyDown(pickupKey))
        {
            holder.Equip(weapon);
            Destroy(gameObject);      // 拾取后地上消失
        }
    }

    void OnGUI()
    {
        if (inRange)
            GUI.Label(new Rect(10, Screen.height - 40, 300, 30), "按 E 拾取");
    }
}