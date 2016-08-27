using UnityEngine;
using System.Collections;
using Kalagaan;

public class DragonAIScript : MonoBehaviour {
	public enum DragonStatus{SLEEP,TAUNT,FLY_UP,FLY_CIRCLE,ATTACK_NEAR,ATTACK_TRAIN,ATTACK_FIRE,ATTACK_FLY,DOWN};
	public enum AttackStatus{FLY_DOWN,READY,ATTACKING};
	public Transform[] flyCircleTransfrom;
	public DragonController dragonController;
	public Transform playerTransform;
	public Animator dragonAnimator;

	System.DateTime startStatusTime;
	DragonStatus dragonStatus = DragonStatus.SLEEP;
	AttackStatus attackStatus = AttackStatus.FLY_DOWN;
	Quaternion rotation = new Quaternion();
	Vector3 radius = new Vector3(20,0,0);
	float currentRotation = 0;
	Vector3 velocity = Vector3.zero;
	Vector3 targetPosition;
	float targetRotation;
	float speed;
	bool animationStart;

	//public DragonController controller;
	// Use this for initialization
	void Start () {
		startStatusTime = System.DateTime.Now;
	}

	void Update () {
		switch (dragonStatus) {
		case DragonStatus.SLEEP:
			{
				//TODO: wait
				if ((System.DateTime.Now - startStatusTime).TotalSeconds < 1) {
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
					speed = 0.25f;
					updateStartCirclePoint();
					dragonAnimator.SetBool ("Fly", true);
					dragonStatus = DragonStatus.FLY_UP;
				}
			}
			break;
		case DragonStatus.FLY_UP:
			{
				if (dragonAnimator.GetCurrentAnimatorStateInfo (0).IsName ("Fly_1")) {
					movePositionToTarget ();
					moveRotationToTarget ();
				}
				if (transform.position.y > 14) {
					startStatusTime = System.DateTime.Now;
					dragonStatus = DragonStatus.FLY_CIRCLE;
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
				int startAttack = Random.Range (0, circleSec - 5);
				if (startAttack == 0) {
					return;
				}
				switch (Random.Range (0, 4)) {
				case 0:
					{
						startStatusTime = System.DateTime.Now;
						dragonController.m_headLook.weight = 1;
						dragonController.m_fireIntensity = 1;
						dragonStatus = DragonStatus.ATTACK_FIRE;
					}
					break;
				case 1:
					{
						targetPosition = playerTransform.position;
						targetPosition.y = -10;
						Vector3 targetDirection = (playerTransform.position - transform.position).normalized;
						Quaternion direction = Quaternion.identity;
						targetRotation = -Mathf.Atan2 (targetDirection.z, targetDirection.x) * Mathf.Rad2Deg + 90;
						dragonStatus = DragonStatus.ATTACK_NEAR;
						attackStatus = AttackStatus.FLY_DOWN;
					}
					break;
				case 2:
					{
						//dragonStatus = DragonStatus.ATTACK_FLY;
						break;
					}
				case 3:
					{
						//dragonStatus = DragonStatus.ATTACK_TRAIN;
						break;
					}
				}
				break;
			}
		case DragonStatus.ATTACK_FIRE:
			{
				moveCircle();
				if ((System.DateTime.Now - startStatusTime).TotalSeconds < 5) {
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
				targetRotation = -Mathf.Atan2 (targetDirection.z, targetDirection.x) * Mathf.Rad2Deg + 90;
				moveRotationToTarget ();
				switch (attackStatus) {
				case AttackStatus.FLY_DOWN:
					{
						movePositionToTarget ();
						if (transform.position.y < 0.2f) {
							dragonAnimator.SetBool ("Fly",false);
							dragonAnimator.SetBool ("Attack2",true);
							animationStart = false;
							attackStatus = AttackStatus.ATTACKING;
						}
					}
					break;
				case AttackStatus.READY:
					{
						//Todo:wait
						int attackWay = Random.Range (0, 4);
						switch (attackWay) {
						case 0:
							dragonAnimator.SetBool ("Attack1",true);
							break;
						case 1:
						case 2:
							dragonAnimator.SetBool ("Attack2",true);
							break;
						case 3:
							dragonAnimator.SetBool ("Fly", true);
							updateStartCirclePoint ();
							dragonStatus = DragonStatus.FLY_UP;
							return;
						}
						animationStart = false;
						attackStatus = AttackStatus.ATTACKING;
					}
					break;
				case AttackStatus.ATTACKING:
					{
						if (dragonAnimator.GetNextAnimatorStateInfo (0).IsName ("Attack_1")) {
							animationStart = true;
							dragonAnimator.SetBool ("Attack1", false);
						} else if (dragonAnimator.GetNextAnimatorStateInfo (0).IsName ("Attack_2")) {
							animationStart = true;
							dragonAnimator.SetBool ("Attack2", false);
						} else if (animationStart && dragonAnimator.GetNextAnimatorStateInfo (0).IsName ("Idle_1")) {
							animationStart = false;
							attackStatus = AttackStatus.READY;
						}
					}
					break;
				}
			}
			break;
		}
	}

	void updateStartCirclePoint(){
		currentRotation = 0;
		targetPosition = new Vector3 (20, 20, 0);
		Vector3 targetDirection = (playerTransform.position - transform.position).normalized;
		Quaternion direction = Quaternion.identity;
		targetRotation = -Mathf.Atan2 (targetDirection.z, targetDirection.x) * Mathf.Rad2Deg - 180;
	}

	void moveCircle(){
		currentRotation--;
		rotation.eulerAngles = new Vector3 (0, currentRotation, 0);
		Vector3 positionXZ = rotation * radius;
		positionXZ.y = 15;
		transform.position = positionXZ;
		transform.rotation = rotation;
	}

	void movePositionToTarget(){
		Vector3 move = targetPosition - transform.position;
		move.Normalize ();
		move = move * speed;
		transform.position = transform.position + move;
	}

	void moveRotationToTarget(){
		float str = Mathf.Min (2f * Time.deltaTime, 1);
		transform.rotation = Quaternion.Lerp (transform.rotation,Quaternion.Euler(0,targetRotation,0), str);
	}
}
