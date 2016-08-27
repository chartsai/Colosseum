using UnityEngine;
using System.Collections;

public class DragonHeadAttackScript : MonoBehaviour {

    public DragonAIScript dragon;
    public PlayerScript player;
    public Transform sword;

    Vector3 lastSwordPosition;
    float swordSpeed;
    System.DateTime lastHitShieldTime;

    void Start()
    {
        lastSwordPosition = sword.position;
    }

    void Update()
    {
        swordSpeed = Vector3.Distance(lastSwordPosition, sword.position) / Time.deltaTime;
        lastSwordPosition = sword.position;
    }

    void OnTriggerEnter(Collider collider)
    {
        print("OnTriggerEnter");
        if(collider.gameObject.tag == "Sword" && swordSpeed >10)
        {
            dragon.headHurt();
        }
        else if(collider.gameObject.tag == "Shield")
        {
            lastHitShieldTime = System.DateTime.Now;
            //TODO:vibrate
        }
        else if(collider.gameObject.tag == "Player")
        {
            print("head hit player");
            if(dragon.dragonStatus == DragonAIScript.DragonStatus.ATTACK_TRAIN
                && dragon.attackStatus == DragonAIScript.AttackStatus.ATTACKING)
            {
                player.attackByTrain();
            }
            else if(dragon.dragonStatus == DragonAIScript.DragonStatus.ATTACK_NEAR)
            {
                 if((System.DateTime.Now - lastHitShieldTime).TotalSeconds > 1)
                {
                    player.attackByHand();
                }
            }
            //TODO: hit player
        }
    }
}
