using UnityEngine;
using System.Collections;

public class DragonHandAttackScript : MonoBehaviour {

    public DragonAIScript dragon;
    public PlayerScript player;
    System.DateTime lastHitShieldTime;

    void Start()
    {
        lastHitShieldTime = System.DateTime.Now;
    }

    void OnTriggerEnter(Collider collider)
    {
        if (collider.gameObject.tag == "Shield")
        {
            lastHitShieldTime = System.DateTime.Now;
        }
        else if (collider.gameObject.tag == "Player")
        {
            if (dragon.dragonStatus == DragonAIScript.DragonStatus.ATTACK_NEAR
                && dragon.attackStatus == DragonAIScript.AttackStatus.ATTACKING)
            {
                if ((System.DateTime.Now - lastHitShieldTime).TotalSeconds > 1)
                {
                    player.attackByHand();
                }else
                {
                    player.shieldVibrate();
                }
            }
        }
    }
}
