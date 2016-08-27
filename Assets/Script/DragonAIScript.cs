using UnityEngine;
using System.Collections;
using Kalagaan;

public class DragonAIScript : MonoBehaviour {
    private const int TRAIN_OVERSHOT_SECOND = 1;

    public enum DragonStatus{SLEEP, TAUNT, FLY_UP, FLY_CIRCLE, ATTACK_NEAR, ATTACK_TRAIN, ATTACK_FIRE, ATTACK_FLY, DOWN};
    public enum AttackStatus{FLY_DOWN, READY, ATTACKING, ATTACK_FINISH};
    public DragonStatus dragonStatus = DragonStatus.SLEEP;
    public AttackStatus attackStatus = AttackStatus.FLY_DOWN;
    public PlayerScript player;
    public Transform[] flyCircleTransfrom;
    public DragonController dragonController;
    public Transform playerTransform;
    public Animator dragonAnimator;

    public float moveSpeed = 10f;
    public float rotateSpeed = 2.0f;
    public float flyRadius = 70f;
    public float flyHeight = 25f;

    System.DateTime startStatusTime;
    Vector3 flyCircleRadius = new Vector3(20,0,0);
    float flyCircleHeight;
    float flyCircleCurrentRotation = 0;
    Vector3 velocity = Vector3.zero;
    Vector3 targetPosition;
    float moveSpeedFactor = 1.0f;
    float rotateSpeedFactor = 1.0f;
    float targetRotation;
    bool animationStart;
    int hp = 3;

    //public DragonController controller;
    // Use this for initialization
    void Start () {
        startStatusTime = System.DateTime.Now;
    }

    void Update () {
        switch (dragonStatus) {
        case DragonStatus.SLEEP:
            {
                //TODO: tune sleep time
                if ((System.DateTime.Now - startStatusTime).TotalSeconds < 1) {
                    Debug.Log("Exepction: time is less than start status");
                    break;
                }
                dragonAnimator.SetBool ("Sleep", false);
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
                else if (Mathf.Abs(transform.position.y - targetPosition.y) < 5f)
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
                    moveSpeed = 0.5f;
                    updateStartCirclePoint();
                    dragonAnimator.SetBool("Fly", true);
                    dragonStatus = DragonStatus.FLY_UP;
                }
            }
            break;
        }
    }

    void startAttack()
    {
        int nextAttackWay = Random.Range(0, 4);
        switch (nextAttackWay)
        {
            case 0:
                {
                    //Fire
                    startStatusTime = System.DateTime.Now;
                    dragonController.m_headLook.weight = 1;
                    dragonController.m_fireIntensity = 1;
                    dragonStatus = DragonStatus.ATTACK_FIRE;
                }
                break;
            case 1:
                {
                    // near
                    moveSpeedFactor = 1f;
                    targetPosition = playerTransform.position;
                    // TODO: fine tune land distance
                    targetPosition.y = -5;
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
                    if ((System.DateTime.Now - startStatusTime).TotalSeconds < 5)
                    {
                        break;
                    }
                    dragonController.m_headLook.weight = 0.2f;
                    dragonController.m_fireIntensity = 0;
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
                                    dragonAnimator.SetBool("Fly", false);
                                    dragonAnimator.SetBool("Attack2", true);
                                    animationStart = false;
                                    attackStatus = AttackStatus.ATTACKING;
                                }
                            }
                            break;
                        case AttackStatus.READY:
                            {
                                // TODO: control attack time
                                int attackWay = Random.Range(0, 4);
                                switch (attackWay)
                                {
                                    case 0:
                                        dragonAnimator.SetBool("Attack1", true);
                                        break;
                                    case 1:
                                    case 2:
                                        dragonAnimator.SetBool("Attack2", true);
                                        break;
                                    case 3:
                                        dragonAnimator.SetBool("Fly", true);
                                        updateStartCirclePoint();
                                        moveSpeedFactor = 0.5f;
                                        dragonStatus = DragonStatus.FLY_UP;
                                        return;
                                }
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
                            movePositionToTarget();
                            moveRotationToTarget();
                            if (transform.position.y < 0.2f)
                            {   
                                dragonAnimator.SetBool("Fly", false);
                                dragonAnimator.SetBool("Run", true);
                                attackStatus = AttackStatus.ATTACKING;
                            }
                        }
                        break;
                    case AttackStatus.ATTACKING:
                        {
                            Vector3 targetDirection = (playerTransform.position - transform.position).normalized;
                            Quaternion direction = Quaternion.identity;
                            targetRotation = -Mathf.Atan2(targetDirection.z, targetDirection.x) * Mathf.Rad2Deg + 90;
                                transform.rotation = Quaternion.Euler( 0,targetRotation,0);
                                // TODO: fine tune last turn distance
                            if(Vector3.Distance(transform.position,playerTransform.position) < 10)
                            {
                                startStatusTime = System.DateTime.Now;
                                attackStatus = AttackStatus.ATTACK_FINISH;
                            }
                        }
                        break;
                    case AttackStatus.ATTACK_FINISH:
                        {                            
                            if((System.DateTime.Now - startStatusTime).TotalSeconds > TRAIN_OVERSHOT_SECOND)
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
                                //TODO: fine tune min height
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
        while(d2 < 0)
        {
            d2 += 360;
        }
        if (Mathf.Abs(d1-d2) < 10)
        {
            return true;
        }
        return false;
    }

    void updateStartCirclePoint() {
        // TODO: get far point and random radius and fly height
        flyCircleCurrentRotation = (transform.rotation.eulerAngles.y +270) % 360;
        float positionDeg = (transform.rotation.eulerAngles.y + 90) % 360;
        targetRotation = (transform.rotation.eulerAngles.y + 270) % 360;
        flyCircleHeight = flyHeight;
        flyCircleRadius = new Vector3(flyRadius, 0, 0);

        Quaternion rotation = new Quaternion();
        rotation.eulerAngles = new Vector3(0, flyCircleCurrentRotation, 0);
        Vector3 positionXZ = rotation * flyCircleRadius;
        positionXZ.y = flyCircleHeight;
        targetPosition = positionXZ;
    }

    void moveCircle() {
        flyCircleCurrentRotation--;
        Quaternion rotation = new Quaternion();
        rotation.eulerAngles = new Vector3 (0, flyCircleCurrentRotation, 0);
        Vector3 positionXZ = rotation * flyCircleRadius;
        positionXZ.y = flyCircleHeight;
        transform.position = positionXZ;
        transform.rotation = rotation;
    }

    void movePositionToTarget(){
        Vector3 move = targetPosition - transform.position;
        move.Normalize ();
        Vector3 newPosition = transform.position + move * moveSpeed * moveSpeedFactor;
        if ((targetPosition.x - transform.position.x > 0 && targetPosition.x - newPosition.x < 0)
                || (targetPosition.x - transform.position.x < 0 && targetPosition.x - newPosition.x > 0)) {
            // overshot, set to target directly.
            transform.position = targetPosition;
        } else {
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
        if (dragonStatus == DragonStatus.DOWN || dragonStatus == DragonStatus.FLY_UP)
        {
            return;
        }
        hp--;
        startStatusTime = System.DateTime.Now;
        dragonAnimator.SetBool("Die", true);
        dragonAnimator.SetBool("Run", false);
        dragonAnimator.SetBool("Fly", false);
        dragonStatus = DragonStatus.DOWN;
    }
}
