using UnityEngine;
using System.Collections;
using Kalagaan;

public class DragonAIScript : MonoBehaviour {
    public enum DragonStatus{SLEEP, TAUNT, FLY_UP, FLY_CIRCLE, ATTACK_NEAR, ATTACK_TRAIN, ATTACK_FIRE, ATTACK_FLY, DOWN, DIE};
    public enum AttackStatus{FLY_DOWN, READY, ATTACKING, TAUNT, ATTACK_FINISH };
    public enum RoundDirection{CLOCK_WISE, COUNTER_CLOCK_WISE};
    public DragonStatus dragonStatus = DragonStatus.SLEEP;
    public AttackStatus attackStatus = AttackStatus.FLY_DOWN;
    public RoundDirection direction = RoundDirection.CLOCK_WISE;
    public DragonSoundScript dragonSound;
    public PlayerScript player;
    public DragonController dragonController;
    public Transform playerTransform;
    public Animator dragonAnimator;

    public ParticleSystem dustStorm;
    public ParticleSystem armorBrokenEffect;
    public float moveSpeed = 10f;
    /** The difference of angle when dragon round the player. unit is angle per frame */
    public float roundAngleSpeed = 0.3f;
    public float rotateSpeed = 2.0f;
    public float flyRadius = 70f;
    public float flyHeight = 25f;
    public int nextAttack;

    System.DateTime startStatusTime;
    Vector3 defaultScaled;
    Vector3 flyCircleRadius;
    float flyCircleHeight;
    float flyCircleCurrentRotation = 0;
    Vector3 targetPosition;
    float moveSpeedFactor = 1.0f;
    float rotateSpeedFactor = 1.0f;
    float targetRotation;
    bool animationStart;
    int nearAttackCount;
    int hp = 3;

    //public DragonController controller;
    // Use this for initialization
    void Start () {
        startStatusTime = System.DateTime.Now;
        defaultScaled = transform.localScale;
        dragonAnimator.SetBool("Sleep", true);
        flyCircleRadius = new Vector3(flyRadius, 0, 0);
    }

    void Update () {
        // Scale the size of dragon depends on distance between Dragon and player.
        float distance = Vector3.Distance (playerTransform.position, transform.position);
        float factor = 1f + 0.5f * distance / flyRadius;
        transform.localScale = factor * defaultScaled;

        if (dragonStatus == DragonStatus.SLEEP || dragonStatus == DragonStatus.DIE) {
            dustStorm.enableEmission = false;
        } else {
            // play dust storm effect when dragon is near to the ground.
            dustStorm.enableEmission = transform.position.y <= 50 ? true : false;
        }

        switch (dragonStatus) {
        case DragonStatus.SLEEP:
            {
                //TODO: tune sleep time
                if ((System.DateTime.Now - startStatusTime).TotalSeconds < 1) {
                    Debug.Log("Exepction: time is less than start status");
                    break;
                }
                if ((System.DateTime.Now - startStatusTime).TotalSeconds > 5) {
                    dragonAnimator.SetBool("Sleep", false);
                }
                if (dragonAnimator.GetCurrentAnimatorStateInfo (0).IsName ("Idle_1")) {
                    dragonAnimator.SetBool ("Taunt", true);
                    dragonStatus = DragonStatus.TAUNT;
                }
            }
            break;
        case DragonStatus.TAUNT:
            {
                if (dragonAnimator.GetCurrentAnimatorStateInfo (0).IsName ("Taunt1")) {
                    animationStart = true;
                    dragonAnimator.SetBool ("Taunt", false);
                } else if (animationStart && dragonAnimator.GetCurrentAnimatorStateInfo (0).IsName ("Idle_1")) {
                    animationStart = false;
                    moveSpeedFactor = 0.5f;
                    updateStartCirclePoint();
                    dragonAnimator.SetBool ("Fly", true);
                    dragonStatus = DragonStatus.FLY_UP;
                }
            }
            break;
        case DragonStatus.FLY_UP:
            {
                if (dragonAnimator.GetCurrentAnimatorStateInfo (0).IsName ("Fly_1")
                        || dragonAnimator.GetCurrentAnimatorStateInfo(0).IsName("Fly_3")) {
                    movePositionToTarget ();
                }
                if (checkRotateFinish() && Mathf.Abs(transform.position.y - targetPosition.y) < 0.5f)
                {
                    moveSpeedFactor = 0.25f;
                    startStatusTime = System.DateTime.Now;
                    dragonStatus = DragonStatus.FLY_CIRCLE;
                }
                else if (Mathf.Abs(transform.position.y - targetPosition.y) < 1f)
                {
                    moveRotationToTarget();
                }
                
            }
            break;
        case DragonStatus.FLY_CIRCLE:
            {
                moveCircle ();
                int circleSec = (int)(System.DateTime.Now - startStatusTime).TotalSeconds;
                if (circleSec <= 5) {
                    return;
                }
                int startAttackRandom = Random.Range (0, circleSec - 5);
                if (startAttackRandom == 0) {
                    return;
                }
                startAttack();
            }
            break;
        case DragonStatus.ATTACK_FIRE:
        case DragonStatus.ATTACK_NEAR:
        case DragonStatus.ATTACK_FLY:
        case DragonStatus.ATTACK_TRAIN:
            {
                handleAttack();
            }
            break;
        case DragonStatus.DOWN:
            {
                if (hp <= 0) {
                    return;
                }
                if ((System.DateTime.Now - startStatusTime).TotalSeconds > 5) {
                    dragonAnimator.SetBool("Die", false);
                    moveSpeedFactor = 0.5f;
                    updateStartCirclePoint();
                    dragonAnimator.SetBool("Fly", true);
                    dragonStatus = DragonStatus.FLY_UP;
                }
            }
            break;
        case DragonStatus.DIE:
            {
                // TODO play wining BGM and do some wining animation.
            }
            break;
        }
    }

