using UnityEngine;

public class PlayerExp : MonoBehaviour
{
    public int exp = 0;
    public int level = 1;
    public int expToNext = 5;    // 升级所需经验

    public void GainExp(int amount)
    {
        exp += amount;
        Debug.Log("获得经验 +" + amount + "，当前经验：" + exp + " / " + expToNext);  
        while (exp >= expToNext)      // 一次可能连升多级
        {
            exp -= expToNext;         
            level++;
            expToNext += 3;
            GetComponent<PlayerMove>().speed += 0.5f;
            Debug.Log("升级了！当前等级：" + level);
        }
    }
}