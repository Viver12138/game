using UnityEngine;

public class PlayerExp : MonoBehaviour
{
    public int exp = 0;
    public int level = 1;
    public int expToNext = 5;    // 升级所需经验

    public void GainExp(int amount)
    {
        exp += amount;
        Debug.Log("获得经验 +" + amount + "，当前经验：" + exp + " / " + expToNext);   // 每次吸收都提示
        if (exp >= expToNext)
        {
            exp = 0;
            level++;
            expToNext += 3;       // 越升越难
            GetComponent<PlayerMove>().speed += 0.5f;   // 升级奖励：移速+
            Debug.Log("升级了！当前等级：" + level);
        }
    }
}