    void startAttack()
    {
        int nextAttackWay = Random.Range(0, 4);
        if(nextAttack>=0 && nextAttack < 4)
        {
            nextAttackWay = nextAttack;
        }
        switch (nextAttackWay)
        {
            case 0:
                {
                    //Fire
                    startStatusTime = System.DateTime.Now;
                    dragonController.m_headLook.weight = 1;
                    dragonController.m_fireIntensity = 1;
                    dragonSound.startFire();
                    dragonStatus = DragonStatus.ATTACK_FIRE;
                }
                break;
            case 1:
                {
                    // near
                    moveSpeedFactor = 1f;
                    Vector3 diff = playerTransform.position - transform.position;
                    diff.Normalize();
                    targetPosition = playerTransform.position - diff * 55;
                    targetPosition.y = 0;
                    Vector3 targetDirection = (playerTransform.position - transform.position).normalized;
                    Quaternion direction = Quaternion.identity;
                    targetRotation = -Mathf.Atan2(targetDirection.z, targetDirection.x) * Mathf.Rad2Deg + 90;
                    dragonStatus = DragonStatus.ATTACK_NEAR;
                    attackStatus = AttackStatus.FLY_DOWN;
                }
                break;
            case 2:
                {
                    // Fly attack
                    targetPosition = playerTransform.position;
                    Vector3 targetDirection = (playerTransform.position - transform.position).normalized;
                    Quaternion direction = Quaternion.identity;
                    targetRotation = -Mathf.Atan2(targetDirection.z, targetDirection.x) * Mathf.Rad2Deg + 90;
                    dragonStatus = DragonStatus.ATTACK_FLY;
                    attackStatus = AttackStatus.FLY_DOWN;
                }
                break;
            case 3:
                {
                    // train
                    targetPosition = transform.position;
                    targetPosition.y = 0;
                    Vector3 targetDirection = (playerTransform.position - transform.position).normalized;
                    Quaternion direction = Quaternion.identity;
                    targetRotation = -Mathf.Atan2(targetDirection.z, targetDirection.x) * Mathf.Rad2Deg + 90;
                    dragonStatus = DragonStatus.ATTACK_TRAIN;
                    attackStatus = AttackStatus.FLY_DOWN;
                }
                break;
        }
    }

    void handleAttack()
    {
        switch (dragonStatus)
        {
            case DragonStatus.ATTACK_FIRE:
                {
                    moveCircle();
                    player.attackByFire();
                    if ((System.DateTime.Now - startStatusTime).TotalSeconds < 5)
                    {
                        break;
                    }
                    dragonController.m_headLook.weight = 0.2f;
                    dragonController.m_fireIntensity = 0;
                    dragonSound.stopFire();
                    dragonStatus = DragonStatus.FLY_CIRCLE;
                }
                break;
            case DragonStatus.ATTACK_NEAR:
                {
                    Vector3 targetDirection = (playerTransform.position - transform.position).normalized;
                    Quaternion direction = Quaternion.identity;
                    targetRotation = -Mathf.Atan2(targetDirection.z, targetDirection.x) * Mathf.Rad2Deg + 90;
                    moveRotationToTarget();
                    switch (attackStatus)
                    {
                        case AttackStatus.FLY_DOWN:
                            {
                                movePositionToTarget();
                                if (transform.position.y < 0.2f)
                                {
                                    nearAttackCount = 0;
                                    dragonAnimator.SetBool("Fly", false);
                                    attackStatus = AttackStatus.READY;
                                }
                            }
                            break;
                        case AttackStatus.READY:
                            {
                                if(nearAttackCount >= 3)
                                {
                                    if(nearAttackCount >= 5 || Random.Range(0,2) == 0)
                                    {                                        
                                        dragonAnimator.SetBool("Fly", true);
                                        updateStartCirclePoint();
                                        moveSpeedFactor = 0.5f;
                                        dragonAnimator.SetBool("Attack1", false);
                                        dragonAnimator.SetBool("Attack2", false);
                                        dragonStatus = DragonStatus.FLY_UP;
                                        return;
                                    }
                                }
                                int attackWay = Random.Range(0, 2);
                                switch (attackWay)
                                {
                                    case 0:
                                        dragonAnimator.SetBool("Attack1", true);
                                        break;
                                    case 1:
                                        dragonAnimator.SetBool("Attack2", true);
                                        break;
                                }
                                nearAttackCount++;
                                animationStart = false;
                                attackStatus = AttackStatus.ATTACKING;
                            }
                            break;
                        case AttackStatus.ATTACKING:
                            {
                                if (dragonAnimator.GetNextAnimatorStateInfo(0).IsName("Attack_1"))
                                {
                                    animationStart = true;
                                    dragonAnimator.SetBool("Attack1", false);
                                }
                                else if (dragonAnimator.GetNextAnimatorStateInfo(0).IsName("Attack_2"))
                                {
                                    animationStart = true;
                                    dragonAnimator.SetBool("Attack2", false);
                                }
                                else if (animationStart && dragonAnimator.GetNextAnimatorStateInfo(0).IsName("Idle_1"))
                                {
                                    animationStart = false;
                                    attackStatus = AttackStatus.READY;
                                }
                            }
                            break;
                    }                
                }
                break;
            case DragonStatus.ATTACK_TRAIN:
            {
                switch (attackStatus)
                {
                    case AttackStatus.FLY_DOWN:
                        {
                            bool stillMoving = false;
                            if(transform.position.y > 0)
                            {
                                movePositionToTarget();
                                stillMoving = true;
                            }
                            if (!checkRotateFinish())
                            {
                                moveRotationToTarget();
                                stillMoving = true;
                            }
                            if (!stillMoving)
                            {
                                animationStart = false;
                                dragonAnimator.SetBool("Fly", false);
                                dragonAnimator.SetBool("Taunt", true);
                                attackStatus = AttackStatus.TAUNT;
                            }
                        }
                        break;
                    case AttackStatus.TAUNT:
                        {
                            if (dragonAnimator.GetNextAnimatorStateInfo(0).IsName("Taunt1"))
                            {
                                animationStart = true;
                                dragonAnimator.SetBool("Taunt", false);
                            }
                            else if (animationStart && dragonAnimator.GetNextAnimatorStateInfo(0).IsName("Idle_1"))
                            {
                                animationStart = false;
                                dragonAnimator.SetBool("Run", true);
                                attackStatus = AttackStatus.ATTACKING;
                            }
                        }
                        break;
                    case AttackStatus.ATTACKING:
                        {
                            if (Vector3.Distance(transform.position,playerTransform.position) < 35 || Vector3.Distance(transform.position, playerTransform.position) > 100)
                            {
                                attackStatus = AttackStatus.ATTACK_FINISH;
                            }
                        }
                        break;
                    case AttackStatus.ATTACK_FINISH:
                        {
                            // fly up when the overshot distance is larger than the fly radius
                            float distance = Vector3.Distance(transform.position, playerTransform.position);
                            if (distance > flyRadius)
                            {
                                dragonAnimator.SetBool("Run", false);
                                dragonAnimator.SetBool("Fly", true);
                                updateStartCirclePoint();
                                moveSpeedFactor = 0.5f;
                                dragonStatus = DragonStatus.FLY_UP;
                            }
                        }
                        break;
                }
                break;
            }
            case DragonStatus.ATTACK_FLY:
            {
                    switch (attackStatus)
                    {
                        case AttackStatus.FLY_DOWN:
                            {
                                targetPosition = playerTransform.position;
                                targetPosition.y = 3;
                                moveRotationToTarget();
                                if(checkRotateFinish())
                                {
                                    moveSpeedFactor = 2f;
                                    dragonAnimator.SetBool("Fly_plane",true);
                                    attackStatus = AttackStatus.ATTACKING;
                                }
                            }
                            break;
                        case AttackStatus.ATTACKING:
                            {
                                movePositionToTarget();
                                if(transform.position.y < 6)
                                {
                                    moveSpeedFactor = 1f;
                                    dragonAnimator.SetBool("Fly_plane", false);
                                    updateStartCirclePoint();
                                    dragonStatus = DragonStatus.FLY_UP;
                                    player.attackByFly();
                                }
                            }
                            break;
                    }
            }
                break;
        }
    }
    bool checkRotateFinish()
    {
        float d1 = transform.rotation.eulerAngles.y;
        float d2 = targetRotation;
        while (d1 < 0)
        {
            d1 += 360;
        }
        d1 = d1 % 360;
        while(d2 < 0)
        {
            d2 += 360;
        }
        d2 = d2 % 360;
        if (Mathf.Abs(d1-d2) < 10)
        {
            return true;
        }
        return false;
    }

    void updateStartCirclePoint() {
        // TODO: get far point and random radius and fly height
        direction = Random.Range (0, 2) == 0 ? RoundDirection.CLOCK_WISE : RoundDirection.COUNTER_CLOCK_WISE;
        flyCircleCurrentRotation = (transform.rotation.eulerAngles.y +270) % 360;
        float positionDeg = (transform.rotation.eulerAngles.y + 90) % 360;
        targetRotation = transform.rotation.eulerAngles.y + (direction == RoundDirection.CLOCK_WISE ? 90 : -90);
        flyCircleHeight = flyHeight;
        flyCircleRadius = new Vector3(flyRadius, 0, 0);

        Quaternion rotation = new Quaternion();
        rotation.eulerAngles = new Vector3(0, flyCircleCurrentRotation, 0);
        Vector3 positionXZ = rotation * flyCircleRadius;
        positionXZ.y = flyCircleHeight;
        targetPosition = positionXZ;
    }

    void moveCircle() {
        flyCircleCurrentRotation += direction == RoundDirection.CLOCK_WISE ? roundAngleSpeed : -roundAngleSpeed;
        Quaternion rotation = new Quaternion();
        rotation.eulerAngles = new Vector3 (0, flyCircleCurrentRotation, 0);
        Vector3 positionXZ = rotation * flyCircleRadius;
        positionXZ.y = flyCircleHeight;
        transform.position = positionXZ;
        if (direction == RoundDirection.CLOCK_WISE)
        {
            rotation.eulerAngles = new Vector3 (0, flyCircleCurrentRotation + 180, 0);
        }
        transform.rotation = rotation;
    }

    void movePositionToTarget(){
        Vector3 move = targetPosition - transform.position;
        move.Normalize ();
        Vector3 newPosition = transform.position + move * moveSpeed * moveSpeedFactor;
        if ((targetPosition.x - transform.position.x) * (targetPosition.x - newPosition.x) <= 0
                && (targetPosition.y - transform.position.y) * (targetPosition.y - newPosition.y) <= 0
                && (targetPosition.z - transform.position.z) * (targetPosition.z - newPosition.z) <= 0)
        {
            // overshot, set to target directly.
            transform.position = targetPosition;
        }
        else
        {
            transform.position = newPosition;
        }
    }

    void moveRotationToTarget(){
        while(targetRotation < 0)
        {
            targetRotation += 360;
        }
        float str = Mathf.Min (rotateSpeed * rotateSpeedFactor * Time.deltaTime, 1);
        transform.rotation = Quaternion.Lerp (transform.rotation,Quaternion.Euler(0,targetRotation,0), str);
    }

    public void headHurt() {
        if (dragonStatus == DragonStatus.DOWN || dragonStatus == DragonStatus.FLY_UP || dragonStatus == DragonStatus.SLEEP)
        {
            return;
        }
        hp--;
        startStatusTime = System.DateTime.Now;
        dragonAnimator.SetBool("Die", true);
        dragonAnimator.SetBool("Run", false);
        dragonAnimator.SetBool("Fly", false);
        dragonAnimator.SetBool("Attack1", false);
        dragonAnimator.SetBool("Attack2", false);
        dragonStatus = DragonStatus.DOWN;
        if (hp == 2)
        {
            firstBroken();
        }
        else if (hp == 1)
        {
            secondBroken();
        }
        else if (hp <= 0) {
            dragonSound.playDie();
            player.win();
            dragonStatus = DragonStatus.DIE;
        }
    }

    void firstBroken()
    {
        brokenArmor("Armor_Head");
        brokenArmor("Armor_Forearm_L");
        brokenArmor("Armor_Forearm_R");
        brokenArmor("Armor_Thigh_L");
        brokenArmor("Armor_Thigh_R");
    }

    void secondBroken()
    {
        brokenArmor("Armor_Back");
        brokenArmor("Armor_Tail");
        brokenArmor("Armor_Torso");
        brokenArmor("Armor_Wing_L");
        brokenArmor("Armor_Wing_R");
    }

    void brokenArmor(string armorName)
    {
        GameObject obj = GameObject.Find (armorName);
        ParticleSystem ps = Instantiate (armorBrokenEffect);
        ps.transform.position = obj.transform.position;
        ps.Play ();
        Debug.Log ("broken");
        obj.SetActive(false);
    }
}
